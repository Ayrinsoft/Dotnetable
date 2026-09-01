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

public class SupportDeskServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SupportDeskService _service;
    private readonly Mock<IAdminNotificationService> _notifications = new();

    public SupportDeskServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        // Services open a context per call now, so they get a factory over the same options;
        // the fixture keeps its own _context for seeding and asserting.
        var factory = new TestDbContextFactory(opts);
        _service = new SupportDeskService(factory, _notifications.Object);
        SeedBasics();
    }

    private void SeedBasics()
    {
        _context.Websites.Add(new Website
        {
            WebsiteID = 1,
            TradeName = "Test Shop",
            BrandName = "Test Shop",
            WebsiteAddress = "test.local",
            AuthCode = Guid.NewGuid(),
            Active = true,
            Manager = "Mgr",
            Mobile = "100",
            Email = "admin@test.local",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            DefaultLanguageCode = "en",
            DefaultCurrencyCode = "USD",
        });
        _context.Members.Add(new Member
        {
            MemberID = 10,
            WebsiteID = 1,
            Username = "agent1",
            Password = "x",
            Email = "agent@test.local",
            CellphoneNumber = "9000000000",
            CountryCode = "98",
            Givenname = "Sara",
            Surname = "Support",
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            PolicyID = 1,
        });
        _context.WebsiteClients.Add(new WebsiteClient
        {
            WebsiteClientID = 100,
            WebsiteID = 1,
            Cellphone = "09121234567",
            CountryCode = "98",
            Email = "cust@test.local",
            Givenname = "Ali",
            Surname = "Customer",
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            ClientLevel = 1,
            Password = "x",
        });
        _context.Orders.Add(new Order
        {
            OrderID = 500,
            WebsiteID = 1,
            WebsiteClientID = 100,
            OrderNumber = "ORD-001",
            Status = (byte)OrderStatus.Processing,
            CurrencyCode = "USD",
            ExchangeRateToUsd = 1,
            SubTotal = 100,
            DiscountTotal = 0,
            ShippingTotal = 0,
            TaxTotal = 0,
            GrandTotal = 100,
            GrandTotalUsd = 100,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task LookupByMobileAsync_FindsClient_AndBuilds360()
    {
        var result = await _service.LookupByMobileAsync(1, "09121234567");

        result.Matches.Should().ContainSingle(m => m.WebsiteClientID == 100);
        result.Customer360.Should().NotBeNull();
        result.Customer360!.DisplayName.Should().Contain("Ali");
        result.Customer360.Orders.Should().ContainSingle(o => o.OrderNumber == "ORD-001");
        result.Customer360.InProgressOrderCount.Should().Be(1);
    }

    [Fact]
    public async Task LookupByMobileAsync_PartialDigits_Matches()
    {
        // National number stored as 09121234567 — partial substring search
        var result = await _service.LookupByMobileAsync(1, "121234567");
        result.Matches.Should().ContainSingle();
    }

    [Fact]
    public async Task StartSession_And_AddInteraction_LogsAgents()
    {
        var session = await _service.StartSessionAsync(new StartSupportSessionRequest
        {
            WebsiteID = 1,
            WebsiteClientID = 100,
            Channel = SupportChannel.InboundCall,
            Priority = SupportPriority.High,
            Category = SupportCategory.Order,
            RelatedOrderID = 500,
            Subject = "Where is my order?",
            OpeningNote = "Customer called about shipping.",
            CallOutcome = SupportCallOutcome.Answered,
            DurationSeconds = 180,
            AssignToSelf = true,
        }, memberId: 10);

        session.SessionNumber.Should().StartWith("SUP-");
        session.AssignedMemberID.Should().Be(10);
        session.Status.Should().Be((byte)SupportSessionStatus.Open);

        await _service.AddInteractionAsync(new AddSupportInteractionRequest
        {
            SupportSessionID = session.SupportSessionID,
            InteractionType = SupportInteractionType.Note,
            Body = "Told customer package ships tomorrow.",
        }, memberId: 10);

        await _service.TransitionStatusAsync(session.SupportSessionID, SupportSessionStatus.Resolved, 10, "Done");

        var c360 = await _service.GetCustomer360Async(100);
        c360.Should().NotBeNull();
        c360!.Sessions.Should().ContainSingle();
        c360.RecentInteractions.Count.Should().BeGreaterThanOrEqualTo(2);
        c360.AgentsWhoHelped.Should().ContainSingle(a => a.MemberID == 10 && a.InteractionCount >= 2);

        var interactions = await _service.GetInteractionsAsync(session.SupportSessionID);
        interactions.Should().Contain(i => i.InteractionType == (byte)SupportInteractionType.StatusChange);
        interactions.Should().Contain(i => i.InteractionType == (byte)SupportInteractionType.CallInbound);
    }

    [Fact]
    public async Task LinkOrder_Rejects_OrderOfOtherClient()
    {
        _context.WebsiteClients.Add(new WebsiteClient
        {
            WebsiteClientID = 101,
            WebsiteID = 1,
            Cellphone = "09990000000",
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            ClientLevel = 1,
            Password = "x",
        });
        _context.Orders.Add(new Order
        {
            OrderID = 501,
            WebsiteID = 1,
            WebsiteClientID = 101,
            OrderNumber = "ORD-OTHER",
            Status = (byte)OrderStatus.Paid,
            CurrencyCode = "USD",
            ExchangeRateToUsd = 1,
            GrandTotal = 10,
            GrandTotalUsd = 10,
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        var session = await _service.StartSessionAsync(new StartSupportSessionRequest
        {
            WebsiteID = 1,
            WebsiteClientID = 100,
            Subject = "test",
        }, 10);

        var ok = await _service.LinkOrderAsync(session.SupportSessionID, 501, 10);
        ok.Should().BeFalse();
    }

    [Fact]
    public async Task StartUnknownCaller_SessionWithoutClient()
    {
        var session = await _service.StartSessionAsync(new StartSupportSessionRequest
        {
            WebsiteID = 1,
            Cellphone = "09121112233",
            CustomerName = "Unknown",
            Channel = SupportChannel.InboundCall,
            OpeningNote = "No profile match",
        }, 10);

        session.WebsiteClientID.Should().BeNull();
        session.CellphoneSnapshot.Should().Be("09121112233");
    }

    [Fact]
    public async Task StartSession_AppliesSlaDueDates_AndNotifiesAdminsOnUnassigned()
    {
        var session = await _service.StartSessionAsync(new StartSupportSessionRequest
        {
            WebsiteID = 1,
            WebsiteClientID = 100,
            Priority = SupportPriority.Urgent,
            AssignToSelf = false,
            Subject = "Urgent issue",
        }, 10);

        session.FirstResponseDueAt.Should().NotBeNull();
        session.ResolveDueAt.Should().NotBeNull();
        session.FirstResponseDueAt!.Value.Should().BeCloseTo(session.CreatedAt.AddMinutes(15), TimeSpan.FromSeconds(2));
        session.ResolveDueAt!.Value.Should().BeCloseTo(session.CreatedAt.AddHours(4), TimeSpan.FromSeconds(2));

        _notifications.Verify(n => n.NotifySiteAdminsAsync(
            1,
            AdminNotificationType.SupportTicket,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<string>(u => u.Contains($"/support/tickets/{session.SupportSessionID}")),
            session.SupportSessionID,
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ScheduleCallback_And_DeskStats_CountDue()
    {
        var session = await _service.StartSessionAsync(new StartSupportSessionRequest
        {
            WebsiteID = 1,
            WebsiteClientID = 100,
            AssignToSelf = true,
        }, 10);

        await _service.ScheduleCallbackAsync(session.SupportSessionID, DateTime.UtcNow.AddMinutes(-5), "Call back about invoice", 10);

        var stats = await _service.GetDeskStatsAsync(1, 10);
        stats.OpenCount.Should().BeGreaterThanOrEqualTo(1);
        stats.MyOpenCount.Should().BeGreaterThanOrEqualTo(1);
        stats.CallbackDueCount.Should().BeGreaterThanOrEqualTo(1);

        var due = await _service.GetSessionsPagedAsync(1, null, null, null, false, new GridQuery { PageSize = 50 },
            callbackDueOnly: true);
        due.Items.Should().Contain(s => s.SupportSessionID == session.SupportSessionID);
    }

    [Fact]
    public async Task Assign_NotifiesAssignee()
    {
        _context.Members.Add(new Member
        {
            MemberID = 11,
            WebsiteID = 1,
            Username = "agent2",
            Password = "x",
            Email = "a2@test.local",
            CellphoneNumber = "9000000001",
            CountryCode = "98",
            Givenname = "Reza",
            Surname = "Agent",
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            PolicyID = 1,
        });
        await _context.SaveChangesAsync();

        var session = await _service.StartSessionAsync(new StartSupportSessionRequest
        {
            WebsiteID = 1,
            WebsiteClientID = 100,
            AssignToSelf = true,
        }, 10);

        await _service.AssignAsync(session.SupportSessionID, 11, actorMemberId: 10);

        _notifications.Verify(n => n.NotifyMemberAsync(
            11,
            1,
            AdminNotificationType.SupportTicket,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            session.SupportSessionID,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void SupportSlaPolicy_Urgent_IsShorterThanNormal()
    {
        var (uFirst, uRes) = SupportSlaPolicy.GetTargets(SupportPriority.Urgent);
        var (nFirst, nRes) = SupportSlaPolicy.GetTargets(SupportPriority.Normal);
        uFirst.Should().BeLessThan(nFirst);
        uRes.Should().BeLessThan(nRes);

        var created = DateTime.UtcNow.AddHours(-5);
        var state = SupportSlaPolicy.Evaluate(
            (byte)SupportSessionStatus.Open,
            created,
            firstResponseAt: null,
            firstResponseDueAt: created.AddHours(4),
            resolvedAt: null,
            resolveDueAt: created.AddHours(24));
        state.Should().Be(SupportSlaState.FirstResponseBreached);
    }

    [Fact]
    public async Task CustomerTicket_Unassigned_InQueue_AndLinkedToOrder()
    {
        var (ok, err, session) = await _service.CreateCustomerTicketAsync(
            1, 100, "Where is my package?", "Order still not here.", 500, null);

        ok.Should().BeTrue(err);
        session!.Channel.Should().Be((byte)SupportChannel.Website);
        session.AssignedMemberID.Should().BeNull();
        session.CreatedByMemberID.Should().BeNull();
        session.FirstResponseAt.Should().BeNull();
        session.RelatedOrderID.Should().Be(500);
        session.Category.Should().Be((byte)SupportCategory.Order);

        var queue = await _service.GetSessionsPagedAsync(1, SupportSessionStatus.Open, null, null, false, new GridQuery { PageSize = 20 });
        queue.Items.Should().Contain(s => s.SupportSessionID == session.SupportSessionID && s.Channel == (byte)SupportChannel.Website);

        var mine = await _service.GetClientSessionsPagedAsync(100, new GridQuery { PageSize = 20 });
        mine.Items.Should().ContainSingle(s => s.SupportSessionID == session.SupportSessionID);

        var publicIx = await _service.GetClientInteractionsAsync(session.SupportSessionID, 100);
        publicIx.Should().ContainSingle(i => i.InteractionType == (byte)SupportInteractionType.CustomerReply);
    }

    [Fact]
    public async Task CustomerTicket_WrongOrder_Fails()
    {
        var (ok, err, _) = await _service.CreateCustomerTicketAsync(1, 100, "Help", "Please", 999, null);
        ok.Should().BeFalse();
        err.Should().Contain("Order");
    }

    [Fact]
    public async Task CustomerReply_HidesInternal_AndReopensResolved()
    {
        var created = await _service.CreateCustomerTicketAsync(1, 100, "Help", "Need help", 500, null);
        var id = created.Session!.SupportSessionID;
        await _service.AddInteractionAsync(new AddSupportInteractionRequest
        {
            SupportSessionID = id,
            InteractionType = SupportInteractionType.InternalNote,
            Body = "Internal only",
            IsInternal = true,
        }, 10);
        await _service.TransitionStatusAsync(id, SupportSessionStatus.Resolved, 10, "Done");

        var publicIx = await _service.GetClientInteractionsAsync(id, 100);
        publicIx.Should().NotContain(i => i.IsInternal);
        publicIx.Should().NotContain(i => i.Body == "Internal only");

        var (ok, err) = await _service.AddCustomerReplyAsync(id, 100, "Still waiting");
        ok.Should().BeTrue(err);
        var row = await _service.GetClientSessionAsync(id, 100);
        row!.Status.Should().Be((byte)SupportSessionStatus.Open);
    }

    public void Dispose() => _context.Dispose();
}
