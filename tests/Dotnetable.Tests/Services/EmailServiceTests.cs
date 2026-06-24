using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class EmailServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly EmailService _service;

    public EmailServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _service = new EmailService(_context);
    }

    private static EmailSetting FullRow() => new()
    {
        MailServer = "smtp.example.com",
        SMTPPort = 587,
        EnableSSL = true,
        EmailAddress = "noreply@example.com",
        Password = "secret",
        MailName = "App Mailer",
        DefaultEMail = true,
        Active = true,
        EmailTypeID = 0
    };

    // ── GetDefaultAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDefaultAsync_NoRow_ReturnsNull()
    {
        (await _service.GetDefaultAsync()).Should().BeNull();
    }

    [Fact]
    public async Task GetDefaultAsync_WithRow_ReturnsMappedSettings()
    {
        _context.EmailSettings.Add(FullRow());
        await _context.SaveChangesAsync();

        var result = await _service.GetDefaultAsync();

        result.Should().NotBeNull();
        result!.MailServer.Should().Be("smtp.example.com");
        result.SmtpPort.Should().Be(587);
        result.EnableSSL.Should().BeTrue();
        result.EmailAddress.Should().Be("noreply@example.com");
        result.MailName.Should().Be("App Mailer");
    }

    // ── IsConfiguredAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task IsConfiguredAsync_NoRow_ReturnsFalse()
    {
        (await _service.IsConfiguredAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task IsConfiguredAsync_MissingMailServer_ReturnsFalse()
    {
        _context.EmailSettings.Add(new EmailSetting
        {
            MailServer = "",
            EmailAddress = "noreply@example.com",
            Password = "x", MailName = "x",
            DefaultEMail = true, Active = true, EmailTypeID = 0
        });
        await _context.SaveChangesAsync();

        (await _service.IsConfiguredAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task IsConfiguredAsync_MissingEmailAddress_ReturnsFalse()
    {
        _context.EmailSettings.Add(new EmailSetting
        {
            MailServer = "smtp.example.com",
            EmailAddress = "",
            Password = "x", MailName = "x",
            DefaultEMail = true, Active = true, EmailTypeID = 0
        });
        await _context.SaveChangesAsync();

        (await _service.IsConfiguredAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task IsConfiguredAsync_FullyConfigured_ReturnsTrue()
    {
        _context.EmailSettings.Add(FullRow());
        await _context.SaveChangesAsync();

        (await _service.IsConfiguredAsync()).Should().BeTrue();
    }

    // ── SaveDefaultAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task SaveDefaultAsync_NoExistingRow_CreatesNewRow()
    {
        var settings = new EmailSettingsInfo
        {
            MailServer = "smtp.new.com",
            SmtpPort = 465,
            EnableSSL = true,
            EmailAddress = "admin@new.com",
            Password = "pwd",
            MailName = "New Mailer"
        };

        await _service.SaveDefaultAsync(settings);

        var row = _context.EmailSettings.Single();
        row.MailServer.Should().Be("smtp.new.com");
        row.SMTPPort.Should().Be(465);
        row.EmailAddress.Should().Be("admin@new.com");
        row.MailName.Should().Be("New Mailer");
        row.DefaultEMail.Should().BeTrue();
    }

    [Fact]
    public async Task SaveDefaultAsync_ExistingRow_UpdatesInPlace()
    {
        _context.EmailSettings.Add(FullRow());
        await _context.SaveChangesAsync();

        var settings = new EmailSettingsInfo
        {
            MailServer = "smtp.updated.com",
            SmtpPort = 25,
            EnableSSL = false,
            EmailAddress = "updated@example.com",
            Password = "newpwd",
            MailName = "Updated Mailer"
        };
        await _service.SaveDefaultAsync(settings);

        _context.EmailSettings.Should().HaveCount(1);
        var row = _context.EmailSettings.Single();
        row.MailServer.Should().Be("smtp.updated.com");
        row.SMTPPort.Should().Be(25);
    }

    [Fact]
    public async Task SaveDefaultAsync_EmptyMailName_FallsBackToEmailAddress()
    {
        var settings = new EmailSettingsInfo
        {
            MailServer = "smtp.example.com",
            EmailAddress = "hello@example.com",
            MailName = ""
        };

        await _service.SaveDefaultAsync(settings);

        _context.EmailSettings.Single().MailName.Should().Be("hello@example.com");
    }

    [Fact]
    public async Task SaveDefaultAsync_WhitespaceMailName_FallsBackToEmailAddress()
    {
        var settings = new EmailSettingsInfo
        {
            MailServer = "smtp.example.com",
            EmailAddress = "hello@example.com",
            MailName = "   "
        };

        await _service.SaveDefaultAsync(settings);

        _context.EmailSettings.Single().MailName.Should().Be("hello@example.com");
    }

    [Fact]
    public async Task SaveDefaultAsync_TrimsWhitespaceFromMailServerAndAddress()
    {
        var settings = new EmailSettingsInfo
        {
            MailServer = "  smtp.example.com  ",
            EmailAddress = "  admin@example.com  ",
            MailName = "Mailer"
        };

        await _service.SaveDefaultAsync(settings);

        var row = _context.EmailSettings.Single();
        row.MailServer.Should().Be("smtp.example.com");
        row.EmailAddress.Should().Be("admin@example.com");
    }

    public void Dispose() => _context.Dispose();
}
