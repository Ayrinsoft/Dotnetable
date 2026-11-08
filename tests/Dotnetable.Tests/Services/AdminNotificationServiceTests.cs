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

public class AdminNotificationServiceTests : IDisposable
{
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly AppDbContext _context;
    private readonly Mock<IEmailService> _emailMock;
    private readonly AdminNotificationService _service;
    private readonly Website _website;

    public AdminNotificationServiceTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(_options);
        _emailMock = new Mock<IEmailService>();
        _emailMock.Setup(e => e.IsConfiguredAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var factory = new TestDbContextFactory(_options);
        var wa = new Mock<IWhatsAppSender>();
        wa.Setup(w => w.IsConfiguredAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _service = new AdminNotificationService(factory, _emailMock.Object, wa.Object, NullLogger<AdminNotificationService>.Instance);

        _website = new Website
        {
            TradeName = "Test", BrandName = "Test", WebsiteAddress = "test.com",
            AuthCode = Guid.NewGuid(), Active = true, Manager = "Mgr", Mobile = "1",
            Email = "site@test.com", RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
        };
        _context.Websites.Add(_website);
        _context.SaveChanges();
    }

    private async Task<Member> SeedMemberAsync(bool isSiteAdmin, string email = "admin@test.com", bool active = true)
    {
        var policy = new Policy { Title = "P", Active = true, WebsiteID = _website.WebsiteID };
        _context.Policies.Add(policy);
        await _context.SaveChangesAsync();

        var member = new Member
        {
            Username = email, Active = active, Password = "x", Email = email,
            CellphoneNumber = "1", CountryCode = "1",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            Givenname = "Admin", Surname = "User", HashKey = Guid.NewGuid(),
            PolicyID = policy.PolicyID, WebsiteID = _website.WebsiteID,
            IsSiteAdmin = isSiteAdmin,
        };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    [Fact]
    public async Task NotifySiteAdminsAsync_CreatesRowsOnlyForActiveSiteAdmins()
    {
        var admin = await SeedMemberAsync(isSiteAdmin: true, email: "a@test.com");
        await SeedMemberAsync(isSiteAdmin: false, email: "b@test.com");
        await SeedMemberAsync(isSiteAdmin: true, email: "c@test.com", active: false);

        await _service.NotifySiteAdminsAsync(
            _website.WebsiteID,
            AdminNotificationType.ContactMessage,
            "New contact message",
            "Someone wrote in",
            "/messages/contacts/1",
            1);

        await using var verify = new AppDbContext(_options);
        var rows = verify.AdminNotifications.ToList();
        rows.Should().HaveCount(1);
        rows[0].MemberID.Should().Be(admin.MemberID);
        rows[0].Title.Should().Be("New contact message");
        rows[0].ActionUrl.Should().Be("/messages/contacts/1");
        rows[0].IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task NotifySiteAdminsAsync_SendsEmailWhenConfigured()
    {
        await SeedMemberAsync(isSiteAdmin: true, email: "a@test.com");

        await _service.NotifySiteAdminsAsync(
            _website.WebsiteID,
            AdminNotificationType.NewOrder,
            "New order",
            "Order #1 placed",
            "/orders/1",
            1);

        _emailMock.Verify(e => e.SendTemplateAsync(
            _website.WebsiteID,
            EmailTemplateKeys.AdminSiteNotification,
            "a@test.com",
            It.Is<IDictionary<string, string>>(t =>
                t["Title"].StartsWith("New order — Test (test.com) — #1 — ")
                && t["Title"].EndsWith(" UTC")
                && t["Action"] == "New order"
                && t["MessageBody"].Contains("Order #1 placed")
                && t["MessageBody"].Contains("test.com")),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifySiteAdminsAsync_SkipsEmailWhenNotConfigured()
    {
        _emailMock.Setup(e => e.IsConfiguredAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        await SeedMemberAsync(isSiteAdmin: true, email: "a@test.com");

        await _service.NotifySiteAdminsAsync(
            _website.WebsiteID,
            AdminNotificationType.BankReceipt,
            "Bank receipt",
            "Needs review",
            "/payments",
            9);

        await using var verify = new AppDbContext(_options);
        verify.AdminNotifications.Should().HaveCount(1);
        _emailMock.Verify(e => e.SendTemplateAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NotifySiteAdminsAsync_EmailsTheSiteOwnerAsWellAsAdmins()
    {
        await SeedMemberAsync(isSiteAdmin: true, email: "a@test.com");

        await _service.NotifySiteAdminsAsync(
            _website.WebsiteID, AdminNotificationType.ContactMessage, "New contact message", "Hi", "/messages/contacts/1", 1);

        _emailMock.Verify(e => e.SendTemplateAsync(
            _website.WebsiteID, EmailTemplateKeys.AdminSiteNotification, "site@test.com",
            It.Is<IDictionary<string, string>>(t =>
                t["AdminName"] == "Mgr" && t["Title"].StartsWith("New contact message — Test (test.com) — #1 — ")),
            "en", It.IsAny<CancellationToken>()), Times.Once);
        _emailMock.Verify(e => e.SendTemplateAsync(
            _website.WebsiteID, EmailTemplateKeys.AdminSiteNotification, "a@test.com",
            It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifySiteAdminsAsync_DoesNotEmailTheOwnerTwiceWhenTheyAreTheAdmin()
    {
        await SeedMemberAsync(isSiteAdmin: true, email: "site@test.com");

        await _service.NotifySiteAdminsAsync(
            _website.WebsiteID, AdminNotificationType.NewOrder, "New order", "Order #1", "/orders/1", 1);

        _emailMock.Verify(e => e.SendTemplateAsync(
            It.IsAny<int>(), EmailTemplateKeys.AdminSiteNotification, "site@test.com",
            It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifySiteAdminsAsync_UsesWebsite1EmailWhenTheSiteHasNone()
    {
        _website.Email = "   ";
        var child = new Website
        {
            TradeName = "Child", BrandName = "Child", WebsiteAddress = "child.test",
            AuthCode = Guid.NewGuid(), Active = true, Manager = "Child mgr", Mobile = "2",
            Email = "", RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            DefaultLanguageCode = "fa", DefaultCurrencyCode = "USD",
        };
        _context.Websites.Add(child);
        await _context.SaveChangesAsync();
        _website.Email = "owner@master.test";
        await _context.SaveChangesAsync();

        await _service.NotifySiteAdminsAsync(
            child.WebsiteID, AdminNotificationType.ClientRegistered, "New customer registration", "Sara registered.", "/clients/1", 1);

        await using var verify = new AppDbContext(_options);
        verify.AdminNotifications.Should().BeEmpty();
        _emailMock.Verify(e => e.SendTemplateAsync(
            child.WebsiteID, EmailTemplateKeys.AdminSiteNotification, "owner@master.test",
            It.Is<IDictionary<string, string>>(t => t["AdminName"] == "Mgr" && t["Title"].StartsWith("New customer registration — Child (child.test) — #1 — ")),
            "en", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifySiteAdminsAsync_SkipsOwnerEmailWhenNeitherSiteHasOne()
    {
        _website.Email = "";
        await _context.SaveChangesAsync();

        await _service.NotifySiteAdminsAsync(
            _website.WebsiteID, AdminNotificationType.PaymentReceived, "Payment", "Paid", "/payments/1", 1);

        _emailMock.Verify(e => e.SendTemplateAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NotifySiteAdminsAsync_SiteReport_DoesNotSendTheGenericOwnerEmail()
    {
        await _service.NotifySiteAdminsAsync(
            _website.WebsiteID, AdminNotificationType.SiteReport, "New site report", "A report", "/messages/reports/1", 1);

        _emailMock.Verify(e => e.SendTemplateAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NotifySiteAdminsAsync_EachSendGetsItsOwnSubject()
    {
        await SeedMemberAsync(isSiteAdmin: true, email: "a@test.com");
        string? first = null;
        string? second = null;
        _emailMock.Setup(e => e.SendTemplateAsync(
                _website.WebsiteID, EmailTemplateKeys.AdminSiteNotification, "a@test.com",
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback<int, string, string, IDictionary<string, string>?, string?, CancellationToken>((_, _, _, tokens, _, _) =>
            {
                var title = tokens!["Title"];
                if (first is null) first = title;
                else second = title;
            })
            .Returns(Task.CompletedTask);

        await _service.NotifySiteAdminsAsync(
            _website.WebsiteID, AdminNotificationType.NewOrder, "New order", "One", "/orders/1", 1);
        await _service.NotifySiteAdminsAsync(
            _website.WebsiteID, AdminNotificationType.NewOrder, "New order", "Two", "/orders/2", 2);

        first.Should().StartWith("New order — Test (test.com) — #1 — ");
        second.Should().StartWith("New order — Test (test.com) — #2 — ");
        first.Should().NotBe(second);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsOnlyForMember()
    {
        var admin = await SeedMemberAsync(isSiteAdmin: true, email: "a@test.com");
        var other = await SeedMemberAsync(isSiteAdmin: true, email: "b@test.com");

        await _service.NotifySiteAdminsAsync(_website.WebsiteID, AdminNotificationType.ClientRegistered, "Reg", "New client", "/clients/1", 1);

        var mine = await _service.GetRecentAsync(admin.MemberID);
        mine.Should().HaveCount(1);
        (await _service.GetUnreadCountAsync(admin.MemberID)).Should().Be(1);

        var others = await _service.GetRecentAsync(other.MemberID);
        others.Should().HaveCount(1);
    }

    [Fact]
    public async Task NotifyMemberAsync_CreatesRowForSingleMember()
    {
        var agent = await SeedMemberAsync(isSiteAdmin: false, email: "agent@test.com");

        await _service.NotifyMemberAsync(
            agent.MemberID,
            _website.WebsiteID,
            AdminNotificationType.SupportTicket,
            "Ticket assigned",
            "SUP-1 needs you",
            "/support/tickets/1",
            1);

        await using var verify = new AppDbContext(_options);
        var rows = verify.AdminNotifications.Where(n => n.MemberID == agent.MemberID).ToList();
        rows.Should().ContainSingle();
        rows[0].NotificationType.Should().Be((byte)AdminNotificationType.SupportTicket);
        rows[0].ActionUrl.Should().Be("/support/tickets/1");
    }

    public void Dispose() => _context.Dispose();

    private sealed class TestDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public TestDbContextFactory(DbContextOptions<AppDbContext> options) => _options = options;

        public AppDbContext CreateDbContext() => new(_options);

        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
