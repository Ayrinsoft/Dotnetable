using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.DTOs;

/// <summary>Lifecycle of a support ticket / session.</summary>
public enum SupportSessionStatus : byte
{
    Open = 1,
    Pending = 2,
    WaitingCustomer = 3,
    Resolved = 4,
    Closed = 5,
}

/// <summary>Urgency for queue sorting and SLA focus.</summary>
public enum SupportPriority : byte
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4,
}

/// <summary>How the customer reached support (or how the agent opened the session).</summary>
public enum SupportChannel : byte
{
    InboundCall = 1,
    OutboundCall = 2,
    Chat = 3,
    Email = 4,
    Manual = 5,
    WalkIn = 6,
    Sms = 7,
}

/// <summary>High-level reason for the contact.</summary>
public enum SupportCategory : byte
{
    General = 1,
    Order = 2,
    Payment = 3,
    Shipping = 4,
    Product = 5,
    Account = 6,
    Wallet = 7,
    Refund = 8,
    Other = 9,
}

/// <summary>Kind of timeline row on a session.</summary>
public enum SupportInteractionType : byte
{
    Note = 1,
    InternalNote = 2,
    CallInbound = 3,
    CallOutbound = 4,
    Email = 5,
    Sms = 6,
    StatusChange = 7,
    Assignment = 8,
    OrderLinked = 9,
    System = 10,
}

/// <summary>Result of a phone attempt.</summary>
public enum SupportCallOutcome : byte
{
    Answered = 1,
    NoAnswer = 2,
    Busy = 3,
    Voicemail = 4,
    CallbackScheduled = 5,
    WrongNumber = 6,
}

/// <summary>Computed SLA health for a ticket (not stored).</summary>
public enum SupportSlaState : byte
{
    Ok = 0,
    FirstResponseAtRisk = 1,
    FirstResponseBreached = 2,
    ResolveAtRisk = 3,
    ResolveBreached = 4,
    Met = 5,
    Closed = 6,
}

/// <summary>Priority → first-response / resolve windows used by the support desk.</summary>
public static class SupportSlaPolicy
{
    /// <summary>At-risk window: last 20% of the SLA budget (min 5 minutes).</summary>
    public static readonly TimeSpan MinAtRiskWindow = TimeSpan.FromMinutes(5);

    public static (TimeSpan FirstResponse, TimeSpan Resolve) GetTargets(SupportPriority priority) => priority switch
    {
        SupportPriority.Urgent => (TimeSpan.FromMinutes(15), TimeSpan.FromHours(4)),
        SupportPriority.High => (TimeSpan.FromHours(1), TimeSpan.FromHours(8)),
        SupportPriority.Normal => (TimeSpan.FromHours(4), TimeSpan.FromHours(24)),
        SupportPriority.Low => (TimeSpan.FromHours(8), TimeSpan.FromHours(48)),
        _ => (TimeSpan.FromHours(4), TimeSpan.FromHours(24)),
    };

    public static void ApplyDueDates(SupportSession session)
    {
        var (first, resolve) = GetTargets((SupportPriority)session.Priority);
        session.FirstResponseDueAt = session.CreatedAt.Add(first);
        session.ResolveDueAt = session.CreatedAt.Add(resolve);
    }

    /// <summary>On reopen, give a fresh resolve window from <paramref name="now"/> while keeping first-response history.</summary>
    public static void RefreshResolveDueOnReopen(SupportSession session, DateTime now)
    {
        var (_, resolve) = GetTargets((SupportPriority)session.Priority);
        session.ResolveDueAt = now.Add(resolve);
    }

    public static SupportSlaState Evaluate(
        byte status,
        DateTime createdAt,
        DateTime? firstResponseAt,
        DateTime? firstResponseDueAt,
        DateTime? resolvedAt,
        DateTime? resolveDueAt,
        DateTime? now = null)
    {
        var utc = now ?? DateTime.UtcNow;
        if (status is (byte)SupportSessionStatus.Resolved or (byte)SupportSessionStatus.Closed)
        {
            if (resolveDueAt is DateTime rd && resolvedAt is DateTime ra && ra > rd)
                return SupportSlaState.ResolveBreached;
            if (firstResponseDueAt is DateTime fd && firstResponseAt is DateTime fa && fa > fd)
                return SupportSlaState.FirstResponseBreached;
            return SupportSlaState.Met;
        }

        if (firstResponseAt is null && firstResponseDueAt is DateTime frDue)
        {
            if (utc > frDue) return SupportSlaState.FirstResponseBreached;
            if (IsAtRisk(createdAt, frDue, utc)) return SupportSlaState.FirstResponseAtRisk;
        }

        if (resolvedAt is null && resolveDueAt is DateTime resDue)
        {
            if (utc > resDue) return SupportSlaState.ResolveBreached;
            if (IsAtRisk(createdAt, resDue, utc)) return SupportSlaState.ResolveAtRisk;
        }

        return SupportSlaState.Ok;
    }

