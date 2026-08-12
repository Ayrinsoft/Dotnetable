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
        wa.SetupGet(w => w.IsConfigured).Returns(false);
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
            It.Is<IDictionary<string, string>>(t => t["Title"] == "New order" && t["MessageBody"] == "Order #1 placed"),
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
