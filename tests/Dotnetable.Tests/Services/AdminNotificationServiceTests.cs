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
    private readonly AppDbContext _context;
    private readonly Mock<IEmailService> _emailMock;
    private readonly AdminNotificationService _service;
    private readonly Website _website;

    public AdminNotificationServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _emailMock = new Mock<IEmailService>();
        _emailMock.Setup(e => e.IsConfiguredAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _service = new AdminNotificationService(_context, _emailMock.Object, NullLogger<AdminNotificationService>.Instance);

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

        var rows = _context.AdminNotifications.ToList();
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

        _context.AdminNotifications.Should().HaveCount(1);
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

    public void Dispose() => _context.Dispose();
}