    public static bool IsBreached(SupportSlaState state) =>
        state is SupportSlaState.FirstResponseBreached or SupportSlaState.ResolveBreached;

    private static bool IsAtRisk(DateTime start, DateTime due, DateTime now)
    {
        if (now >= due) return false;
        var total = due - start;
        if (total <= TimeSpan.Zero) return true;
        var remaining = due - now;
        var window = TimeSpan.FromTicks(Math.Max(MinAtRiskWindow.Ticks, total.Ticks / 5));
        return remaining <= window;
    }
}

/// <summary>Request to open a new support session (ticket).</summary>
public sealed class StartSupportSessionRequest
{
    public int WebsiteID { get; set; }
    public int? WebsiteClientID { get; set; }
    public string? Cellphone { get; set; }
    public string? CountryCode { get; set; }
    public string? Email { get; set; }
    public string? CustomerName { get; set; }
    public SupportChannel Channel { get; set; } = SupportChannel.InboundCall;
    public SupportPriority Priority { get; set; } = SupportPriority.Normal;
    public SupportCategory Category { get; set; } = SupportCategory.General;
    public string? Subject { get; set; }
    public string? Tags { get; set; }
    public int? RelatedOrderID { get; set; }
    public string? OpeningNote { get; set; }
    public bool OpeningNoteInternal { get; set; }
    public int? DurationSeconds { get; set; }
    public SupportCallOutcome? CallOutcome { get; set; }
    /// <summary>When true, auto-assign the creating agent.</summary>
    public bool AssignToSelf { get; set; } = true;
    /// <summary>Optional scheduled callback when logging an opening call.</summary>
    public DateTime? CallbackAt { get; set; }
    public string? CallbackNote { get; set; }
}

/// <summary>Request to append a timeline interaction.</summary>
public sealed class AddSupportInteractionRequest
{
    public int SupportSessionID { get; set; }
    public SupportInteractionType InteractionType { get; set; } = SupportInteractionType.Note;
    public string? Body { get; set; }
    public int? DurationSeconds { get; set; }
    public SupportCallOutcome? CallOutcome { get; set; }
    public int? RelatedOrderID { get; set; }
    public bool IsInternal { get; set; }
    public DateTime? CallbackAt { get; set; }
    public string? CallbackNote { get; set; }
}

