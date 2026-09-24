using Dotnetable.Application;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Messaging;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using Dotnetable.Infrastructure.Sms;
using Dotnetable.Infrastructure.WhatsApp;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dotnetable.Tests.Services;

/// <summary>
/// The senders record every attempt in <c>MessageLogs</c>, honour the ambient
/// <see cref="MessageLogScope"/> (source, recipient, redaction), and WhatsApp falls back to the
/// master website's gateway where SMS does not.
/// </summary>
public class MessageLogTests : IDisposable
{
    private const int WebsiteId = 2;

    private readonly AppDbContext _context;
    private readonly TestDbContextFactory _factory;
    private readonly MessageLogService _log;

    public MessageLogTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _factory = new TestDbContextFactory(opts);
        _log = new MessageLogService(_factory, NullLogger<MessageLogService>.Instance);
    }

    public void Dispose() => _context.Dispose();

    private SmsSender Sms() =>
        new(_factory, new SmsProviderRegistry([new FakeSms()]), _log, NullLogger<SmsSender>.Instance);

    private WhatsAppSender WhatsApp() =>
        new(_factory, new WhatsAppProviderRegistry([new FakeWhatsApp()]), _log, NullLogger<WhatsAppSender>.Instance);

    private async Task<List<MessageLog>> LogsAsync()
    {
        _context.ChangeTracker.Clear();
        return await _context.MessageLogs.AsNoTracking().ToListAsync();
    }

    [Fact]
    public async Task A_Successful_Sms_Is_Logged_With_Its_Scope()
    {
        _context.WebsiteSmsSettings.Add(new WebsiteSmsSetting
        {
            WebsiteID = WebsiteId, Provider = FakeSms.ProviderKey, Title = "t", SettingsJSON = "{}",
            IsActive = true, CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        using (MessageLogScope.Begin(MessageLogSources.Manual, recipientType: MessageRecipientType.Client,
                   recipientId: 7, recipientName: "Sara", sentByMemberId: 3, sentByName: "admin"))
        {
            (await Sms().SendAsync(WebsiteId, "98", "9120000000", "hello")).Should().BeTrue();
        }

        var row = (await LogsAsync()).Should().ContainSingle().Subject;
        row.Channel.Should().Be((byte)MessageChannel.Sms);
        row.Status.Should().Be((byte)MessageLogStatus.Sent);
        row.Recipient.Should().Be("+98 9120000000");
        row.Body.Should().Be("hello");
        row.Source.Should().Be(MessageLogSources.Manual);
        row.RecipientID.Should().Be(7);
        row.RecipientName.Should().Be("Sara");
        row.SentByMemberID.Should().Be(3);
    }

    [Fact]
    public async Task A_One_Time_Code_Is_Logged_Without_Its_Body()
    {
        _context.WebsiteSmsSettings.Add(new WebsiteSmsSetting
        {
            WebsiteID = WebsiteId, Provider = FakeSms.ProviderKey, Title = "t", SettingsJSON = "{}",
            IsActive = true, CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        using (MessageLogScope.Begin(MessageLogSources.Otp, redactBody: true))
        using (MessageLogScope.Begin(recipientType: MessageRecipientType.Client)) // an inner scope cannot un-redact
        {
            await Sms().SendAsync(WebsiteId, "98", "9120000000", "Your code is 123456");
        }

        var row = (await LogsAsync()).Should().ContainSingle().Subject;
        row.Body.Should().BeNull();
        row.IsBodyRedacted.Should().BeTrue();
        row.Source.Should().Be(MessageLogSources.Otp);
    }

    [Fact]
    public async Task An_Sms_With_No_Gateway_Is_Logged_As_Failed()
    {
        (await Sms().SendAsync(WebsiteId, "98", "9120000000", "hello")).Should().BeFalse();

        var row = (await LogsAsync()).Should().ContainSingle().Subject;
        row.Status.Should().Be((byte)MessageLogStatus.Failed);
        row.Error.Should().NotBeNullOrEmpty();
        row.Source.Should().Be(MessageLogSources.System);
    }

    [Fact]
    public async Task WhatsApp_Falls_Back_To_The_Master_Websites_Gateway()
    {
        _context.WebsiteWhatsAppSettings.Add(new WebsiteWhatsAppSetting
        {
            WebsiteID = AppConstants.MasterWebsiteId, Provider = FakeWhatsApp.ProviderKey, Title = "master",
            SettingsJSON = "{}", IsActive = true, CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        var sender = WhatsApp();
        (await sender.IsConfiguredAsync(WebsiteId)).Should().BeTrue();
        (await sender.SendAsync(WebsiteId, "98", "9120000000", "hi")).Should().BeTrue();
        FakeWhatsApp.LastSettingsWebsiteId.Should().Be(AppConstants.MasterWebsiteId);

        // The log is filed under the website the message was sent for, not the gateway owner.
        (await LogsAsync()).Should().ContainSingle().Which.WebsiteID.Should().Be(WebsiteId);
    }

    [Fact]
    public async Task WhatsApp_Prefers_The_Websites_Own_Gateway()
    {
        _context.WebsiteWhatsAppSettings.AddRange(
            new WebsiteWhatsAppSetting
            {
                WebsiteID = AppConstants.MasterWebsiteId, Provider = FakeWhatsApp.ProviderKey, Title = "master",
                SettingsJSON = "{}", IsActive = true, CreatedAt = DateTime.UtcNow,
            },
            new WebsiteWhatsAppSetting
            {
                WebsiteID = WebsiteId, Provider = FakeWhatsApp.ProviderKey, Title = "own",
                SettingsJSON = "{}", IsActive = true, SortOrder = 5, CreatedAt = DateTime.UtcNow,
            });
        await _context.SaveChangesAsync();

        (await WhatsApp().SendAsync(WebsiteId, "98", "9120000000", "hi")).Should().BeTrue();
        FakeWhatsApp.LastSettingsWebsiteId.Should().Be(WebsiteId);
    }

    [Fact]
    public async Task The_Summary_Counts_By_Status_And_Channel()
    {
        await _log.WriteAsync(new MessageLogEntry { WebsiteID = WebsiteId, Channel = MessageChannel.Email, Success = true, Recipient = "a@b.c" });
        await _log.WriteAsync(new MessageLogEntry { WebsiteID = WebsiteId, Channel = MessageChannel.Sms, Success = false, Recipient = "+98 912" });
        await _log.WriteAsync(new MessageLogEntry { WebsiteID = 9, Channel = MessageChannel.Sms, Success = true, Recipient = "+98 913" });

        var summary = await _log.GetSummaryAsync(new MessageLogFilter { WebsiteID = WebsiteId });

        summary.Total.Should().Be(2);
        summary.Sent.Should().Be(1);
        summary.Failed.Should().Be(1);
        summary.ByChannel[MessageChannel.Sms].Should().Be(1);
    }

    private sealed class FakeSms : ISmsProvider
    {
        public const string ProviderKey = "fake-sms";
        public string Key => ProviderKey;
        public string DisplayName => "Fake SMS";
        public bool IsIranian => false;
        public bool IsConfigured(SmsSettingContext ctx) => true;

        public Task<SmsSendResult> SendAsync(SmsSettingContext ctx, string countryCode, string cellphone,
            string message, CancellationToken ct = default) => Task.FromResult(SmsSendResult.Ok());
    }

    private sealed class FakeWhatsApp : IWhatsAppProvider
    {
        public const string ProviderKey = "fake-wa";

        public static int LastSettingsWebsiteId { get; private set; }

        public string Key => ProviderKey;
        public string DisplayName => "Fake WhatsApp";
        public bool IsConfigured(WhatsAppSettingContext ctx) => true;

        public Task<SmsSendResult> SendAsync(WhatsAppSettingContext ctx, string countryCode, string cellphone,
            string message, CancellationToken ct = default)
        {
            LastSettingsWebsiteId = ctx.WebsiteID;
            return Task.FromResult(SmsSendResult.Ok());
        }
    }
}
