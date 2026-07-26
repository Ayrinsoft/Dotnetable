using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class SupportDeskServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SupportDeskService _service;

    public SupportDeskServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _service = new SupportDeskService(_context);
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

    public void Dispose() => _context.Dispose();
}
