using Dotnetable.Application;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Sms;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dotnetable.Tests.Services;

/// <summary>
/// SMS stays on the sending website's own gateway. Email (and WhatsApp) fall back to website 1 when a
/// site never configured its own, because a dropped notification is worse than a shared sender; a text
/// message is different — it goes out under the panel owner's sender line and billing, so borrowing
/// another site's gateway is never right.
/// </summary>
public class SmsSenderScopeTests : IDisposable
{
    private const int WebsiteId = 2;

    private readonly AppDbContext _context;
    private readonly SmsSender _sender;

    public SmsSenderScopeTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _sender = new SmsSender(
            new TestDbContextFactory(opts),
            new SmsProviderRegistry([new AlwaysReadyProvider()]),
            NullLogger<SmsSender>.Instance);
    }

    public void Dispose() => _context.Dispose();

    private static WebsiteSmsSetting Row(int websiteId) => new()
    {
        WebsiteID = websiteId, Provider = AlwaysReadyProvider.ProviderKey, Title = "Gateway",
        SettingsJSON = "{}", SenderNumber = "1000", IsActive = true, SortOrder = 0, CreatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task A_Website_Without_Its_Own_Gateway_Does_Not_Borrow_Website_1s()
    {
        _context.WebsiteSmsSettings.Add(Row(AppConstants.MasterWebsiteId));
        await _context.SaveChangesAsync();

        (await _sender.IsConfiguredAsync(WebsiteId)).Should().BeFalse();
        (await _sender.SendAsync(WebsiteId, "98", "9120000000", "code 1234")).Should().BeFalse();
    }

    [Fact]
    public async Task A_Website_With_Its_Own_Gateway_Sends_Through_It()
    {
        _context.WebsiteSmsSettings.AddRange(Row(WebsiteId), Row(AppConstants.MasterWebsiteId));
        await _context.SaveChangesAsync();

        (await _sender.IsConfiguredAsync(WebsiteId)).Should().BeTrue();
        (await _sender.SendAsync(WebsiteId, "98", "9120000000", "code 1234")).Should().BeTrue();
        AlwaysReadyProvider.LastWebsiteId.Should().Be(WebsiteId);
    }

    [Fact]
    public async Task An_Inactive_Own_Gateway_Is_Not_Used()
    {
        var row = Row(WebsiteId);
        row.IsActive = false;
        _context.WebsiteSmsSettings.Add(row);
        await _context.SaveChangesAsync();

        (await _sender.IsConfiguredAsync(WebsiteId)).Should().BeFalse();
    }

    private sealed class AlwaysReadyProvider : ISmsProvider
    {
        public const string ProviderKey = "test-gateway";

        /// <summary>Which website's settings the sender handed over — proves no cross-site borrowing.</summary>
        public static int LastWebsiteId { get; private set; }

        public string Key => ProviderKey;
        public string DisplayName => "Test gateway";
        public bool IsIranian => false;

        public bool IsConfigured(SmsSettingContext ctx) => true;

        public Task<SmsSendResult> SendAsync(SmsSettingContext ctx, string countryCode, string cellphone,
            string message, CancellationToken ct = default)
        {
            LastWebsiteId = ctx.WebsiteID;
            return Task.FromResult(SmsSendResult.Ok());
        }
    }
}
