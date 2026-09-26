using System.Data;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public sealed class BookingService : IBookingService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ICurrencyConversionService _currency;
    private readonly IOrderService _orders;
    private readonly IPaymentService _payments;

    public BookingService(
        IDbContextFactory<AppDbContext> factory,
        ICurrencyConversionService currency,
        IOrderService orders,
        IPaymentService payments)
    {
        _factory = factory;
        _currency = currency;
        _orders = orders;
        _payments = payments;
    }

    public async Task<BookingProfile> GetOrCreateProfileAsync(int websiteId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var profile = await db.BookingProfiles.FirstOrDefaultAsync(p => p.WebsiteID == websiteId, ct);
        if (profile is not null) return profile;

        profile = new BookingProfile { WebsiteID = websiteId };
        db.BookingProfiles.Add(profile);
        await db.SaveChangesAsync(ct);
        return profile;
    }

    public async Task SaveProfileAsync(BookingProfile profile, CancellationToken ct = default)
    {
        NormalizeProfile(profile);
        await using var db = await _factory.CreateDbContextAsync(ct);
        var current = await db.BookingProfiles.FirstOrDefaultAsync(p => p.WebsiteID == profile.WebsiteID, ct)
            ?? throw new InvalidOperationException("Booking profile was not found.");
        current.TimeZoneId = profile.TimeZoneId;
        current.BookThrough = profile.BookThrough;
        current.RetentionDays = profile.RetentionDays;
        current.RequirePayment = profile.RequirePayment;
        current.AllowOnlinePayment = profile.AllowOnlinePayment;
        current.AllowOfflinePayment = profile.AllowOfflinePayment;
        current.HoldMinutes = profile.HoldMinutes;
        current.LeadMinutes = profile.LeadMinutes;
        current.DayStartMinutes = profile.DayStartMinutes;
        current.DayEndMinutes = profile.DayEndMinutes;
        current.WorkDays = profile.WorkDays;
        current.BreakStartMinutes = profile.BreakStartMinutes;
        current.BreakEndMinutes = profile.BreakEndMinutes;
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<BookingResource>> GetResourcesAsync(int websiteId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.BookingResources.AsNoTracking()
            .Where(r => r.WebsiteID == websiteId)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<BookingResource> SaveResourceAsync(BookingResource resource, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(resource.Name))
            throw new InvalidOperationException("Name is required.");
        resource.Name = resource.Name.Trim();
        if (resource.Name.Length > 120) resource.Name = resource.Name[..120];
        if (resource.DayEndMinutes <= resource.DayStartMinutes)
            throw new InvalidOperationException("The day must end after it starts.");
        if (resource.WorkDays == 0)
            throw new InvalidOperationException("Pick at least one open day.");

        await using var db = await _factory.CreateDbContextAsync(ct);
        if (resource.BookingResourceID == 0)
        {
            db.BookingResources.Add(resource);
            await db.SaveChangesAsync(ct);
            return resource;
        }

        var current = await db.BookingResources.FirstOrDefaultAsync(r => r.BookingResourceID == resource.BookingResourceID, ct)
            ?? throw new InvalidOperationException("Resource was not found.");
        current.Name = resource.Name;
        current.IsActive = resource.IsActive;
        current.SortOrder = resource.SortOrder;
        current.DayStartMinutes = resource.DayStartMinutes;
        current.DayEndMinutes = resource.DayEndMinutes;
        current.WorkDays = resource.WorkDays;
        current.BreakStartMinutes = resource.BreakStartMinutes;
        current.BreakEndMinutes = resource.BreakEndMinutes;
        await db.SaveChangesAsync(ct);
        return current;
    }

    public async Task DeleteResourceAsync(int resourceId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var has = await db.BookingAppointments.AnyAsync(a =>
            a.BookingResourceID == resourceId && a.Status != (byte)BookingAppointmentStatus.Cancelled, ct);
        if (has) throw new InvalidOperationException("This person still has appointments. Cancel them first.");
        var cancelled = await db.BookingAppointments.Where(a => a.BookingResourceID == resourceId).ToListAsync(ct);
        db.BookingAppointments.RemoveRange(cancelled);
        var row = await db.BookingResources.FirstOrDefaultAsync(r => r.BookingResourceID == resourceId, ct);
        if (row is null) return;
        db.BookingResources.Remove(row);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<BookingOffering>> GetOfferingsAsync(int websiteId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.BookingOfferings.AsNoTracking()
            .Where(o => o.WebsiteID == websiteId)
            .OrderBy(o => o.SortOrder).ThenBy(o => o.Name)
            .ToListAsync(ct);
    }

    public async Task<BookingOffering> SaveOfferingAsync(BookingOffering offering, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(offering.Name))
            throw new InvalidOperationException("Name is required.");
        offering.Name = offering.Name.Trim();
        if (offering.DurationMinutes is < 5 or > 480)
            throw new InvalidOperationException("Duration must be between 5 and 480 minutes.");
        if (offering.FullPrice < 0 || offering.DepositAmount < 0)
            throw new InvalidOperationException("Prices cannot be negative.");
        if (offering.DepositAmount > offering.FullPrice)
            throw new InvalidOperationException("The reservation amount cannot be more than the service price.");

        await using var db = await _factory.CreateDbContextAsync(ct);
        var resource = await db.BookingResources.AsNoTracking()
            .FirstOrDefaultAsync(r => r.BookingResourceID == offering.BookingResourceID && r.WebsiteID == offering.WebsiteID, ct)
            ?? throw new InvalidOperationException("Pick a person for this service.");
        _ = resource;

        if (offering.BookingOfferingID == 0)
        {
            db.BookingOfferings.Add(offering);
            await db.SaveChangesAsync(ct);
            return offering;
        }

        var current = await db.BookingOfferings.FirstOrDefaultAsync(o => o.BookingOfferingID == offering.BookingOfferingID, ct)
            ?? throw new InvalidOperationException("Service was not found.");
        current.BookingResourceID = offering.BookingResourceID;
        current.Name = offering.Name;
        current.Description = string.IsNullOrWhiteSpace(offering.Description) ? null : offering.Description.Trim();
        current.DurationMinutes = offering.DurationMinutes;
        current.FullPrice = offering.FullPrice;
        current.DepositAmount = offering.DepositAmount;
        current.IsActive = offering.IsActive;
        current.SortOrder = offering.SortOrder;
        await db.SaveChangesAsync(ct);
        return current;
    }

    public async Task DeleteOfferingAsync(int offeringId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var has = await db.BookingAppointments.AnyAsync(a =>
            a.BookingOfferingID == offeringId && a.Status != (byte)BookingAppointmentStatus.Cancelled, ct);
        if (has) throw new InvalidOperationException("This service still has appointments.");
        var cancelled = await db.BookingAppointments.Where(a => a.BookingOfferingID == offeringId).ToListAsync(ct);
        db.BookingAppointments.RemoveRange(cancelled);
        var row = await db.BookingOfferings.FirstOrDefaultAsync(o => o.BookingOfferingID == offeringId, ct);
        if (row is null) return;
        db.BookingOfferings.Remove(row);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<BookingClosure>> GetClosuresAsync(int websiteId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.BookingClosures.AsNoTracking()
            .Where(c => c.WebsiteID == websiteId && c.Date >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)))
            .OrderBy(c => c.Date)
            .ToListAsync(ct);
    }

    public async Task AddClosureAsync(int websiteId, int? resourceId, DateOnly date, string? note, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var exists = await db.BookingClosures.AnyAsync(c =>
            c.WebsiteID == websiteId && c.Date == date && c.BookingResourceID == resourceId, ct);
        if (exists) return;
        db.BookingClosures.Add(new BookingClosure
        {
            WebsiteID = websiteId,
            BookingResourceID = resourceId,
            Date = date,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteClosureAsync(int closureId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var row = await db.BookingClosures.FirstOrDefaultAsync(c => c.BookingClosureID == closureId, ct);
        if (row is null) return;
        db.BookingClosures.Remove(row);
        await db.SaveChangesAsync(ct);
    }

    public async Task<BookingCatalogDto?> GetCatalogAsync(int websiteId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var profile = await db.BookingProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.WebsiteID == websiteId, ct);
        if (profile is null) return null;
        var currency = await db.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => w.DefaultCurrencyCode)
            .FirstOrDefaultAsync(ct) ?? "";
        var resources = await db.BookingResources.AsNoTracking()
            .Where(r => r.WebsiteID == websiteId && r.IsActive)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name)
            .ToListAsync(ct);
        var offerings = await db.BookingOfferings.AsNoTracking()
            .Where(o => o.WebsiteID == websiteId && o.IsActive)
            .OrderBy(o => o.SortOrder).ThenBy(o => o.Name)
            .ToListAsync(ct);

        return new BookingCatalogDto
        {
            RequirePayment = profile.RequirePayment && (profile.AllowOnlinePayment || profile.AllowOfflinePayment),
            AllowOnlinePayment = profile.AllowOnlinePayment,
            AllowOfflinePayment = profile.AllowOfflinePayment,
            BookThrough = profile.BookThrough?.ToString("yyyy-MM-dd"),
            CurrencyCode = currency,
            Resources = resources.Select(r => new BookingCatalogResourceDto
            {
                ResourceId = r.BookingResourceID,
                Name = r.Name,
                Hours = $"{Fmt(r.DayStartMinutes)}–{Fmt(r.DayEndMinutes)}",
                Offerings = offerings.Where(o => o.BookingResourceID == r.BookingResourceID)
                    .Select(o => new BookingCatalogOfferingDto
                    {
                        OfferingId = o.BookingOfferingID,
                        Name = o.Name,
                        Description = o.Description,
                        DurationMinutes = o.DurationMinutes,
                        FullPrice = o.FullPrice,
                        DepositAmount = o.DepositAmount,
                    }).ToList(),
            }).Where(r => r.Offerings.Count > 0).ToList(),
        };
    }

    public async Task<IReadOnlyList<BookingDayDto>> GetMonthAsync(int offeringId, int year, int month, CancellationToken ct = default)
    {
        var (offering, resource, profile, zone) = await LoadOfferingContextAsync(offeringId, ct);
        await SyncWebsiteAsync(resource.WebsiteID, ct);
        var days = DateTime.DaysInMonth(year, month);
        var closures = await LoadClosuresAsync(resource.WebsiteID, resource.BookingResourceID, ct);
        var list = new List<BookingDayDto>(days);
        for (var d = 1; d <= days; d++)
        {
            var date = new DateOnly(year, month, d);
            var open = IsBookableDay(date, resource, profile, zone, closures);
            if (open)
            {
                var slots = await BuildSlotsAsync(offering, resource, profile, zone, date, closures, ct);
                open = slots.Count > 0;
            }
            list.Add(new BookingDayDto { Date = date.ToString("yyyy-MM-dd"), Open = open });
        }
        return list;
    }

    public async Task<IReadOnlyList<BookingSlotDto>> GetSlotsAsync(int offeringId, DateOnly date, CancellationToken ct = default)
    {
        var (offering, resource, profile, zone) = await LoadOfferingContextAsync(offeringId, ct);
        await SyncWebsiteAsync(resource.WebsiteID, ct);
        var closures = await LoadClosuresAsync(resource.WebsiteID, resource.BookingResourceID, ct);
        if (!IsBookableDay(date, resource, profile, zone, closures))
            return Array.Empty<BookingSlotDto>();
        return await BuildSlotsAsync(offering, resource, profile, zone, date, closures, ct);
    }

    public async Task<BookingBookResult> BookAsync(
        int websiteId, int clientId, int offeringId, DateOnly date, TimeOnly start, string? note, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var client = await db.WebsiteClients.AsNoTracking()
            .FirstOrDefaultAsync(c => c.WebsiteClientID == clientId && c.WebsiteID == websiteId && c.Active, ct);
        if (client is null)
            return Fail("Sign in with an active account on this website.");

        var name = $"{client.Givenname} {client.Surname}".Trim();
        if (string.IsNullOrWhiteSpace(name)) name = client.Email ?? client.Cellphone ?? "Customer";

        var created = await InsertAppointmentAsync(
            websiteId, offeringId, date, start, client.WebsiteClientID, name, client.Cellphone, note,
            createdByMemberId: null, forceConfirmed: false, ct);
        if (!created.Success || created.AppointmentId is null) return created;

        var profile = await db.BookingProfiles.AsNoTracking().FirstAsync(p => p.WebsiteID == websiteId, ct);
        var offering = await db.BookingOfferings.AsNoTracking().FirstAsync(o => o.BookingOfferingID == offeringId, ct);
        var charge = ShouldCharge(profile, offering);
        if (!charge)
        {
            await SetStatusAsync(created.AppointmentId.Value, BookingAppointmentStatus.Confirmed, ct);
            return created;
        }

        var order = await CreateDepositOrderAsync(websiteId, clientId, offering, date, start, createdByMemberId: null, ct);
        if (!order.Success)
        {
            await SetStatusAsync(created.AppointmentId.Value, BookingAppointmentStatus.Cancelled, ct);
            return Fail(order.Error ?? "Could not create the invoice.");
        }

        await using var update = await _factory.CreateDbContextAsync(ct);
        var row = await update.BookingAppointments.FirstAsync(a => a.BookingAppointmentID == created.AppointmentId, ct);
        row.OrderID = order.OrderId;
        await update.SaveChangesAsync(ct);
        created.OrderId = order.OrderId;
        created.OrderNumber = order.OrderNumber;
        created.NeedsPayment = true;
        return created;
    }

    public async Task<IReadOnlyList<BookingDayRowDto>> GetDaySheetAsync(
        int websiteId, DateOnly date, int? resourceId, CancellationToken ct = default)
    {
        await SyncWebsiteAsync(websiteId, ct);
        await using var db = await _factory.CreateDbContextAsync(ct);
        var profile = await db.BookingProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.WebsiteID == websiteId, ct);
        var zone = Zone(profile?.TimeZoneId);
        var startLocal = date.ToDateTime(TimeOnly.MinValue);
        var endLocal = date.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, zone);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, zone);

        var rows = await db.BookingAppointments.AsNoTracking()
            .Include(a => a.Resource)
            .Include(a => a.Offering)
            .Include(a => a.Order)
            .Where(a => a.WebsiteID == websiteId
                && a.StartsAtUtc >= startUtc && a.StartsAtUtc < endUtc
                && a.Status != (byte)BookingAppointmentStatus.Cancelled
                && (resourceId == null || a.BookingResourceID == resourceId))
            .OrderBy(a => a.StartsAtUtc)
            .ToListAsync(ct);

        return rows.Select(a =>
        {
            var localStart = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(a.StartsAtUtc, DateTimeKind.Utc), zone);
            var localEnd = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(a.EndsAtUtc, DateTimeKind.Utc), zone);
            var paid = a.Order?.Status is (byte)OrderStatus.Paid or (byte)OrderStatus.Processing
                or (byte)OrderStatus.Shipped or (byte)OrderStatus.Completed;
            return new BookingDayRowDto
            {
                AppointmentId = a.BookingAppointmentID,
                ResourceId = a.BookingResourceID,
                ResourceName = a.Resource.Name,
                OfferingName = a.Offering.Name,
                Start = localStart.ToString("HH:mm"),
                End = localEnd.ToString("HH:mm"),
                CustomerName = a.CustomerName,
                Phone = a.Phone,
                Note = a.Note,
                Status = ((BookingAppointmentStatus)a.Status).ToString(),
                FullPrice = a.Offering.FullPrice,
                DepositAmount = a.Offering.DepositAmount,
                OrderId = a.OrderID,
                OrderNumber = a.Order?.OrderNumber,
                Paid = paid || a.OrderID is null && a.Status == (byte)BookingAppointmentStatus.Confirmed && a.Offering.DepositAmount <= 0,
                CanDelete = !paid,
                CanMove = true,
            };
        }).ToList();
    }

    public async Task<BookingBookResult> AdminAddAsync(
        int websiteId, int memberId, int offeringId, DateOnly date, TimeOnly start,
        int? clientId, string? guestName, string? phone, string? note, bool markPaid, CancellationToken ct = default)
    {
        string name = guestName?.Trim() ?? "";
        string? resolvedPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        if (clientId is int cid)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);
            var client = await db.WebsiteClients.AsNoTracking()
                .FirstOrDefaultAsync(c => c.WebsiteClientID == cid && c.WebsiteID == websiteId, ct);
            if (client is null) return Fail("Customer was not found.");
            if (string.IsNullOrWhiteSpace(name))
                name = $"{client.Givenname} {client.Surname}".Trim();
            resolvedPhone ??= client.Cellphone;
        }
        if (string.IsNullOrWhiteSpace(name)) return Fail("A customer name is required.");

        var actor = memberId > 0 ? memberId : (int?)null;
        var created = await InsertAppointmentAsync(
            websiteId, offeringId, date, start, clientId, name, resolvedPhone, note, actor, forceConfirmed: true, ct);
        if (!created.Success || created.AppointmentId is null) return created;

        await using var read = await _factory.CreateDbContextAsync(ct);
        var offering = await read.BookingOfferings.AsNoTracking().FirstAsync(o => o.BookingOfferingID == offeringId, ct);
        if (offering.DepositAmount <= 0 || clientId is null)
            return created;

        var order = await CreateDepositOrderAsync(websiteId, clientId.Value, offering, date, start, actor, ct);
        if (!order.Success) return created;

        await using var update = await _factory.CreateDbContextAsync(ct);
        var row = await update.BookingAppointments.FirstAsync(a => a.BookingAppointmentID == created.AppointmentId, ct);
        row.OrderID = order.OrderId;
        await update.SaveChangesAsync(ct);
        created.OrderId = order.OrderId;
        created.OrderNumber = order.OrderNumber;

        if (markPaid)
        {
            if (memberId <= 0)
                created.Error = "Sign in again to record a payment.";
            else
            {
                var pay = await _payments.RecordReceivedPaymentAsync(
                    order.OrderId!.Value, PaymentMethod.Manual, offering.DepositAmount, null, "Booking desk", memberId, ct: ct);
                if (!pay.Success) created.Error = pay.Error;
            }
        }
        else
            created.NeedsPayment = true;
        return created;
    }

    public async Task<string?> CancelUnpaidAsync(int appointmentId, int? memberId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var row = await db.BookingAppointments.Include(a => a.Order)
            .FirstOrDefaultAsync(a => a.BookingAppointmentID == appointmentId, ct);
        if (row is null) return "Appointment was not found.";
        if (row.Order?.Status is (byte)OrderStatus.Paid or (byte)OrderStatus.Processing
            or (byte)OrderStatus.Shipped or (byte)OrderStatus.Completed)
            return "This appointment is paid. Refund the order before removing it.";

        if (row.OrderID is int orderId && row.Order?.Status == (byte)OrderStatus.PendingPayment)
            await _orders.TransitionStatusAsync(orderId, OrderStatus.Cancelled, memberId, "Booking removed.", ct);

        row.Status = (byte)BookingAppointmentStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> RescheduleAsync(int appointmentId, DateOnly date, TimeOnly start, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var row = await db.BookingAppointments.Include(a => a.Offering).Include(a => a.Resource)
            .FirstOrDefaultAsync(a => a.BookingAppointmentID == appointmentId, ct);
        if (row is null || row.Status == (byte)BookingAppointmentStatus.Cancelled)
            return "Appointment was not found.";

        var profile = await db.BookingProfiles.FirstAsync(p => p.WebsiteID == row.WebsiteID, ct);
        var zone = Zone(profile.TimeZoneId);
        var closures = await db.BookingClosures.AsNoTracking()
            .Where(c => c.WebsiteID == row.WebsiteID && (c.BookingResourceID == null || c.BookingResourceID == row.BookingResourceID))
            .ToListAsync(ct);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(start), zone);
        var endUtc = startUtc.AddMinutes(row.Offering.DurationMinutes);
        var reason = ValidateWindow(date, start, startUtc, endUtc, row.Resource, row.Offering, profile, zone, closures, adminOverride: true);
        if (reason is not null) return reason;

        var taken = await OverlapsAsync(db, row.BookingResourceID, startUtc, endUtc, row.BookingAppointmentID, profile, ct);
        if (taken) return "That time is already taken.";

        row.StartsAtUtc = startUtc;
        row.EndsAtUtc = endUtc;
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> MarkReceivedAsync(int appointmentId, int memberId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var row = await db.BookingAppointments.Include(a => a.Offering).Include(a => a.Order)
            .FirstOrDefaultAsync(a => a.BookingAppointmentID == appointmentId, ct);
        if (row is null) return "Appointment was not found.";
        if (row.Order?.Status is (byte)OrderStatus.Paid or (byte)OrderStatus.Processing
            or (byte)OrderStatus.Shipped or (byte)OrderStatus.Completed)
            return null;
        if (row.WebsiteClientID is null) return "Attach a customer account before recording a payment.";
        if (row.Offering.DepositAmount <= 0) return "This service has no reservation amount.";

        var zone = Zone((await db.BookingProfiles.AsNoTracking().FirstAsync(p => p.WebsiteID == row.WebsiteID, ct)).TimeZoneId);
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(row.StartsAtUtc, DateTimeKind.Utc), zone);
        if (row.OrderID is null)
        {
            var order = await CreateDepositOrderAsync(
                row.WebsiteID, row.WebsiteClientID.Value, row.Offering,
                DateOnly.FromDateTime(local), TimeOnly.FromDateTime(local), memberId, ct);
            if (!order.Success || order.OrderId is null) return order.Error ?? "Could not create the invoice.";
            row.OrderID = order.OrderId;
            await db.SaveChangesAsync(ct);
        }

        var pay = await _payments.RecordReceivedPaymentAsync(
            row.OrderID!.Value, PaymentMethod.Manual, row.Offering.DepositAmount, null, "Booking desk", memberId, ct: ct);
        if (!pay.Success) return pay.Error;
        row.Status = (byte)BookingAppointmentStatus.Confirmed;
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task SweepAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var websiteIds = await db.BookingProfiles.AsNoTracking().Select(p => p.WebsiteID).ToListAsync(ct);
        foreach (var id in websiteIds)
        {
            await SyncWebsiteAsync(id, ct);
            await PurgeOldAsync(id, ct);
        }
    }

    private async Task SyncWebsiteAsync(int websiteId, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var profile = await db.BookingProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.WebsiteID == websiteId, ct);
        if (profile is null) return;
        var now = DateTime.UtcNow;
        var holdCutoff = now.AddMinutes(-Math.Max(5, profile.HoldMinutes));
        var open = await db.BookingAppointments
            .Where(a => a.WebsiteID == websiteId &&
                (a.Status == (byte)BookingAppointmentStatus.Hold || a.Status == (byte)BookingAppointmentStatus.AwaitingVerification))
            .ToListAsync(ct);

        foreach (var row in open)
        {
            if (row.OrderID is int orderId)
            {
                var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
                if (order?.Status is (byte)OrderStatus.Paid or (byte)OrderStatus.Processing
                    or (byte)OrderStatus.Shipped or (byte)OrderStatus.Completed)
                {
                    row.Status = (byte)BookingAppointmentStatus.Confirmed;
                    continue;
                }
                if (order?.Status is (byte)OrderStatus.Cancelled or (byte)OrderStatus.Refunded)
                {
                    row.Status = (byte)BookingAppointmentStatus.Cancelled;
                    continue;
                }

                var pendingReceipt = await db.Payments.AnyAsync(p =>
                    p.OrderID == orderId && p.Status == (byte)PaymentStatus.Pending, ct);
                if (pendingReceipt)
                {
                    row.Status = (byte)BookingAppointmentStatus.AwaitingVerification;
                    continue;
                }
            }

            if (row.Status == (byte)BookingAppointmentStatus.Hold && row.CreatedAt < holdCutoff)
            {
                if (row.OrderID is int expireId)
                    await _orders.TransitionStatusAsync(expireId, OrderStatus.Cancelled, null, "Booking hold expired.", ct);
                row.Status = (byte)BookingAppointmentStatus.Cancelled;
            }
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task PurgeOldAsync(int websiteId, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var profile = await db.BookingProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.WebsiteID == websiteId, ct);
        if (profile is null || profile.RetentionDays <= 0) return;
        var cutoff = DateTime.UtcNow.AddDays(-profile.RetentionDays);
        var old = await db.BookingAppointments
            .Where(a => a.WebsiteID == websiteId && a.EndsAtUtc < cutoff
                && a.Status != (byte)BookingAppointmentStatus.Hold
                && a.Status != (byte)BookingAppointmentStatus.AwaitingVerification)
            .ToListAsync(ct);
        if (old.Count == 0) return;
        db.BookingAppointments.RemoveRange(old);
        await db.SaveChangesAsync(ct);
    }

    private async Task<BookingBookResult> InsertAppointmentAsync(
        int websiteId, int offeringId, DateOnly date, TimeOnly start,
        int? clientId, string customerName, string? phone, string? note,
        int? createdByMemberId, bool forceConfirmed, CancellationToken ct)
    {
        await SyncWebsiteAsync(websiteId, ct);
        await using var db = await _factory.CreateDbContextAsync(ct);
        var offering = await db.BookingOfferings.Include(o => o.Resource)
            .FirstOrDefaultAsync(o => o.BookingOfferingID == offeringId && o.WebsiteID == websiteId && o.IsActive, ct);
        if (offering is null || !offering.Resource.IsActive) return Fail("This service is not available.");
        var profile = await db.BookingProfiles.FirstOrDefaultAsync(p => p.WebsiteID == websiteId, ct);
        if (profile is null) return Fail("Booking is not set up yet.");
        var zone = Zone(profile.TimeZoneId);
        var closures = await db.BookingClosures.AsNoTracking()
            .Where(c => c.WebsiteID == websiteId && (c.BookingResourceID == null || c.BookingResourceID == offering.BookingResourceID))
            .ToListAsync(ct);

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(start), zone);
        var endUtc = startUtc.AddMinutes(offering.DurationMinutes);
        var reason = ValidateWindow(date, start, startUtc, endUtc, offering.Resource, offering, profile, zone, closures, adminOverride: forceConfirmed);
        if (reason is not null) return Fail(reason);

        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            if (await OverlapsAsync(db, offering.BookingResourceID, startUtc, endUtc, null, profile, ct))
            {
                await tx.RollbackAsync(ct);
                return Fail("That time was just taken.");
            }

            var row = new BookingAppointment
            {
                WebsiteID = websiteId,
                BookingResourceID = offering.BookingResourceID,
                BookingOfferingID = offering.BookingOfferingID,
                WebsiteClientID = clientId,
                CustomerName = customerName.Length > 120 ? customerName[..120] : customerName,
                Phone = phone,
                Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                StartsAtUtc = startUtc,
                EndsAtUtc = endUtc,
                Status = (byte)(forceConfirmed ? BookingAppointmentStatus.Confirmed : BookingAppointmentStatus.Hold),
                CreatedAt = DateTime.UtcNow,
                CreatedByMemberID = createdByMemberId,
            };
            db.BookingAppointments.Add(row);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new BookingBookResult { Success = true, AppointmentId = row.BookingAppointmentID };
        });
    }

    private async Task<(bool Success, string? Error, int? OrderId, string? OrderNumber)> CreateDepositOrderAsync(
        int websiteId, int clientId, BookingOffering offering, DateOnly date, TimeOnly start, int? createdByMemberId, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var (currency, rate) = await _currency.GetActiveRateAsync(websiteId, null, ct);
        if (rate <= 0) rate = 1m;
        decimal ToUsd(decimal local) => Math.Round(local / rate, 4, MidpointRounding.AwayFromZero);
        var amount = offering.DepositAmount;
        var title = $"نوبت {offering.Name} — {date:yyyy-MM-dd} {start:HH:mm} — هزینه خدمت {offering.FullPrice:0.##}";
        if (title.Length > 300) title = title[..300];

        var order = new Order
        {
            WebsiteID = websiteId,
            OrderNumber = $"{websiteId}-{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(100, 999)}",
            WebsiteClientID = clientId,
            Status = (byte)OrderStatus.PendingPayment,
            CurrencyCode = currency,
            ExchangeRateToUsd = rate,
            SubTotal = amount,
            DiscountTotal = 0,
            ShippingTotal = 0,
            TaxTotal = 0,
            PricesIncludeTax = true,
            GrandTotal = amount,
            GrandTotalUsd = ToUsd(amount),
            Note = title,
            SalesChannel = (byte)OrderSalesChannel.Online,
            ReportToTax = true,
            PreparationStatus = (byte)OrderPreparationStatus.NotStarted,
            ShippingStatus = (byte)OrderShippingStatus.NotShipped,
            MarkupTotal = 0,
            CreatedByMemberID = createdByMemberId,
            CreatedAt = DateTime.UtcNow,
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        db.OrderItems.Add(new OrderItem
        {
            OrderID = order.OrderID,
            WebsiteID = websiteId,
            SourceWebsiteID = websiteId,
            TitleSnapshot = title,
            SkuSnapshot = "BOOK",
            Quantity = 1,
            UnitPrice = amount,
            UnitPriceUsd = ToUsd(amount),
            UnitCostUsd = 0,
            CatalogUnitPrice = amount,
            UnitMarkup = 0,
            DiscountAmount = 0,
            TotalPrice = amount,
        });
        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderID = order.OrderID,
            ToStatus = (byte)OrderStatus.PendingPayment,
            Note = "Booking deposit.",
            CreatedByMemberID = createdByMemberId,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync(ct);
        return (true, null, order.OrderID, order.OrderNumber);
    }

    private async Task<List<BookingSlotDto>> BuildSlotsAsync(
        BookingOffering offering, BookingResource resource, BookingProfile profile, TimeZoneInfo zone,
        DateOnly date, List<BookingClosure> closures, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = date.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var from = TimeZoneInfo.ConvertTimeToUtc(dayStart, zone);
        var to = TimeZoneInfo.ConvertTimeToUtc(dayEnd, zone);
        var existing = await db.BookingAppointments.AsNoTracking()
            .Where(a => a.BookingResourceID == resource.BookingResourceID && a.StartsAtUtc < to && a.EndsAtUtc > from)
            .ToListAsync(ct);
        var now = DateTime.UtcNow;
        existing = existing.Where(a => Blocks(a, profile, now)).ToList();

        var slots = new List<BookingSlotDto>();
        var cursor = resource.DayStartMinutes;
        while (cursor + offering.DurationMinutes <= resource.DayEndMinutes)
        {
            var end = cursor + offering.DurationMinutes;
            if (OverlapsBreak(cursor, end, resource.BreakStartMinutes, resource.BreakEndMinutes))
            {
                cursor = resource.BreakEndMinutes ?? end;
                continue;
            }
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(new TimeOnly(cursor / 60, cursor % 60)), zone);
            var endUtc = startUtc.AddMinutes(offering.DurationMinutes);
            if (startUtc >= now.AddMinutes(profile.LeadMinutes)
                && existing.All(a => a.EndsAtUtc <= startUtc || a.StartsAtUtc >= endUtc))
            {
                slots.Add(new BookingSlotDto { Start = Fmt(cursor), End = Fmt(end) });
            }
            cursor += offering.DurationMinutes;
        }
        _ = closures;
        return slots;
    }

    private async Task<(BookingOffering Offering, BookingResource Resource, BookingProfile Profile, TimeZoneInfo Zone)> LoadOfferingContextAsync(
        int offeringId, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var offering = await db.BookingOfferings.AsNoTracking().Include(o => o.Resource)
            .FirstOrDefaultAsync(o => o.BookingOfferingID == offeringId, ct)
            ?? throw new InvalidOperationException("Service was not found.");
        var profile = await db.BookingProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.WebsiteID == offering.WebsiteID, ct)
            ?? new BookingProfile { WebsiteID = offering.WebsiteID };
        return (offering, offering.Resource, profile, Zone(profile.TimeZoneId));
    }

    private async Task<List<BookingClosure>> LoadClosuresAsync(int websiteId, int resourceId, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.BookingClosures.AsNoTracking()
            .Where(c => c.WebsiteID == websiteId && (c.BookingResourceID == null || c.BookingResourceID == resourceId))
            .ToListAsync(ct);
    }

    private static bool IsBookableDay(
        DateOnly date, BookingResource resource, BookingProfile profile, TimeZoneInfo zone, List<BookingClosure> closures, bool enforceHorizon = true)
    {
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone));
        if (date < today) return false;
        if (enforceHorizon && profile.BookThrough is null) return false;
        if (enforceHorizon && profile.BookThrough is DateOnly through && date > through) return false;
        var bit = WeekBit(date.DayOfWeek);
        if ((resource.WorkDays & bit) == 0) return false;
        if (closures.Any(c => c.Date == date)) return false;
        if (!resource.IsActive) return false;
        return true;
    }

    private static string? ValidateWindow(
        DateOnly date, TimeOnly start, DateTime startUtc, DateTime endUtc,
        BookingResource resource, BookingOffering offering, BookingProfile profile, TimeZoneInfo zone,
        List<BookingClosure> closures, bool adminOverride)
    {
        if (!IsBookableDay(date, resource, profile, zone, closures, enforceHorizon: !adminOverride))
            return "That day is closed.";
        var startMin = start.Hour * 60 + start.Minute;
        var endMin = startMin + offering.DurationMinutes;
        if (startMin < resource.DayStartMinutes || endMin > resource.DayEndMinutes)
            return "That time is outside working hours.";
        if (OverlapsBreak(startMin, endMin, resource.BreakStartMinutes, resource.BreakEndMinutes))
            return "That time falls in the break.";
        if (!adminOverride && startUtc < DateTime.UtcNow.AddMinutes(profile.LeadMinutes))
            return "That time is too soon.";
        _ = endUtc;
        return null;
    }

    private static async Task<bool> OverlapsAsync(
        AppDbContext db, int resourceId, DateTime startUtc, DateTime endUtc, int? ignoreId, BookingProfile profile, CancellationToken ct)
    {
        var rows = await db.BookingAppointments
            .Where(a => a.BookingResourceID == resourceId
                && a.BookingAppointmentID != (ignoreId ?? 0)
                && a.StartsAtUtc < endUtc && a.EndsAtUtc > startUtc)
            .ToListAsync(ct);
        var now = DateTime.UtcNow;
        return rows.Any(a => Blocks(a, profile, now));
    }

    private static bool Blocks(BookingAppointment appointment, BookingProfile profile, DateTime now)
    {
        if (appointment.Status == (byte)BookingAppointmentStatus.Cancelled) return false;
        if (appointment.Status == (byte)BookingAppointmentStatus.Hold
            && appointment.CreatedAt < now.AddMinutes(-Math.Max(5, profile.HoldMinutes)))
            return false;
        return true;
    }

    private static bool ShouldCharge(BookingProfile profile, BookingOffering offering) =>
        profile.RequirePayment
        && offering.DepositAmount > 0
        && (profile.AllowOnlinePayment || profile.AllowOfflinePayment);

    private async Task SetStatusAsync(int appointmentId, BookingAppointmentStatus status, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var row = await db.BookingAppointments.FirstOrDefaultAsync(a => a.BookingAppointmentID == appointmentId, ct);
        if (row is null) return;
        row.Status = (byte)status;
        await db.SaveChangesAsync(ct);
    }

    private static bool OverlapsBreak(int start, int end, int? breakStart, int? breakEnd)
    {
        if (breakStart is null || breakEnd is null || breakEnd <= breakStart) return false;
        return start < breakEnd && end > breakStart;
    }

    private static int WeekBit(DayOfWeek day) => day switch
    {
        DayOfWeek.Saturday => 1,
        DayOfWeek.Sunday => 2,
        DayOfWeek.Monday => 4,
        DayOfWeek.Tuesday => 8,
        DayOfWeek.Wednesday => 16,
        DayOfWeek.Thursday => 32,
        DayOfWeek.Friday => 64,
        _ => 0,
    };

    private static TimeZoneInfo Zone(string? id)
    {
        foreach (var candidate in new[] { id, "Asia/Tehran", "Iran Standard Time" })
        {
            if (string.IsNullOrWhiteSpace(candidate)) continue;
            try { return TimeZoneInfo.FindSystemTimeZoneById(candidate); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.Utc;
    }

    private static string Fmt(int minutes) => $"{minutes / 60:00}:{minutes % 60:00}";

    private static void NormalizeProfile(BookingProfile profile)
    {
        profile.HoldMinutes = Math.Clamp(profile.HoldMinutes, 5, 24 * 60);
        profile.LeadMinutes = Math.Clamp(profile.LeadMinutes, 0, 7 * 24 * 60);
        profile.RetentionDays = Math.Clamp(profile.RetentionDays, 0, 3650);
        if (profile.DayEndMinutes <= profile.DayStartMinutes)
            throw new InvalidOperationException("The day must end after it starts.");
        if (string.IsNullOrWhiteSpace(profile.TimeZoneId))
            profile.TimeZoneId = "Asia/Tehran";
    }

    private static BookingBookResult Fail(string error) => new() { Success = false, Error = error };
}
