using System.Text.RegularExpressions;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class SupportDeskService : ISupportDeskService
{
    private static readonly byte[] OpenStatuses =
    [
        (byte)SupportSessionStatus.Open,
        (byte)SupportSessionStatus.Pending,
        (byte)SupportSessionStatus.WaitingCustomer,
    ];

    private static readonly byte[] InProgressOrderStatuses =
    [
        (byte)OrderStatus.PendingPayment,
        (byte)OrderStatus.Paid,
        (byte)OrderStatus.Processing,
        (byte)OrderStatus.Shipped,
    ];

    private readonly AppDbContext _context;
    private readonly IAdminNotificationService _notifications;

    public SupportDeskService(AppDbContext context, IAdminNotificationService notifications)
    {
        _context = context;
        _notifications = notifications;
    }

    public async Task<SupportMobileLookupResult> LookupByMobileAsync(int? websiteId, string mobile, CancellationToken ct = default)
    {
        var query = mobile?.Trim() ?? string.Empty;
        var digits = DigitsOnly(query);
        // Common paste forms: +98912… → 98912… → also try national 0912… / 912…
        var candidates = BuildPhoneSearchCandidates(digits);

        var result = new SupportMobileLookupResult
        {
            Query = query,
            NormalizedDigits = digits,
        };

        if (candidates.Count == 0 || candidates.All(c => c.Length < 4))
            return result;

        var clientsQ = _context.WebsiteClients.AsNoTracking().AsQueryable();
        if (websiteId is int wid)
            clientsQ = clientsQ.Where(c => c.WebsiteID == wid);

        // Expand up to 4 candidates into explicit OR clauses (reliable SQL translation).
        var c0 = candidates.ElementAtOrDefault(0);
        var c1 = candidates.ElementAtOrDefault(1);
        var c2 = candidates.ElementAtOrDefault(2);
        var c3 = candidates.ElementAtOrDefault(3);

        var matches = await clientsQ
            .Where(c => c.Cellphone != null && (
                (c0 != null && c.Cellphone.Contains(c0)) ||
                (c1 != null && c.Cellphone.Contains(c1)) ||
                (c2 != null && c.Cellphone.Contains(c2)) ||
                (c3 != null && c.Cellphone.Contains(c3)) ||
                c.Cellphone.Contains(query)))
            .OrderByDescending(c => c.WebsiteClientID)
            .Take(25)
            .Select(c => new
            {
                c.WebsiteClientID,
                c.WebsiteID,
                c.Givenname,
                c.Surname,
                c.Email,
                c.Cellphone,
                c.CountryCode,
                c.Active,
                OrderCount = c.Orders.Count,
                OpenSessionCount = c.SupportSessions.Count(s => OpenStatuses.Contains(s.Status) && !s.Archive),
            })
            .ToListAsync(ct);

        result.Matches = matches.Select(c => new SupportClientMatchDto
        {
            WebsiteClientID = c.WebsiteClientID,
            WebsiteID = c.WebsiteID,
            DisplayName = DisplayName(c.Givenname, c.Surname, c.Email, c.Cellphone),
            Email = c.Email,
            Cellphone = c.Cellphone,
            CountryCode = c.CountryCode,
            PhoneDisplay = PhoneDisplay(c.CountryCode, c.Cellphone),
            Active = c.Active,
            OrderCount = c.OrderCount,
            OpenSessionCount = c.OpenSessionCount,
        }).ToList();

        if (result.Matches.Count == 1)
            result.Customer360 = await GetCustomer360Async(result.Matches[0].WebsiteClientID, ct);

        return result;
    }

    public async Task<Customer360Dto?> GetCustomer360Async(int websiteClientId, CancellationToken ct = default)
    {
        var client = await _context.WebsiteClients.AsNoTracking()
            .Include(c => c.ClientWallet)
            .Include(c => c.WebsiteClientAddresses)
            .FirstOrDefaultAsync(c => c.WebsiteClientID == websiteClientId, ct);
        if (client is null) return null;

        var orders = await _context.Orders.AsNoTracking()
            .Where(o => o.WebsiteClientID == websiteClientId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(50)
            .Select(o => new
            {
                o.OrderID,
                o.OrderNumber,
                o.Status,
                o.CurrencyCode,
                o.GrandTotal,
                o.CreatedAt,
                o.PaidAt,
                ItemCount = o.OrderItems.Count,
            })
            .ToListAsync(ct);

        var sessions = await _context.SupportSessions.AsNoTracking()
            .Where(s => s.WebsiteClientID == websiteClientId)
            .OrderByDescending(s => s.LastInteractionAt ?? s.CreatedAt)
            .Take(40)
            .Include(s => s.AssignedMember)
            .Include(s => s.RelatedOrder)
            .ToListAsync(ct);

        var sessionIds = sessions.Select(s => s.SupportSessionID).ToList();

        var recentInteractions = await _context.SupportInteractions.AsNoTracking()
            .Where(i => sessionIds.Contains(i.SupportSessionID))
            .OrderByDescending(i => i.CreatedAt)
            .Take(40)
            .Include(i => i.CreatedByMember)
            .Include(i => i.RelatedOrder)
            .ToListAsync(ct);

        var agentRows = await _context.SupportInteractions.AsNoTracking()
            .Where(i => sessionIds.Contains(i.SupportSessionID) && i.CreatedByMemberID != null)
            .GroupBy(i => i.CreatedByMemberID!.Value)
            .Select(g => new
            {
                MemberID = g.Key,
                InteractionCount = g.Count(),
                LastActivityAt = g.Max(x => x.CreatedAt),
            })
            .OrderByDescending(x => x.LastActivityAt)
            .Take(20)
            .ToListAsync(ct);

        var agentMemberIds = agentRows.Select(a => a.MemberID).ToList();
        var members = await _context.Members.AsNoTracking()
            .Where(m => agentMemberIds.Contains(m.MemberID))
            .ToDictionaryAsync(m => m.MemberID, ct);

        var sessionAgentCounts = sessions
            .Where(s => s.AssignedMemberID is int)
            .GroupBy(s => s.AssignedMemberID!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        // Contact form messages matching this client's phone or email (no FK on ContactUsMessages).
        var cell = client.Cellphone;
        var email = client.Email;
        var contactQ = _context.ContactUsMessages.AsNoTracking()
            .Where(m => m.WebsiteID == client.WebsiteID);
        if (!string.IsNullOrEmpty(cell) || !string.IsNullOrEmpty(email))
        {
            contactQ = contactQ.Where(m =>
                (!string.IsNullOrEmpty(cell) && m.CellphoneNumber.Contains(cell!)) ||
                (!string.IsNullOrEmpty(email) && m.EmailAddress == email));
        }
        else
        {
            contactQ = contactQ.Where(_ => false);
        }

        var contacts = await contactQ
            .OrderByDescending(m => m.LogTime)
            .Take(20)
            .Select(m => new SupportContactMessageDto
            {
                ContactUsMessagesID = m.ContactUsMessagesID,
                MessageSubject = m.MessageSubject,
                MessageBody = m.MessageBody,
                LogTime = m.LogTime,
                Archive = m.Archive,
            })
            .ToListAsync(ct);

        var orderDtos = orders.Select(o =>
        {
            var status = (OrderStatus)o.Status;
            return new SupportOrderSummaryDto
            {
                OrderID = o.OrderID,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                StatusName = status.ToString(),
                CurrencyCode = o.CurrencyCode,
                GrandTotal = o.GrandTotal,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt,
                ItemCount = o.ItemCount,
                IsInProgress = InProgressOrderStatuses.Contains(o.Status),
            };
        }).ToList();

        return new Customer360Dto
        {
            WebsiteClientID = client.WebsiteClientID,
            WebsiteID = client.WebsiteID,
            Givenname = client.Givenname,
            Surname = client.Surname,
            DisplayName = DisplayName(client.Givenname, client.Surname, client.Email, client.Cellphone),
            Email = client.Email,
            Cellphone = client.Cellphone,
            CountryCode = client.CountryCode,
            PhoneDisplay = PhoneDisplay(client.CountryCode, client.Cellphone),
            Active = client.Active,
            ClientLevel = client.ClientLevel,
            RegisterDate = client.RegisterDate,
            OpenSessionCount = sessions.Count(s => OpenStatuses.Contains(s.Status) && !s.Archive),
            TotalSessionCount = sessions.Count,
            OrderCount = orderDtos.Count,
            InProgressOrderCount = orderDtos.Count(o => o.IsInProgress),
            WalletBalanceUsd = client.ClientWallet?.BalanceUsd,
            Orders = orderDtos,
            Sessions = sessions.Select(MapSessionSummary).ToList(),
            RecentInteractions = recentInteractions.Select(MapInteraction).ToList(),
            AgentsWhoHelped = agentRows.Select(a =>
            {
                members.TryGetValue(a.MemberID, out var m);
                return new SupportAgentActivityDto
                {
                    MemberID = a.MemberID,
                    DisplayName = m is null ? $"#{a.MemberID}" : $"{m.Givenname} {m.Surname}".Trim(),
                    InteractionCount = a.InteractionCount,
                    SessionCount = sessionAgentCounts.GetValueOrDefault(a.MemberID),
                    LastActivityAt = a.LastActivityAt,
                };
            }).ToList(),
            ContactMessages = contacts,
            Addresses = client.WebsiteClientAddresses.Select(a => new SupportAddressDto
            {
                WebsiteClientAddressID = a.WebsiteClientAddressID,
                Title = a.Title,
                RecipientName = a.ReceiverName,
                Phone = a.Phone,
                FullAddress = string.Join(", ", new[] { a.AddressLine, a.PostalCode }.Where(x => !string.IsNullOrWhiteSpace(x))),
                IsDefault = a.IsDefault,
            }).ToList(),
        };
    }

    public async Task<PagedResult<SupportSessionSummaryDto>> GetSessionsPagedAsync(
        int? websiteId,
        SupportSessionStatus? status,
        SupportPriority? priority,
        int? assignedMemberId,
        bool? archive,
        GridQuery query,
        bool? slaBreachedOnly = null,
        bool? unassignedOnly = null,
        bool? callbackDueOnly = null,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var q = _context.SupportSessions.AsNoTracking()
            .Include(s => s.AssignedMember)
            .Include(s => s.RelatedOrder)
            .AsQueryable();

        if (websiteId is int wid)
            q = q.Where(s => s.WebsiteID == wid);
        if (status is SupportSessionStatus st)
            q = q.Where(s => s.Status == (byte)st);
        if (priority is SupportPriority pr)
            q = q.Where(s => s.Priority == (byte)pr);
        if (assignedMemberId is int mid)
            q = q.Where(s => s.AssignedMemberID == mid);
        if (archive is bool arch)
            q = q.Where(s => s.Archive == arch);
        if (unassignedOnly == true)
            q = q.Where(s => s.AssignedMemberID == null && OpenStatuses.Contains(s.Status) && !s.Archive);
        if (callbackDueOnly == true)
            q = q.Where(s => s.CallbackAt != null && s.CallbackAt <= now && OpenStatuses.Contains(s.Status) && !s.Archive);
        if (slaBreachedOnly == true)
        {
            q = q.Where(s => !s.Archive && OpenStatuses.Contains(s.Status) && (
                (s.FirstResponseAt == null && s.FirstResponseDueAt != null && s.FirstResponseDueAt < now) ||
                (s.ResolvedAt == null && s.ResolveDueAt != null && s.ResolveDueAt < now)));
        }

        if (query.GetSearch("SessionNumber") is string sn)
            q = q.Where(s => s.SessionNumber.Contains(sn));
        if (query.GetSearch("Cellphone") is string cell)
            q = q.Where(s => s.CellphoneSnapshot != null && s.CellphoneSnapshot.Contains(cell));
        if (query.GetSearch("CustomerName") is string name)
            q = q.Where(s => s.CustomerNameSnapshot != null && s.CustomerNameSnapshot.Contains(name));
        if (query.GetSearch("Subject") is string subject)
            q = q.Where(s => s.Subject != null && s.Subject.Contains(subject));
        if (query.GetSearch("Tags") is string tags)
            q = q.Where(s => s.Tags != null && s.Tags.Contains(tags));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(SupportSession.CreatedAt))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        var ids = items.Select(i => i.SupportSessionID).ToList();
        var counts = await _context.SupportInteractions.AsNoTracking()
            .Where(i => ids.Contains(i.SupportSessionID))
            .GroupBy(i => i.SupportSessionID)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        var dtos = items.Select(s =>
        {
            var dto = MapSessionSummary(s);
            dto.InteractionCount = counts.GetValueOrDefault(s.SupportSessionID);
            return dto;
        }).ToList();

        return new PagedResult<SupportSessionSummaryDto> { Items = dtos, TotalCount = total };
    }

    public async Task<SupportDeskStatsDto> GetDeskStatsAsync(int? websiteId, int? memberId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var q = _context.SupportSessions.AsNoTracking().AsQueryable();
        if (websiteId is int wid)
            q = q.Where(s => s.WebsiteID == wid);

        var open = q.Where(s => !s.Archive && OpenStatuses.Contains(s.Status));

        return new SupportDeskStatsDto
        {
            OpenCount = await open.CountAsync(ct),
            MyOpenCount = memberId is int mid
                ? await open.CountAsync(s => s.AssignedMemberID == mid, ct)
                : 0,
            UnassignedCount = await open.CountAsync(s => s.AssignedMemberID == null, ct),
            WaitingCustomerCount = await open.CountAsync(s => s.Status == (byte)SupportSessionStatus.WaitingCustomer, ct),
            UrgentCount = await open.CountAsync(s => s.Priority == (byte)SupportPriority.Urgent, ct),
            SlaBreachedCount = await open.CountAsync(s =>
                (s.FirstResponseAt == null && s.FirstResponseDueAt != null && s.FirstResponseDueAt < now) ||
                (s.ResolvedAt == null && s.ResolveDueAt != null && s.ResolveDueAt < now), ct),
            CallbackDueCount = await open.CountAsync(s => s.CallbackAt != null && s.CallbackAt <= now, ct),
            ResolvedTodayCount = await q.CountAsync(s =>
                s.ResolvedAt != null && s.ResolvedAt >= today && s.ResolvedAt < today.AddDays(1), ct),
        };
    }

    public async Task<SupportSession?> GetSessionEntityByIdAsync(int sessionId, CancellationToken ct = default) =>
        await _context.SupportSessions
            .Include(s => s.AssignedMember)
            .Include(s => s.CreatedByMember)
            .Include(s => s.RelatedOrder)
            .Include(s => s.WebsiteClient)
            .Include(s => s.SupportInteractions.OrderByDescending(i => i.CreatedAt))
                .ThenInclude(i => i.CreatedByMember)
            .Include(s => s.SupportInteractions)
                .ThenInclude(i => i.RelatedOrder)
            .FirstOrDefaultAsync(s => s.SupportSessionID == sessionId, ct);

    public async Task<SupportSessionSummaryDto?> GetSessionSummaryByIdAsync(int sessionId, CancellationToken ct = default)
    {
        var s = await _context.SupportSessions.AsNoTracking()
            .Include(s => s.AssignedMember)
            .Include(s => s.RelatedOrder)
            .FirstOrDefaultAsync(x => x.SupportSessionID == sessionId, ct);
        if (s is null) return null;
        var dto = MapSessionSummary(s);
        dto.InteractionCount = await _context.SupportInteractions.CountAsync(i => i.SupportSessionID == sessionId, ct);
        return dto;
    }

    public async Task<IReadOnlyList<SupportInteractionDto>> GetInteractionsAsync(int sessionId, CancellationToken ct = default)
    {
        var items = await _context.SupportInteractions.AsNoTracking()
            .Where(i => i.SupportSessionID == sessionId)
            .OrderByDescending(i => i.CreatedAt)
            .Include(i => i.CreatedByMember)
            .Include(i => i.RelatedOrder)
            .ToListAsync(ct);
        return items.Select(MapInteraction).ToList();
    }

    public async Task<SupportSession> StartSessionAsync(StartSupportSessionRequest request, int memberId, CancellationToken ct = default)
    {
        WebsiteClient? client = null;
        if (request.WebsiteClientID is int cid)
            client = await _context.WebsiteClients.FirstOrDefaultAsync(c => c.WebsiteClientID == cid, ct);

        var now = DateTime.UtcNow;
        var cellphone = Normalize(request.Cellphone) ?? client?.Cellphone;
        var country = Normalize(request.CountryCode) ?? client?.CountryCode;
        var email = Normalize(request.Email) ?? client?.Email;
        var name = Normalize(request.CustomerName)
                   ?? DisplayName(client?.Givenname, client?.Surname, client?.Email, client?.Cellphone);

        if (client is not null && request.WebsiteID == 0)
            request.WebsiteID = client.WebsiteID;

        var session = new SupportSession
        {
            WebsiteID = request.WebsiteID,
            WebsiteClientID = client?.WebsiteClientID ?? request.WebsiteClientID,
            SessionNumber = await NextSessionNumberAsync(request.WebsiteID, now, ct),
            Status = (byte)SupportSessionStatus.Open,
            Priority = (byte)request.Priority,
            Channel = (byte)request.Channel,
            Category = (byte)request.Category,
            Subject = Truncate(request.Subject, 256),
            Tags = Truncate(request.Tags, 256),
            CellphoneSnapshot = Truncate(cellphone, 16),
            CountryCodeSnapshot = Truncate(country, 3),
            EmailSnapshot = Truncate(email, 64),
            CustomerNameSnapshot = Truncate(name, 128),
            RelatedOrderID = request.RelatedOrderID,
            CreatedByMemberID = memberId,
            AssignedMemberID = request.AssignToSelf ? memberId : null,
            CreatedAt = now,
            UpdatedAt = now,
            LastInteractionAt = now,
            Archive = false,
            CallbackAt = request.CallbackAt,
            CallbackNote = Truncate(request.CallbackNote, 500),
        };
        SupportSlaPolicy.ApplyDueDates(session);

        _context.SupportSessions.Add(session);
        await _context.SaveChangesAsync(ct);

        // Opening interaction
        var openType = request.Channel switch
        {
            SupportChannel.InboundCall => SupportInteractionType.CallInbound,
            SupportChannel.OutboundCall => SupportInteractionType.CallOutbound,
            SupportChannel.Email => SupportInteractionType.Email,
            SupportChannel.Sms => SupportInteractionType.Sms,
            _ => request.OpeningNoteInternal ? SupportInteractionType.InternalNote : SupportInteractionType.Note,
        };

        var body = string.IsNullOrWhiteSpace(request.OpeningNote)
            ? $"Session opened via {request.Channel}."
            : request.OpeningNote.Trim();

        var interaction = new SupportInteraction
        {
            SupportSessionID = session.SupportSessionID,
            InteractionType = (byte)openType,
            Body = Truncate(body, 4000),
            DurationSeconds = request.DurationSeconds,
            CallOutcome = (byte?)request.CallOutcome,
            RelatedOrderID = request.RelatedOrderID,
            CreatedByMemberID = memberId,
            IsInternal = request.OpeningNoteInternal || openType == SupportInteractionType.InternalNote,
            CreatedAt = now,
        };
        _context.SupportInteractions.Add(interaction);

        // Agent who opens the ticket while on the call counts as first response.
        session.FirstResponseAt = now;
        if (request.CallOutcome == SupportCallOutcome.CallbackScheduled && request.CallbackAt is null)
        {
            // Default callback window: 2 hours if outcome is scheduled but no time given.
            session.CallbackAt = now.AddHours(2);
        }

        await _context.SaveChangesAsync(ct);

        await NotifyTicketAsync(
            session,
            title: $"Support ticket {session.SessionNumber}",
            message: $"{session.CustomerNameSnapshot ?? session.CellphoneSnapshot ?? "Customer"} — {session.Subject ?? request.Channel.ToString()} [{(SupportPriority)session.Priority}]",
            notifyAllAdmins: !request.AssignToSelf || request.Priority is SupportPriority.Urgent or SupportPriority.High,
            notifyMemberId: request.AssignToSelf ? null : null,
            ct);

        return session;
    }

    public async Task<SupportInteraction> AddInteractionAsync(AddSupportInteractionRequest request, int memberId, CancellationToken ct = default)
    {
        var session = await _context.SupportSessions.FirstOrDefaultAsync(s => s.SupportSessionID == request.SupportSessionID, ct)
            ?? throw new InvalidOperationException("Support session not found.");

        var now = DateTime.UtcNow;
        var type = request.InteractionType;
        var isInternal = request.IsInternal || type is SupportInteractionType.InternalNote or SupportInteractionType.System;

        var interaction = new SupportInteraction
        {
            SupportSessionID = session.SupportSessionID,
            InteractionType = (byte)type,
            Body = Truncate(request.Body, 4000),
            DurationSeconds = request.DurationSeconds,
            CallOutcome = (byte?)request.CallOutcome,
            RelatedOrderID = request.RelatedOrderID ?? session.RelatedOrderID,
            CreatedByMemberID = memberId,
            IsInternal = isInternal,
            CreatedAt = now,
        };
        _context.SupportInteractions.Add(interaction);

        session.LastInteractionAt = now;
        session.UpdatedAt = now;
        if (session.FirstResponseAt is null && !isInternal)
            session.FirstResponseAt = now;

        if (request.CallbackAt is DateTime cb)
        {
            session.CallbackAt = cb;
            session.CallbackNote = Truncate(request.CallbackNote, 500);
        }
        else if (request.CallOutcome == SupportCallOutcome.CallbackScheduled && session.CallbackAt is null)
        {
            session.CallbackAt = now.AddHours(2);
            session.CallbackNote = Truncate(request.CallbackNote ?? request.Body, 500);
        }
        else if (request.CallOutcome is SupportCallOutcome.Answered && type is SupportInteractionType.CallOutbound or SupportInteractionType.CallInbound)
        {
            // Successful call clears pending callback.
            if (session.CallbackAt is not null && session.CallbackAt <= now.AddMinutes(1))
            {
                session.CallbackAt = null;
                session.CallbackNote = null;
            }
        }

        // Re-open closed/resolved tickets when agent logs new customer-facing work
        if (session.Status is (byte)SupportSessionStatus.Resolved or (byte)SupportSessionStatus.Closed
            && type is SupportInteractionType.CallInbound or SupportInteractionType.CallOutbound
                or SupportInteractionType.Note or SupportInteractionType.Email or SupportInteractionType.Sms)
        {
            session.Status = (byte)SupportSessionStatus.Open;
            session.ResolvedAt = null;
            session.ClosedAt = null;
            session.Archive = false;
            SupportSlaPolicy.RefreshResolveDueOnReopen(session, now);
        }

        await _context.SaveChangesAsync(ct);
        return interaction;
    }

    public async Task<bool> TransitionStatusAsync(int sessionId, SupportSessionStatus newStatus, int memberId, string? note, CancellationToken ct = default)
    {
        var session = await _context.SupportSessions.FirstOrDefaultAsync(s => s.SupportSessionID == sessionId, ct);
        if (session is null) return false;

        var from = (SupportSessionStatus)session.Status;
        if (from == newStatus) return true;

        var now = DateTime.UtcNow;
        session.Status = (byte)newStatus;
        session.UpdatedAt = now;
        session.LastInteractionAt = now;

        if (newStatus is SupportSessionStatus.Resolved)
        {
            session.ResolvedAt ??= now;
            session.CallbackAt = null;
        }
        else if (newStatus is SupportSessionStatus.Closed)
        {
            session.ResolvedAt ??= now;
            session.ClosedAt = now;
            session.CallbackAt = null;
        }
        else if (newStatus is SupportSessionStatus.Open or SupportSessionStatus.Pending or SupportSessionStatus.WaitingCustomer)
        {
            session.ClosedAt = null;
            if (newStatus is SupportSessionStatus.Open && from is SupportSessionStatus.Resolved or SupportSessionStatus.Closed)
            {
                session.ResolvedAt = null;
                SupportSlaPolicy.RefreshResolveDueOnReopen(session, now);
            }
            else if (newStatus is SupportSessionStatus.Open)
            {
                session.ResolvedAt = null;
            }
        }

        var body = string.IsNullOrWhiteSpace(note)
            ? $"Status: {from} → {newStatus}"
            : $"Status: {from} → {newStatus}. {note.Trim()}";

        _context.SupportInteractions.Add(new SupportInteraction
        {
            SupportSessionID = session.SupportSessionID,
            InteractionType = (byte)SupportInteractionType.StatusChange,
            Body = Truncate(body, 4000),
            FromStatus = (byte)from,
            ToStatus = (byte)newStatus,
            CreatedByMemberID = memberId,
            IsInternal = true,
            CreatedAt = now,
        });

        await _context.SaveChangesAsync(ct);

        if (session.AssignedMemberID is int assignee && assignee != memberId
            && newStatus is SupportSessionStatus.Open or SupportSessionStatus.Pending)
        {
            await _notifications.NotifyMemberAsync(
                assignee,
                session.WebsiteID,
                AdminNotificationType.SupportTicket,
                $"Ticket {session.SessionNumber} → {newStatus}",
                body,
                $"/support/tickets/{session.SupportSessionID}",
                session.SupportSessionID,
                ct);
        }

        return true;
    }

    public async Task<bool> AssignAsync(int sessionId, int? assignedMemberId, int actorMemberId, CancellationToken ct = default)
    {
        var session = await _context.SupportSessions.FirstOrDefaultAsync(s => s.SupportSessionID == sessionId, ct);
        if (session is null) return false;

        var now = DateTime.UtcNow;
        var previous = session.AssignedMemberID;
        session.AssignedMemberID = assignedMemberId;
        session.UpdatedAt = now;
        session.LastInteractionAt = now;

        string? toName = null;
        if (assignedMemberId is int mid)
        {
            var m = await _context.Members.AsNoTracking().FirstOrDefaultAsync(x => x.MemberID == mid, ct);
            toName = m is null ? $"#{mid}" : $"{m.Givenname} {m.Surname}".Trim();
        }

        _context.SupportInteractions.Add(new SupportInteraction
        {
            SupportSessionID = session.SupportSessionID,
            InteractionType = (byte)SupportInteractionType.Assignment,
            Body = assignedMemberId is null
                ? $"Unassigned (was member #{previous})."
                : $"Assigned to {toName} (was {(previous is int p ? $"#{p}" : "unassigned")}).",
            CreatedByMemberID = actorMemberId,
            IsInternal = true,
            CreatedAt = now,
        });

        await _context.SaveChangesAsync(ct);

        if (assignedMemberId is int toId && toId != actorMemberId)
        {
            await _notifications.NotifyMemberAsync(
                toId,
                session.WebsiteID,
                AdminNotificationType.SupportTicket,
                $"Ticket assigned: {session.SessionNumber}",
                session.Subject ?? session.CustomerNameSnapshot ?? session.CellphoneSnapshot ?? "Support ticket",
                $"/support/tickets/{session.SupportSessionID}",
                session.SupportSessionID,
                ct);
        }

        return true;
    }

    public async Task<bool> SetPriorityAsync(int sessionId, SupportPriority priority, int memberId, CancellationToken ct = default)
    {
        var session = await _context.SupportSessions.FirstOrDefaultAsync(s => s.SupportSessionID == sessionId, ct);
        if (session is null) return false;

        var from = (SupportPriority)session.Priority;
        if (from == priority) return true;

        var now = DateTime.UtcNow;
        session.Priority = (byte)priority;
        session.UpdatedAt = now;
        SupportSlaPolicy.ApplyDueDates(session);

        _context.SupportInteractions.Add(new SupportInteraction
        {
            SupportSessionID = session.SupportSessionID,
            InteractionType = (byte)SupportInteractionType.System,
            Body = $"Priority: {from} → {priority} (SLA deadlines refreshed)",
            CreatedByMemberID = memberId,
            IsInternal = true,
            CreatedAt = now,
        });

        await _context.SaveChangesAsync(ct);

        if (priority is SupportPriority.Urgent or SupportPriority.High)
        {
            await NotifyTicketAsync(
                session,
                $"Priority {priority}: {session.SessionNumber}",
                session.Subject ?? "Support ticket escalated",
                notifyAllAdmins: true,
                notifyMemberId: session.AssignedMemberID,
                ct);
        }

        return true;
    }

    public async Task<bool> LinkOrderAsync(int sessionId, int? orderId, int memberId, CancellationToken ct = default)
    {
        var session = await _context.SupportSessions.FirstOrDefaultAsync(s => s.SupportSessionID == sessionId, ct);
        if (session is null) return false;

        string? orderNumber = null;
        if (orderId is int oid)
        {
            var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderID == oid, ct);
            if (order is null) return false;
            if (session.WebsiteClientID is int cid && order.WebsiteClientID != cid)
                return false;
            orderNumber = order.OrderNumber;
        }

        var now = DateTime.UtcNow;
        session.RelatedOrderID = orderId;
        session.UpdatedAt = now;
        session.LastInteractionAt = now;
        if (orderId is not null && session.Category == (byte)SupportCategory.General)
            session.Category = (byte)SupportCategory.Order;

        _context.SupportInteractions.Add(new SupportInteraction
        {
            SupportSessionID = session.SupportSessionID,
            InteractionType = (byte)SupportInteractionType.OrderLinked,
            Body = orderId is null ? "Unlinked order." : $"Linked order {orderNumber} (#{orderId}).",
            RelatedOrderID = orderId,
            CreatedByMemberID = memberId,
            IsInternal = true,
            CreatedAt = now,
        });

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task SetArchiveAsync(int sessionId, bool archive, CancellationToken ct = default)
    {
        var session = await _context.SupportSessions.FirstOrDefaultAsync(s => s.SupportSessionID == sessionId, ct);
        if (session is null) return;
        session.Archive = archive;
        session.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task SetSatisfactionAsync(int sessionId, byte? rating, CancellationToken ct = default)
    {
        var session = await _context.SupportSessions.FirstOrDefaultAsync(s => s.SupportSessionID == sessionId, ct);
        if (session is null) return;
        if (rating is byte r && (r < 1 || r > 5)) return;
        session.SatisfactionRating = rating;
        session.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateSessionMetaAsync(
        int sessionId,
        string? subject,
        SupportCategory? category,
        string? tags,
        CancellationToken ct = default)
    {
        var session = await _context.SupportSessions.FirstOrDefaultAsync(s => s.SupportSessionID == sessionId, ct);
        if (session is null) return;
        if (subject is not null) session.Subject = Truncate(subject, 256);
        if (category is SupportCategory cat) session.Category = (byte)cat;
        if (tags is not null) session.Tags = Truncate(tags, 256);
        session.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ScheduleCallbackAsync(int sessionId, DateTime? callbackAt, string? note, int memberId, CancellationToken ct = default)
    {
        var session = await _context.SupportSessions.FirstOrDefaultAsync(s => s.SupportSessionID == sessionId, ct);
        if (session is null) return false;

        var now = DateTime.UtcNow;
        session.CallbackAt = callbackAt;
        session.CallbackNote = Truncate(note, 500);
        session.UpdatedAt = now;
        session.LastInteractionAt = now;

        _context.SupportInteractions.Add(new SupportInteraction
        {
            SupportSessionID = session.SupportSessionID,
            InteractionType = (byte)SupportInteractionType.System,
            Body = callbackAt is DateTime at
                ? $"Callback scheduled for {at:u}. {note}".Trim()
                : $"Callback cleared. {note}".Trim(),
            CallOutcome = callbackAt is null ? null : (byte)SupportCallOutcome.CallbackScheduled,
            CreatedByMemberID = memberId,
            IsInternal = true,
            CreatedAt = now,
        });

        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private async Task NotifyTicketAsync(
        SupportSession session,
        string title,
        string message,
        bool notifyAllAdmins,
        int? notifyMemberId,
        CancellationToken ct)
    {
        var url = $"/support/tickets/{session.SupportSessionID}";
        try
        {
            if (notifyAllAdmins)
            {
                await _notifications.NotifySiteAdminsAsync(
                    session.WebsiteID,
                    AdminNotificationType.SupportTicket,
                    title,
                    message,
                    url,
                    session.SupportSessionID,
                    ct);
            }

            if (notifyMemberId is int mid)
            {
                await _notifications.NotifyMemberAsync(
                    mid,
                    session.WebsiteID,
                    AdminNotificationType.SupportTicket,
                    title,
                    message,
                    url,
                    session.SupportSessionID,
                    ct);
            }
        }
        catch
        {
            // Notifications must never break the desk workflow.
        }
    }

    private async Task<string> NextSessionNumberAsync(int websiteId, DateTime now, CancellationToken ct)
    {
        var day = now.ToString("yyyyMMdd");
        var prefix = $"SUP-{day}-";
        var last = await _context.SupportSessions.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId && s.SessionNumber.StartsWith(prefix))
            .OrderByDescending(s => s.SessionNumber)
            .Select(s => s.SessionNumber)
            .FirstOrDefaultAsync(ct);

        var seq = 1;
        if (last is not null && last.Length > prefix.Length
            && int.TryParse(last.AsSpan(prefix.Length), out var n))
            seq = n + 1;

        return $"{prefix}{seq:D5}";
    }

    private static SupportSessionSummaryDto MapSessionSummary(SupportSession s)
    {
        // Backfill SLA dues for rows created before due columns existed.
        var firstDue = s.FirstResponseDueAt;
        var resolveDue = s.ResolveDueAt;
        if (firstDue is null || resolveDue is null)
        {
            var (fr, res) = SupportSlaPolicy.GetTargets((SupportPriority)s.Priority);
            firstDue ??= s.CreatedAt.Add(fr);
            resolveDue ??= s.CreatedAt.Add(res);
        }

        var sla = SupportSlaPolicy.Evaluate(
            s.Status, s.CreatedAt, s.FirstResponseAt, firstDue, s.ResolvedAt, resolveDue);

        return new SupportSessionSummaryDto
        {
            SupportSessionID = s.SupportSessionID,
            SessionNumber = s.SessionNumber,
            Status = s.Status,
            StatusName = ((SupportSessionStatus)s.Status).ToString(),
            Priority = s.Priority,
            PriorityName = ((SupportPriority)s.Priority).ToString(),
            Channel = s.Channel,
            ChannelName = ((SupportChannel)s.Channel).ToString(),
            Category = s.Category,
            CategoryName = ((SupportCategory)s.Category).ToString(),
            Subject = s.Subject,
            Tags = s.Tags,
            CellphoneSnapshot = s.CellphoneSnapshot,
            CustomerNameSnapshot = s.CustomerNameSnapshot,
            WebsiteClientID = s.WebsiteClientID,
            RelatedOrderID = s.RelatedOrderID,
            RelatedOrderNumber = s.RelatedOrder?.OrderNumber,
            AssignedMemberID = s.AssignedMemberID,
            AssignedMemberName = s.AssignedMember is null
                ? null
                : $"{s.AssignedMember.Givenname} {s.AssignedMember.Surname}".Trim(),
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            LastInteractionAt = s.LastInteractionAt,
            FirstResponseAt = s.FirstResponseAt,
            ResolvedAt = s.ResolvedAt,
            FirstResponseDueAt = firstDue,
            ResolveDueAt = resolveDue,
            CallbackAt = s.CallbackAt,
            CallbackNote = s.CallbackNote,
            SlaState = sla,
            SlaLabel = SlaLabel(sla),
            IsSlaBreached = SupportSlaPolicy.IsBreached(sla),
            Archive = s.Archive,
            SatisfactionRating = s.SatisfactionRating,
        };
    }

    private static string SlaLabel(SupportSlaState state) => state switch
    {
        SupportSlaState.Ok => "On track",
        SupportSlaState.FirstResponseAtRisk => "First response at risk",
        SupportSlaState.FirstResponseBreached => "First response breached",
        SupportSlaState.ResolveAtRisk => "Resolve at risk",
        SupportSlaState.ResolveBreached => "Resolve breached",
        SupportSlaState.Met => "SLA met",
        SupportSlaState.Closed => "Closed",
        _ => state.ToString(),
    };

    private static SupportInteractionDto MapInteraction(SupportInteraction i) => new()
    {
        SupportInteractionID = i.SupportInteractionID,
        SupportSessionID = i.SupportSessionID,
        InteractionType = i.InteractionType,
        InteractionTypeName = ((SupportInteractionType)i.InteractionType).ToString(),
        Body = i.Body,
        DurationSeconds = i.DurationSeconds,
        CallOutcome = i.CallOutcome,
        CallOutcomeName = i.CallOutcome is byte co ? ((SupportCallOutcome)co).ToString() : null,
        FromStatus = i.FromStatus,
        ToStatus = i.ToStatus,
        RelatedOrderID = i.RelatedOrderID,
        RelatedOrderNumber = i.RelatedOrder?.OrderNumber,
        CreatedByMemberID = i.CreatedByMemberID,
        CreatedByMemberName = i.CreatedByMember is null
            ? null
            : $"{i.CreatedByMember.Givenname} {i.CreatedByMember.Surname}".Trim(),
        IsInternal = i.IsInternal,
        CreatedAt = i.CreatedAt,
    };

    private static string DigitsOnly(string value) =>
        Regex.Replace(value ?? string.Empty, @"\D", string.Empty);

    /// <summary>
    /// Expand pasted phone strings into likely national substrings for Contains search
    /// (E.164, with/without leading 0, without country code).
    /// </summary>
    private static List<string> BuildPhoneSearchCandidates(string digits)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        if (string.IsNullOrEmpty(digits)) return [];

        void Add(string? s)
        {
            if (!string.IsNullOrEmpty(s) && s.Length >= 4)
                set.Add(s);
        }

        Add(digits);

        // Drop leading zeros for alternate form
        var noLeadingZero = digits.TrimStart('0');
        Add(noLeadingZero);
        if (noLeadingZero.Length >= 9)
            Add("0" + noLeadingZero);

        // Iran (+98) and common country codes: if starts with 98 and long enough, also try 0 + rest
        if (digits.StartsWith("98", StringComparison.Ordinal) && digits.Length >= 12)
        {
            var national = digits[2..];
            Add(national);
            Add("0" + national.TrimStart('0'));
        }

        // If still long international, keep last 10 digits as a candidate
        if (digits.Length > 10)
            Add(digits[^10..]);

        return set.OrderByDescending(s => s.Length).ToList();
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Truncate(string? value, int max) =>
        value is null ? null : value.Length <= max ? value : value[..max];

    private static string DisplayName(string? given, string? surname, string? email, string? cell)
    {
        var name = $"{given} {surname}".Trim();
        if (!string.IsNullOrEmpty(name)) return name;
        if (!string.IsNullOrEmpty(email)) return email!;
        if (!string.IsNullOrEmpty(cell)) return cell!;
        return "Unknown";
    }

    private static string PhoneDisplay(string? countryCode, string? cellphone)
    {
        if (string.IsNullOrEmpty(cellphone)) return string.Empty;
        return string.IsNullOrEmpty(countryCode) ? cellphone : $"+{countryCode} {cellphone}";
    }
}