/// <summary>Lightweight order row for Customer 360 / support desk panels.</summary>
public sealed class SupportOrderSummaryDto
{
    public int OrderID { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public byte Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public int ItemCount { get; set; }
    public bool IsInProgress { get; set; }
}

/// <summary>One agent who previously handled this customer, with counts.</summary>
public sealed class SupportAgentActivityDto
{
    public int MemberID { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int InteractionCount { get; set; }
    public int SessionCount { get; set; }
    public DateTime LastActivityAt { get; set; }
}

/// <summary>Compact session row for lists and 360 timelines.</summary>
public sealed class SupportSessionSummaryDto
{
    public int SupportSessionID { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public byte Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public byte Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public byte Channel { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public byte Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Tags { get; set; }
    public string? CellphoneSnapshot { get; set; }
    public string? CustomerNameSnapshot { get; set; }
    public int? WebsiteClientID { get; set; }
    public int? RelatedOrderID { get; set; }
    public string? RelatedOrderNumber { get; set; }
    public int? AssignedMemberID { get; set; }
    public string? AssignedMemberName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastInteractionAt { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? FirstResponseDueAt { get; set; }
    public DateTime? ResolveDueAt { get; set; }
    public DateTime? CallbackAt { get; set; }
    public string? CallbackNote { get; set; }
    public SupportSlaState SlaState { get; set; }
    public string SlaLabel { get; set; } = string.Empty;
    public bool IsSlaBreached { get; set; }
    public int InteractionCount { get; set; }
    public bool Archive { get; set; }
    public byte? SatisfactionRating { get; set; }
}

/// <summary>Dashboard counters for the support desk / ticket queue.</summary>
public sealed class SupportDeskStatsDto
{
    public int OpenCount { get; set; }
    public int MyOpenCount { get; set; }
    public int UnassignedCount { get; set; }
    public int WaitingCustomerCount { get; set; }
    public int UrgentCount { get; set; }
    public int SlaBreachedCount { get; set; }
    public int CallbackDueCount { get; set; }
    public int ResolvedTodayCount { get; set; }
}

/// <summary>Timeline entry DTO (avoids navigation cycles for Blazor grids).</summary>
public sealed class SupportInteractionDto
{
    public int SupportInteractionID { get; set; }
    public int SupportSessionID { get; set; }
    public byte InteractionType { get; set; }
    public string InteractionTypeName { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int? DurationSeconds { get; set; }
    public byte? CallOutcome { get; set; }
    public string? CallOutcomeName { get; set; }
    public byte? FromStatus { get; set; }
    public byte? ToStatus { get; set; }
    public int? RelatedOrderID { get; set; }
    public string? RelatedOrderNumber { get; set; }
    public int? CreatedByMemberID { get; set; }
    public string? CreatedByMemberName { get; set; }
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Full Customer 360 payload for the support desk: profile + orders + tickets + agents + extras.
/// </summary>
public sealed class Customer360Dto
{
    public int WebsiteClientID { get; set; }
    public int WebsiteID { get; set; }
    public string? Givenname { get; set; }
    public string? Surname { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Cellphone { get; set; }
    public string? CountryCode { get; set; }
    public string PhoneDisplay { get; set; } = string.Empty;
    public bool Active { get; set; }
    public byte ClientLevel { get; set; }
    public DateOnly RegisterDate { get; set; }

    public int OpenSessionCount { get; set; }
    public int TotalSessionCount { get; set; }
    public int OrderCount { get; set; }
    public int InProgressOrderCount { get; set; }
    public decimal? WalletBalanceUsd { get; set; }

    public IReadOnlyList<SupportOrderSummaryDto> Orders { get; set; } = Array.Empty<SupportOrderSummaryDto>();
    public IReadOnlyList<SupportSessionSummaryDto> Sessions { get; set; } = Array.Empty<SupportSessionSummaryDto>();
    public IReadOnlyList<SupportInteractionDto> RecentInteractions { get; set; } = Array.Empty<SupportInteractionDto>();
    public IReadOnlyList<SupportAgentActivityDto> AgentsWhoHelped { get; set; } = Array.Empty<SupportAgentActivityDto>();
    public IReadOnlyList<SupportContactMessageDto> ContactMessages { get; set; } = Array.Empty<SupportContactMessageDto>();
    public IReadOnlyList<SupportAddressDto> Addresses { get; set; } = Array.Empty<SupportAddressDto>();
}

public sealed class SupportContactMessageDto
{
    public int ContactUsMessagesID { get; set; }
    public string MessageSubject { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    public DateTime LogTime { get; set; }
    public bool Archive { get; set; }
}

public sealed class SupportAddressDto
{
    public int WebsiteClientAddressID { get; set; }
    public string? Title { get; set; }
    public string? RecipientName { get; set; }
    public string? Phone { get; set; }
    public string? FullAddress { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>Result of a phone lookup that may match zero, one, or many clients.</summary>
public sealed class SupportMobileLookupResult
{
    public string Query { get; set; } = string.Empty;
    public string NormalizedDigits { get; set; } = string.Empty;
    public IReadOnlyList<SupportClientMatchDto> Matches { get; set; } = Array.Empty<SupportClientMatchDto>();
    /// <summary>When exactly one match, the full 360 is populated for instant desk use.</summary>
    public Customer360Dto? Customer360 { get; set; }
}

public sealed class SupportClientMatchDto
{
    public int WebsiteClientID { get; set; }
    public int WebsiteID { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Cellphone { get; set; }
    public string? CountryCode { get; set; }
    public string PhoneDisplay { get; set; } = string.Empty;
    public bool Active { get; set; }
    public int OrderCount { get; set; }
    public int OpenSessionCount { get; set; }
}
