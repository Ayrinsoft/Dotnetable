using Dotnetable.Application;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Email;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class EmailTemplateServiceTests : IDisposable
{
    private const int WebsiteId = 2;

    private readonly AppDbContext _context;
    private readonly EmailTemplateService _service;

    public EmailTemplateServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        // Services open a context per call now, so they get a factory over the same options;
        // the fixture keeps its own _context for seeding and asserting.
        var factory = new TestDbContextFactory(opts);
        _service = new EmailTemplateService(factory);

        _context.Websites.Add(new Website
        {
            WebsiteID = WebsiteId,
            BrandName = "Shop",
            TradeName = "Shop",
            WebsiteAddress = "https://shop.test",
            DefaultLanguageCode = "en",
            DefaultCurrencyCode = "USD",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            Manager = "Mgr",
            Mobile = "123",
            Email = "site@shop.test",
        });
        _context.EmailTemplates.Add(new EmailTemplate
        {
            WebsiteID = AppConstants.MasterWebsiteId,
            TemplateKey = EmailTemplateKeys.Welcome,
            Name = "Welcome",
            Subject = "Welcome EN",
            HtmlBody = "<p>Hello EN</p>",
            AccountType = (byte)EmailAccountType.NoReply,
            Active = true,
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAsync_WithoutTranslation_ReturnsDefaultContent()
    {
        var info = await _service.GetAsync(WebsiteId, EmailTemplateKeys.Welcome, "fa");
        info.Should().NotBeNull();
        info!.Subject.Should().Be("Welcome EN");
        info.HtmlBody.Should().Be("<p>Hello EN</p>");
        info.IsOverride.Should().BeFalse();
    }

    [Fact]
    public async Task SetTranslationsAsync_CreatesOverrideAndReturnsLocalizedContent()
    {
        await _service.SetTranslationsAsync(WebsiteId, EmailTemplateKeys.Welcome, new Dictionary<string, (string, string)>
        {
            ["fa"] = ("خوش آمدید", "<p dir=\"rtl\">سلام</p>"),
        });

        // The service wrote through its own short-lived context, so this fixture's context must
        // re-read rather than answer from entities it is still tracking.
        _context.ChangeTracker.Clear();

        var own = await _context.EmailTemplates.SingleAsync(t => t.WebsiteID == WebsiteId && t.TemplateKey == EmailTemplateKeys.Welcome);
        own.Subject.Should().Be("Welcome EN"); // cloned from master

        var fa = await _service.GetAsync(WebsiteId, EmailTemplateKeys.Welcome, "fa");
        fa!.IsOverride.Should().BeTrue();
        fa.Subject.Should().Be("خوش آمدید");
        fa.HtmlBody.Should().Be("<p dir=\"rtl\">سلام</p>");

        // Default language still uses parent fields
        var en = await _service.GetAsync(WebsiteId, EmailTemplateKeys.Welcome, "en");
        en!.Subject.Should().Be("Welcome EN");
    }

    [Fact]
    public async Task SetTranslationsAsync_BlankSubject_RemovesTranslation()
    {
        await _service.SetTranslationsAsync(WebsiteId, EmailTemplateKeys.Welcome, new Dictionary<string, (string, string)>
        {
            ["fa"] = ("خوش آمدید", "<p>سلام</p>"),
        });
        await _service.SetTranslationsAsync(WebsiteId, EmailTemplateKeys.Welcome, new Dictionary<string, (string, string)>
        {
            ["fa"] = ("", ""),
        });

        // The service wrote through its own short-lived context, so this fixture's context must
        // re-read rather than answer from entities it is still tracking.
        _context.ChangeTracker.Clear();

        (await _context.EmailTemplateTranslations.CountAsync()).Should().Be(0);
        var fa = await _service.GetAsync(WebsiteId, EmailTemplateKeys.Welcome, "fa");
        fa!.Subject.Should().Be("Welcome EN");
    }

    [Fact]
    public async Task ResetToDefaultAsync_RemovesOverrideAndTranslations()
    {
        await _service.SaveAsync(WebsiteId, new EmailTemplateInfo
        {
            TemplateKey = EmailTemplateKeys.Welcome,
            Name = "Welcome",
            Subject = "Custom",
            HtmlBody = "<p>Custom</p>",
            AccountType = EmailAccountType.NoReply,
            Active = true,
        });
        await _service.SetTranslationsAsync(WebsiteId, EmailTemplateKeys.Welcome, new Dictionary<string, (string, string)>
        {
            ["fa"] = ("فا", "<p>فا</p>"),
        });

        await _service.ResetToDefaultAsync(WebsiteId, EmailTemplateKeys.Welcome);

        // The service wrote through its own short-lived context, so this fixture's context must
        // re-read rather than answer from entities it is still tracking.
        _context.ChangeTracker.Clear();

        (await _context.EmailTemplates.AnyAsync(t => t.WebsiteID == WebsiteId)).Should().BeFalse();
        (await _context.EmailTemplateTranslations.CountAsync()).Should().Be(0);
    }

    public void Dispose() => _context.Dispose();
}
