using Dotnetable.Application;
using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class EmailServiceTests : IDisposable
{
    private const int WebsiteId = 2;

    private readonly AppDbContext _context;
    private readonly EmailService _service;
    private readonly EmailAccountService _accounts;

    public EmailServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        // Services open a context per call now, so they get a factory over the same options;
        // the fixture keeps its own _context for seeding and asserting.
        var factory = new TestDbContextFactory(opts);
        _accounts = new EmailAccountService(factory);
        _service = new EmailService(factory, new EmailTemplateService(factory));
    }

    private static EmailAccount FullRow(int websiteId, EmailAccountType type = EmailAccountType.NoReply, bool isDefault = true) => new()
    {
        WebsiteID = websiteId,
        AccountType = (byte)type,
        Name = type.ToString(),
        MailServer = "smtp.example.com",
        SMTPPort = 587,
        EnableSSL = true,
        EmailAddress = "noreply@example.com",
        Password = "secret",
        MailName = "App Mailer",
        IsDefault = isDefault,
        Active = true,
    };

    // ── IsConfiguredAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task IsConfiguredAsync_NoRow_ReturnsFalse()
    {
        (await _service.IsConfiguredAsync(WebsiteId)).Should().BeFalse();
    }

    [Fact]
    public async Task IsConfiguredAsync_OwnAccount_ReturnsTrue()
    {
        _context.EmailAccounts.Add(FullRow(WebsiteId));
        await _context.SaveChangesAsync();

        (await _service.IsConfiguredAsync(WebsiteId)).Should().BeTrue();
    }

    [Fact]
    public async Task IsConfiguredAsync_FallsBackToMasterWebsite()
    {
        _context.EmailAccounts.Add(FullRow(AppConstants.MasterWebsiteId));
        await _context.SaveChangesAsync();

        (await _service.IsConfiguredAsync(WebsiteId)).Should().BeTrue();
    }

    [Fact]
    public async Task IsConfiguredAsync_InactiveAccount_ReturnsFalse()
    {
        var row = FullRow(WebsiteId);
        row.Active = false;
        _context.EmailAccounts.Add(row);
        await _context.SaveChangesAsync();

        (await _service.IsConfiguredAsync(WebsiteId)).Should().BeFalse();
    }

    // ── SendAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task SendAsync_NotConfigured_Throws()
    {
        var act = () => _service.SendAsync(WebsiteId, EmailAccountType.NoReply, "to@example.com", "Subject", "<p>Body</p>");
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── SendTemplateAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task SendTemplateAsync_UnknownKey_Throws()
    {
        _context.EmailAccounts.Add(FullRow(WebsiteId));
        await _context.SaveChangesAsync();

        var act = () => _service.SendTemplateAsync(WebsiteId, "NotARealKey", "to@example.com");
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── ApplyDocumentDirection ────────────────────────────────────────────────

    [Fact]
    public void ApplyDocumentDirection_FullHtml_SetsDirAndLang()
    {
        var html = "<!doctype html><html dir=\"ltr\" lang=\"en\"><body>Hi</body></html>";
        var result = EmailService.ApplyDocumentDirection(html, "fa", rtl: true);
        result.Should().Contain("dir=\"rtl\"");
        result.Should().Contain("lang=\"fa\"");
        result.Should().NotContain("dir=\"ltr\"");
    }

    [Fact]
    public void ApplyDocumentDirection_Fragment_WrapsWithDir()
    {
        var result = EmailService.ApplyDocumentDirection("<p>سلام</p>", "fa", rtl: true);
        result.Should().Be("<div dir=\"rtl\" lang=\"fa\"><p>سلام</p></div>");
    }

    [Fact]
    public void ApplyDocumentDirection_Ltr_KeepsLtr()
    {
        var html = "<html><body>x</body></html>";
        var result = EmailService.ApplyDocumentDirection(html, "en", rtl: false);
        result.Should().Contain("dir=\"ltr\"");
        result.Should().Contain("lang=\"en\"");
    }

    // ── EmailAccountService.SaveAsync ─────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_NoExistingRow_CreatesNewRow()
    {
        var info = new EmailAccountInfo
        {
            WebsiteID = WebsiteId,
            AccountType = EmailAccountType.Support,
            Name = "Support",
            MailServer = "smtp.new.com",
            SmtpPort = 465,
            EnableSSL = true,
            EmailAddress = "admin@new.com",
            Password = "pwd",
            MailName = "New Mailer",
            Active = true,
        };

        await _accounts.SaveAsync(info);

        var row = _context.EmailAccounts.Single();
        row.MailServer.Should().Be("smtp.new.com");
        row.SMTPPort.Should().Be(465);
        row.EmailAddress.Should().Be("admin@new.com");
        row.AccountType.Should().Be((byte)EmailAccountType.Support);
    }

    [Fact]
    public async Task SaveAsync_EmptyMailName_FallsBackToEmailAddress()
    {
        var info = new EmailAccountInfo
        {
            WebsiteID = WebsiteId,
            Name = "NoReply",
            MailServer = "smtp.example.com",
            EmailAddress = "hello@example.com",
            MailName = "",
        };

        await _accounts.SaveAsync(info);

        _context.EmailAccounts.Single().MailName.Should().Be("hello@example.com");
    }

    [Fact]
    public async Task SaveAsync_NewDefault_DemotesPreviousDefaultForSameWebsite()
    {
        var first = FullRow(WebsiteId, EmailAccountType.NoReply, isDefault: true);
        _context.EmailAccounts.Add(first);
        await _context.SaveChangesAsync();

        var info = new EmailAccountInfo
        {
            WebsiteID = WebsiteId,
            AccountType = EmailAccountType.Support,
            Name = "Support",
            MailServer = "smtp.example.com",
            EmailAddress = "support@example.com",
            MailName = "Support",
            IsDefault = true,
            Active = true,
        };
        await _accounts.SaveAsync(info);

        // The service wrote through its own short-lived context, so this fixture's context must
        // re-read rather than answer from entities it is still tracking.
        _context.ChangeTracker.Clear();

        _context.EmailAccounts.Single(a => a.EmailAccountID == first.EmailAccountID).IsDefault.Should().BeFalse();
        _context.EmailAccounts.Count(a => a.IsDefault).Should().Be(1);
    }

    public void Dispose() => _context.Dispose();
}
