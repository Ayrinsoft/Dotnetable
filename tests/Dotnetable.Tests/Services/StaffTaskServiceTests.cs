using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

public class StaffTaskServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly StaffTaskService _service;
    private readonly Mock<IAdminNotificationService> _notifications = new();

    public StaffTaskServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        // Services open a context per call now, so they get a factory over the same options;
        // the fixture keeps its own _context for seeding and asserting.
        var factory = new TestDbContextFactory(opts);
        _service = new StaffTaskService(new TestDbContextFactory(opts), _notifications.Object);
        Seed();
    }

    public void Dispose() => _context.Dispose();

    private void Seed()
    {
        _context.Websites.Add(new Website
        {
            WebsiteID = 1,
            TradeName = "Shop",
            BrandName = "Shop",
            WebsiteAddress = "shop.test",
            AuthCode = Guid.NewGuid(),
            Active = true,
            Manager = "Mgr",
            Mobile = "1",
            Email = "a@shop.test",
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DefaultLanguageCode = "en",
            DefaultCurrencyCode = "USD",
        });
        _context.Websites.Add(new Website
        {
            WebsiteID = 2,
            TradeName = "Other",
            BrandName = "Other",
            WebsiteAddress = "other.test",
            AuthCode = Guid.NewGuid(),
            Active = true,
            Manager = "Mgr",
            Mobile = "2",
            Email = "a@other.test",
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DefaultLanguageCode = "en",
            DefaultCurrencyCode = "USD",
        });
        AddMember(10, 1, "sara", "Sara", "One");
        AddMember(11, 1, "ali", "Ali", "Two");
        AddMember(20, 2, "other", "Other", "Site");
        _context.Orders.Add(new Order
        {
            OrderID = 500,
            WebsiteID = 1,
            WebsiteClientID = 1,
            OrderNumber = "ORD-500",
            Status = (byte)OrderStatus.Processing,
            CurrencyCode = "USD",
            ExchangeRateToUsd = 1,
            SubTotal = 10,
            DiscountTotal = 0,
            ShippingTotal = 0,
            TaxTotal = 0,
            GrandTotal = 10,
            GrandTotalUsd = 10,
            CreatedAt = DateTime.UtcNow,
        });
        _context.StockDocuments.Add(new StockDocument
        {
            StockDocumentID = 70,
            WebsiteID = 1,
            DocumentNumber = "OUT-70",
            DocumentType = (byte)StockDocumentType.Outbound,
            Status = (byte)StockDocumentStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
        });
        _context.SaveChanges();
    }

    private void AddMember(int id, int websiteId, string username, string given, string surname)
    {
        _context.Members.Add(new Member
        {
            MemberID = id,
            WebsiteID = websiteId,
            Username = username,
            Password = "x",
            Email = username + "@shop.test",
            CellphoneNumber = "9000000000",
            CountryCode = "98",
            Givenname = given,
            Surname = surname,
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            PolicyID = 1,
        });
    }

    private static StaffTaskWriteRequest Req(int assignee, byte relatedKind = 0, int? relatedId = null) => new()
    {
        WebsiteID = 1,
        Title = "Follow up",
        Description = "Call the warehouse",
        AssignedMemberID = assignee,
        RelatedKind = relatedKind,
        RelatedEntityID = relatedId,
    };

    [Fact]
    public async Task Create_Self_Succeeds_WithoutManage()
    {
        var (ok, err, task) = await _service.CreateAsync(Req(10), actorMemberId: 10, canManage: false);

        ok.Should().BeTrue(err);
        task!.AssignedMemberID.Should().Be(10);
        task.CreatedByMemberID.Should().Be(10);
        task.Status.Should().Be((byte)StaffTaskStatus.Open);
    }

    [Fact]
    public async Task Create_ForColleague_Fails_WithoutManage()
    {
        var (ok, err, _) = await _service.CreateAsync(Req(11), actorMemberId: 10, canManage: false);

        ok.Should().BeFalse();
        err.Should().Contain("yourself");
    }

    [Fact]
    public async Task Create_ForColleague_Succeeds_WithManage()
    {
        var (ok, err, task) = await _service.CreateAsync(Req(11), actorMemberId: 10, canManage: true);

        ok.Should().BeTrue(err);
        task!.AssignedMemberID.Should().Be(11);
        _notifications.Verify(n => n.NotifyMemberAsync(
            11, 1, AdminNotificationType.StaffTask, It.IsAny<string>(), It.IsAny<string>(),
            "/tasks", It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_ForOtherWebsiteMember_Fails()
    {
        var (ok, err, _) = await _service.CreateAsync(Req(20), actorMemberId: 10, canManage: true);

        ok.Should().BeFalse();
        err.Should().Contain("colleague");
    }

    [Fact]
    public async Task Create_RelatedOrder_And_Outbound()
    {
        var order = await _service.CreateAsync(
            Req(10, (byte)StaffTaskRelatedKind.Order, 500), 10, false);
        order.Success.Should().BeTrue(order.Error);
        order.Task!.RelatedLabel.Should().Be("ORD-500");
        order.Task.RelatedUrl.Should().Be("/orders/500");

        var outbound = await _service.CreateAsync(
            Req(10, (byte)StaffTaskRelatedKind.StockOutbound, 70), 10, false);
        outbound.Success.Should().BeTrue(outbound.Error);
        outbound.Task!.RelatedLabel.Should().Be("OUT-70");

        var inboundOnOutboundDoc = await _service.CreateAsync(
            Req(10, (byte)StaffTaskRelatedKind.StockInbound, 70), 10, false);
        inboundOnOutboundDoc.Success.Should().BeFalse();
    }

    [Fact]
    public async Task List_NonManager_SeesOwnOnly()
    {
        await _service.CreateAsync(Req(10), 10, false);
        await _service.CreateAsync(Req(11), 10, true);

        var mine = await _service.ListAsync(new StaffTaskListFilter
        {
            WebsiteID = 1,
            ActorMemberID = 11,
            CanManage = false,
        });
        mine.Should().ContainSingle(t => t.AssignedMemberID == 11);

        var managed = await _service.ListAsync(new StaffTaskListFilter
        {
            WebsiteID = 1,
            ActorMemberID = 10,
            CanManage = true,
        });
        managed.Should().HaveCount(2);
    }

    [Fact]
    public async Task SetStatus_Assignee_CanComplete()
    {
        var created = await _service.CreateAsync(Req(11), 10, true);
        var (ok, err) = await _service.SetStatusAsync(created.Task!.StaffTaskID, StaffTaskStatus.Done, 11, false);
        ok.Should().BeTrue(err);

        var row = await _service.GetByIdAsync(created.Task.StaffTaskID);
        row!.Status.Should().Be((byte)StaffTaskStatus.Done);
        row.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task AddNote_AppearsOnGetById_AndListCount()
    {
        var created = await _service.CreateAsync(Req(11), 10, true);
        var id = created.Task!.StaffTaskID;

        var (ok, err, note) = await _service.AddNoteAsync(id, "Customer called; waiting on inbound.", 11, false);
        ok.Should().BeTrue(err);
        note!.Body.Should().Contain("inbound");

        var detail = await _service.GetByIdAsync(id);
        detail!.Notes.Should().ContainSingle(n => n.Body.Contains("inbound"));
        detail.NoteCount.Should().Be(1);

        var list = await _service.ListAsync(new StaffTaskListFilter
        {
            WebsiteID = 1,
            ActorMemberID = 11,
            CanManage = false,
        });
        list.Should().Contain(t => t.StaffTaskID == id && t.NoteCount == 1);
    }
}
