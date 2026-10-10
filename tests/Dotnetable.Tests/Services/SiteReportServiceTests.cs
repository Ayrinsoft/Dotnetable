using Dotnetable.Application.DTOs;
using Dotnetable.Application.Email;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

public class SiteReportServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IEmailService> _email;
    private readonly Mock<IAdminNotificationService> _notifications;
    private readonly SiteReportService _service;
    private readonly Website _site;
    private readonly Website _other;

    public SiteReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _email = new Mock<IEmailService>();
        _email.Setup(e => e.IsConfiguredAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _notifications = new Mock<IAdminNotificationService>();
        _service = new SiteReportService(new TestDbContextFactory(options), _notifications.Object, _email.Object, NullLogger<SiteReportService>.Instance);

        _site = Website("one.test", "one@test.com");
        _other = Website("two.test", "two@test.com");
        _context.Websites.AddRange(_site, _other);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateAsync_StoresReport_NotifiesAdmins_AndEmailsTheManager()
    {
        var saved = await _service.CreateAsync(new SiteReport
        {
            WebsiteID = _site.WebsiteID,
            ReporterName = "Neda",
            EmailAddress = "neda@visitor.test",
            Kind = (byte)SiteReportKind.InappropriateContent,
            Subject = "Bad photo",
            MessageBody = "This image should not be public.",
            PageUrl = "/posts/hello",
            SenderIPAddress = "127.0.0.1",
        });

        saved.SiteReportID.Should().BeGreaterThan(0);
        saved.Status.Should().Be((byte)SiteReportStatus.New);

        _notifications.Verify(n => n.NotifySiteAdminsAsync(
            _site.WebsiteID,
            AdminNotificationType.SiteReport,
            "New site report",
            It.Is<string>(m => m.Contains("Neda") && m.Contains("Bad photo")),
            $"/messages/reports/{saved.SiteReportID}",
            saved.SiteReportID,
            It.IsAny<CancellationToken>()), Times.Once);

        _email.Verify(e => e.SendTemplateAsync(
            _site.WebsiteID,
            EmailTemplateKeys.SiteReportNotification,
            "one@test.com",
            It.Is<IDictionary<string, string>>(t => t["Kind"] == "Inappropriate content" && t["PageUrl"] == "/posts/hello"),
            "en",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_MasterSeesEverySite_SiteAdminSeesOnlyTheirOwn()
    {
        await _service.CreateAsync(Report(_site.WebsiteID, "A"));
        await _service.CreateAsync(Report(_other.WebsiteID, "B"));

        var all = await _service.GetPagedAsync(null, new GridQuery { PageIndex = 1, PageSize = 20 });
        all.TotalCount.Should().Be(2);

        var own = await _service.GetPagedAsync(_site.WebsiteID, new GridQuery { PageIndex = 1, PageSize = 20 });
        own.Items.Should().OnlyContain(r => r.WebsiteID == _site.WebsiteID);
    }

    [Fact]
    public async Task GetByIdAsync_HidesAnotherSitesReport()
    {
        var saved = await _service.CreateAsync(Report(_site.WebsiteID, "Hidden"));
        (await _service.GetByIdAsync(saved.SiteReportID, _other.WebsiteID)).Should().BeNull();
        (await _service.GetByIdAsync(saved.SiteReportID, null))!.Subject.Should().Be("Hidden");
    }

    [Fact]
    public async Task UpdateReviewAsync_ChangesStatusForTheOwningSite()
    {
        var saved = await _service.CreateAsync(Report(_site.WebsiteID, "Fix me"));
        (await _service.UpdateReviewAsync(saved.SiteReportID, _site.WebsiteID, SiteReportStatus.Resolved, "Fixed the link")).Should().BeTrue();

        _context.ChangeTracker.Clear();
        var row = await _context.SiteReports.SingleAsync();
        row.Status.Should().Be((byte)SiteReportStatus.Resolved);
        row.AdminNote.Should().Be("Fixed the link");
    }

    private static Website Website(string host, string email) => new()
    {
        TradeName = host, BrandName = host, WebsiteAddress = host,
        AuthCode = Guid.NewGuid(), Active = true, Manager = "Manager", Mobile = "1",
        Email = email, RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
        DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
    };

    private static SiteReport Report(int websiteId, string subject) => new()
    {
        WebsiteID = websiteId,
        ReporterName = "Visitor",
        EmailAddress = "v@test.com",
        Kind = (byte)SiteReportKind.Bug,
        Subject = subject,
        MessageBody = "Steps to reproduce.",
        SenderIPAddress = "10.0.0.1",
    };

    public void Dispose() => _context.Dispose();
}
