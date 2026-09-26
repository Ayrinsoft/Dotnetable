using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IBookingService
{
    Task<BookingProfile> GetOrCreateProfileAsync(int websiteId, CancellationToken ct = default);
    Task SaveProfileAsync(BookingProfile profile, CancellationToken ct = default);

    Task<List<BookingResource>> GetResourcesAsync(int websiteId, CancellationToken ct = default);
    Task<BookingResource> SaveResourceAsync(BookingResource resource, CancellationToken ct = default);
    Task DeleteResourceAsync(int resourceId, CancellationToken ct = default);

    Task<List<BookingOffering>> GetOfferingsAsync(int websiteId, CancellationToken ct = default);
    Task<BookingOffering> SaveOfferingAsync(BookingOffering offering, CancellationToken ct = default);
    Task DeleteOfferingAsync(int offeringId, CancellationToken ct = default);

    Task<List<BookingClosure>> GetClosuresAsync(int websiteId, CancellationToken ct = default);
    Task AddClosureAsync(int websiteId, int? resourceId, DateOnly date, string? note, CancellationToken ct = default);
    Task DeleteClosureAsync(int closureId, CancellationToken ct = default);

    Task<BookingCatalogDto?> GetCatalogAsync(int websiteId, CancellationToken ct = default);
    Task<IReadOnlyList<BookingDayDto>> GetMonthAsync(int offeringId, int year, int month, CancellationToken ct = default);
    Task<IReadOnlyList<BookingSlotDto>> GetSlotsAsync(int offeringId, DateOnly date, CancellationToken ct = default);

    Task<BookingBookResult> BookAsync(int websiteId, int clientId, int offeringId, DateOnly date, TimeOnly start, string? note, CancellationToken ct = default);

    Task<IReadOnlyList<BookingDayRowDto>> GetDaySheetAsync(int websiteId, DateOnly date, int? resourceId, CancellationToken ct = default);

    Task<BookingBookResult> AdminAddAsync(
        int websiteId, int memberId, int offeringId, DateOnly date, TimeOnly start,
        int? clientId, string? guestName, string? phone, string? note, bool markPaid, CancellationToken ct = default);

    Task<string?> CancelUnpaidAsync(int appointmentId, int? memberId, CancellationToken ct = default);

    Task<string?> RescheduleAsync(int appointmentId, DateOnly date, TimeOnly start, CancellationToken ct = default);

    /// <summary>Creates the deposit invoice if needed and records it as paid in person.</summary>
    Task<string?> MarkReceivedAsync(int appointmentId, int memberId, CancellationToken ct = default);

    /// <summary>Confirms holds whose order is paid, and releases expired or rejected holds.</summary>
    Task SweepAsync(CancellationToken ct = default);
}
