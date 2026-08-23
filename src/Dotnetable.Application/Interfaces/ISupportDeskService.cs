using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Customer support desk: mobile lookup, Customer 360, tickets (sessions), and interaction timeline.
/// Admin Blazor injects this directly; storefront tickets go through the public API using the customer methods.
/// </summary>
public interface ISupportDeskService
{
    /// <summary>
    /// Find customers by mobile number (digit-normalized Contains match on national cellphone).
    /// When a single client matches, <see cref="SupportMobileLookupResult.Customer360"/> is filled.
    /// </summary>
    Task<SupportMobileLookupResult> LookupByMobileAsync(int? websiteId, string mobile, CancellationToken ct = default);

    /// <summary>Full Customer 360 for an existing website client.</summary>
    Task<Customer360Dto?> GetCustomer360Async(int websiteClientId, CancellationToken ct = default);

    Task<PagedResult<SupportSessionSummaryDto>> GetSessionsPagedAsync(
        int? websiteId,
        SupportSessionStatus? status,
        SupportPriority? priority,
        int? assignedMemberId,
        bool? archive,
        GridQuery query,
        bool? slaBreachedOnly = null,
        bool? unassignedOnly = null,
        bool? callbackDueOnly = null,
        CancellationToken ct = default);

    /// <summary>Queue health counters for the desk dashboard.</summary>
    Task<SupportDeskStatsDto> GetDeskStatsAsync(int? websiteId, int? memberId, CancellationToken ct = default);

    Task<SupportSession?> GetSessionEntityByIdAsync(int sessionId, CancellationToken ct = default);

    Task<SupportSessionSummaryDto?> GetSessionSummaryByIdAsync(int sessionId, CancellationToken ct = default);

    Task<IReadOnlyList<SupportInteractionDto>> GetInteractionsAsync(int sessionId, CancellationToken ct = default);

    /// <summary>Open a new ticket; optionally logs the opening call/note and assigns the agent.</summary>
    Task<SupportSession> StartSessionAsync(StartSupportSessionRequest request, int memberId, CancellationToken ct = default);

    Task<SupportInteraction> AddInteractionAsync(AddSupportInteractionRequest request, int memberId, CancellationToken ct = default);

    Task<bool> TransitionStatusAsync(int sessionId, SupportSessionStatus newStatus, int memberId, string? note, CancellationToken ct = default);

    Task<bool> AssignAsync(int sessionId, int? assignedMemberId, int actorMemberId, CancellationToken ct = default);

    Task<bool> SetPriorityAsync(int sessionId, SupportPriority priority, int memberId, CancellationToken ct = default);

    Task<bool> LinkOrderAsync(int sessionId, int? orderId, int memberId, CancellationToken ct = default);

    Task SetArchiveAsync(int sessionId, bool archive, CancellationToken ct = default);

    Task SetSatisfactionAsync(int sessionId, byte? rating, CancellationToken ct = default);

    Task UpdateSessionMetaAsync(
        int sessionId,
        string? subject,
        SupportCategory? category,
        string? tags,
        CancellationToken ct = default);

    /// <summary>Schedule or clear a customer callback; logs an interaction.</summary>
    Task<bool> ScheduleCallbackAsync(int sessionId, DateTime? callbackAt, string? note, int memberId, CancellationToken ct = default);

    Task<(bool Success, string? Error, SupportSession? Session)> CreateCustomerTicketAsync(
        int websiteId, int websiteClientId, string subject, string body, int? relatedOrderId, SupportCategory? category, CancellationToken ct = default);

    Task<PagedResult<SupportSessionSummaryDto>> GetClientSessionsPagedAsync(
        int websiteClientId, GridQuery query, CancellationToken ct = default);

    Task<SupportSessionSummaryDto?> GetClientSessionAsync(int sessionId, int websiteClientId, CancellationToken ct = default);

    Task<IReadOnlyList<SupportInteractionDto>> GetClientInteractionsAsync(int sessionId, int websiteClientId, CancellationToken ct = default);

    Task<(bool Success, string? Error)> AddCustomerReplyAsync(
        int sessionId, int websiteClientId, string body, CancellationToken ct = default);
}
