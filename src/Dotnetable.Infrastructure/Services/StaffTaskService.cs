using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class StaffTaskService : IStaffTaskService
{
    private static readonly byte[] OpenStatuses =
    [
        (byte)StaffTaskStatus.Open,
        (byte)StaffTaskStatus.InProgress,
    ];

    private readonly IDbContextFactory<AppDbContext> _db;
    private readonly IAdminNotificationService _notifications;

    public StaffTaskService(IDbContextFactory<AppDbContext> db, IAdminNotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public Task<IReadOnlyList<StaffTaskColleagueDto>> ListColleaguesAsync(
        int websiteId, int actorMemberId, CancellationToken ct = default)
        => _db.UseAsync(async (context, token) =>
        {
            var rows = await context.Members.AsNoTracking()
                .Where(m => m.Active && (m.WebsiteID == websiteId || m.MemberID == actorMemberId))
                .OrderBy(m => m.Givenname).ThenBy(m => m.Surname)
                .Select(m => new StaffTaskColleagueDto
                {
                    MemberID = m.MemberID,
                    Name = ((m.Givenname + " " + m.Surname).Trim()),
                    Username = m.Username,
                })
                .ToListAsync(token);

            if (rows.All(r => r.MemberID != actorMemberId) && actorMemberId > 0)
            {
                var actor = await context.Members.AsNoTracking()
                    .Where(m => m.MemberID == actorMemberId)
                    .Select(m => new StaffTaskColleagueDto
                    {
                        MemberID = m.MemberID,
                        Name = ((m.Givenname + " " + m.Surname).Trim()),
                        Username = m.Username,
                    })
                    .FirstOrDefaultAsync(token);
                if (actor is not null) rows.Insert(0, actor);
            }

            return (IReadOnlyList<StaffTaskColleagueDto>)rows;
        }, ct);

    public Task<IReadOnlyList<StaffTaskRelatedHitDto>> SearchRelatedAsync(
        int websiteId, StaffTaskRelatedKind kind, string? query, CancellationToken ct = default)
        => _db.UseAsync(async (context, token) =>
        {
            var q = (query ?? "").Trim();
            var take = 20;

            if (kind == StaffTaskRelatedKind.None)
                return (IReadOnlyList<StaffTaskRelatedHitDto>)Array.Empty<StaffTaskRelatedHitDto>();

            if (kind == StaffTaskRelatedKind.Order)
            {
                var orders = context.Orders.AsNoTracking().Where(o => o.WebsiteID == websiteId);
                if (q.Length > 0)
                    orders = orders.Where(o => o.OrderNumber.Contains(q) || o.OrderID.ToString() == q);
                return await orders.OrderByDescending(o => o.OrderID).Take(take)
                    .Select(o => new StaffTaskRelatedHitDto { Id = o.OrderID, Label = o.OrderNumber })
                    .ToListAsync(token);
            }

            var stockType = StaffTaskRelated.StockDocumentType(kind);
            if (stockType is byte st)
            {
                var docs = context.StockDocuments.AsNoTracking()
                    .Where(d => d.WebsiteID == websiteId && d.DocumentType == st);
                if (q.Length > 0)
                    docs = docs.Where(d => d.DocumentNumber.Contains(q) || d.StockDocumentID.ToString() == q);
                return await docs.OrderByDescending(d => d.StockDocumentID).Take(take)
                    .Select(d => new StaffTaskRelatedHitDto { Id = d.StockDocumentID, Label = d.DocumentNumber })
                    .ToListAsync(token);
            }

            if (kind == StaffTaskRelatedKind.CustomerReturn)
            {
                var rets = context.CustomerReturnRequests.AsNoTracking()
                    .Where(r => r.WebsiteID == websiteId);
                if (q.Length > 0)
                {
                    rets = rets.Where(r =>
                        r.CustomerReturnRequestID.ToString() == q
                        || r.Order.OrderNumber.Contains(q));
                }
                return await rets.OrderByDescending(r => r.CustomerReturnRequestID).Take(take)
                    .Select(r => new StaffTaskRelatedHitDto
                    {
                        Id = r.CustomerReturnRequestID,
                        Label = "#" + r.CustomerReturnRequestID + " · " + r.Order.OrderNumber,
                    })
                    .ToListAsync(token);
            }

            if (kind == StaffTaskRelatedKind.Payment)
            {
                var pays = context.Payments.AsNoTracking().Where(p => p.WebsiteID == websiteId);
                if (q.Length > 0)
                {
                    pays = pays.Where(p =>
                        p.PaymentID.ToString() == q
                        || (p.TrackingCode != null && p.TrackingCode.Contains(q))
                        || (p.GatewayRefNumber != null && p.GatewayRefNumber.Contains(q)));
                }
                return await pays.OrderByDescending(p => p.PaymentID).Take(take)
                    .Select(p => new StaffTaskRelatedHitDto
                    {
                        Id = p.PaymentID,
                        Label = "#" + p.PaymentID + (p.TrackingCode != null ? " · " + p.TrackingCode : ""),
                    })
                    .ToListAsync(token);
            }

            if (kind == StaffTaskRelatedKind.PaymentRefund)
            {
                var refunds = context.PaymentRefunds.AsNoTracking()
                    .Where(r => r.Payment.WebsiteID == websiteId);
                if (q.Length > 0)
                    refunds = refunds.Where(r => r.PaymentRefundID.ToString() == q || r.PaymentID.ToString() == q);
                return await refunds.OrderByDescending(r => r.PaymentRefundID).Take(take)
                    .Select(r => new StaffTaskRelatedHitDto
                    {
                        Id = r.PaymentRefundID,
                        Label = "Refund #" + r.PaymentRefundID,
                    })
                    .ToListAsync(token);
            }

            return (IReadOnlyList<StaffTaskRelatedHitDto>)Array.Empty<StaffTaskRelatedHitDto>();
        }, ct);

    public Task<IReadOnlyList<StaffTaskDto>> ListAsync(StaffTaskListFilter filter, CancellationToken ct = default)
        => _db.UseAsync(async (context, token) =>
        {
            var q = ApplyFilter(context.StaffTasks.AsNoTracking(), filter);
            var rows = await q
                .Include(t => t.AssignedMember)
                .Include(t => t.CreatedByMember)
                .OrderBy(t => t.Status)
                .ThenByDescending(t => t.Priority)
                .ThenBy(t => t.DueAt ?? DateTime.MaxValue)
                .ThenByDescending(t => t.CreatedAt)
                .Take(200)
                .ToListAsync(token);
            return (IReadOnlyList<StaffTaskDto>)rows.Select(Map).ToList();
        }, ct);

    public Task<IReadOnlyList<(byte Status, int Count)>> CountByStatusAsync(
        StaffTaskListFilter filter, CancellationToken ct = default)
        => _db.UseAsync(async (context, token) =>
        {
            var scoped = filter with { Status = null };
            var rows = await ApplyFilter(context.StaffTasks.AsNoTracking(), scoped)
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(token);
            return (IReadOnlyList<(byte, int)>)rows.Select(r => (r.Status, r.Count)).ToList();
        }, ct);

    public Task<int> CountOpenAsync(int websiteId, int actorMemberId, bool canManage, CancellationToken ct = default)
        => _db.UseAsync((context, token) =>
            ApplyFilter(context.StaffTasks.AsNoTracking(), new StaffTaskListFilter
            {
                WebsiteID = websiteId,
                ActorMemberID = actorMemberId,
                CanManage = canManage,
            }).CountAsync(t => OpenStatuses.Contains(t.Status), token), ct);

    public Task<StaffTaskDto?> GetByIdAsync(int staffTaskId, CancellationToken ct = default)
        => _db.UseAsync(async (context, token) =>
        {
            var row = await context.StaffTasks.AsNoTracking()
                .Include(t => t.AssignedMember)
                .Include(t => t.CreatedByMember)
                .FirstOrDefaultAsync(t => t.StaffTaskID == staffTaskId, token);
            return row is null ? null : Map(row);
        }, ct);

    public Task<(bool Success, string? Error, StaffTaskDto? Task)> CreateAsync(
        StaffTaskWriteRequest request, int actorMemberId, bool canManage, CancellationToken ct = default)
        => _db.UseAsync(async (context, token) =>
        {
            var (ok, err, relatedLabel) = await ValidateWriteAsync(context, request, actorMemberId, canManage, token);
            if (!ok) return (false, err, (StaffTaskDto?)null);

            var now = DateTime.UtcNow;
            var row = new StaffTask
            {
                WebsiteID = request.WebsiteID,
                Title = request.Title.Trim(),
                Description = Truncate(request.Description, 2000),
                Status = (byte)StaffTaskStatus.Open,
                Priority = request.Priority,
                AssignedMemberID = request.AssignedMemberID,
                CreatedByMemberID = actorMemberId,
                DueAt = request.DueAt,
                RelatedKind = request.RelatedKind,
                RelatedEntityID = request.RelatedKind == (byte)StaffTaskRelatedKind.None ? null : request.RelatedEntityID,
                RelatedLabel = relatedLabel,
                CreatedAt = now,
                UpdatedAt = now,
            };
            context.StaffTasks.Add(row);
            await context.SaveChangesAsync(token);

            if (row.AssignedMemberID != actorMemberId)
                await NotifyAssigneeAsync(row, token);

            var created = await context.StaffTasks.AsNoTracking()
                .Include(t => t.AssignedMember)
                .Include(t => t.CreatedByMember)
                .FirstAsync(t => t.StaffTaskID == row.StaffTaskID, token);
            return (true, (string?)null, Map(created));
        }, ct);

    public Task<(bool Success, string? Error, StaffTaskDto? Task)> UpdateAsync(
        int staffTaskId, StaffTaskWriteRequest request, int actorMemberId, bool canManage, CancellationToken ct = default)
        => _db.UseAsync(async (context, token) =>
        {
            var row = await context.StaffTasks.FirstOrDefaultAsync(t => t.StaffTaskID == staffTaskId, token);
            if (row is null) return (false, "Task not found.", (StaffTaskDto?)null);
            if (!CanEdit(row, actorMemberId, canManage))
                return (false, "You cannot edit this task.", (StaffTaskDto?)null);

            request.WebsiteID = row.WebsiteID;
            var (ok, err, relatedLabel) = await ValidateWriteAsync(context, request, actorMemberId, canManage, token);
            if (!ok) return (false, err, (StaffTaskDto?)null);

            var previousAssignee = row.AssignedMemberID;
            row.Title = request.Title.Trim();
            row.Description = Truncate(request.Description, 2000);
            row.Priority = request.Priority;
            row.AssignedMemberID = request.AssignedMemberID;
            row.DueAt = request.DueAt;
            row.RelatedKind = request.RelatedKind;
            row.RelatedEntityID = request.RelatedKind == (byte)StaffTaskRelatedKind.None ? null : request.RelatedEntityID;
            row.RelatedLabel = relatedLabel;
            row.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(token);

            if (row.AssignedMemberID != previousAssignee && row.AssignedMemberID != actorMemberId)
                await NotifyAssigneeAsync(row, token);

            var updated = await context.StaffTasks.AsNoTracking()
                .Include(t => t.AssignedMember)
                .Include(t => t.CreatedByMember)
                .FirstAsync(t => t.StaffTaskID == row.StaffTaskID, token);
            return (true, (string?)null, Map(updated));
        }, ct);

    public Task<(bool Success, string? Error)> SetStatusAsync(
        int staffTaskId, StaffTaskStatus status, int actorMemberId, bool canManage, CancellationToken ct = default)
        => _db.UseAsync(async (context, token) =>
        {
            var row = await context.StaffTasks.FirstOrDefaultAsync(t => t.StaffTaskID == staffTaskId, token);
            if (row is null) return (false, "Task not found.");
            if (!CanEdit(row, actorMemberId, canManage) && row.AssignedMemberID != actorMemberId)
                return (false, "You cannot change this task.");

            row.Status = (byte)status;
            row.CompletedAt = status is StaffTaskStatus.Done or StaffTaskStatus.Cancelled
                ? DateTime.UtcNow
                : null;
            row.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(token);
            return (true, (string?)null);
        }, ct);

    public Task<(bool Success, string? Error)> DeleteAsync(
        int staffTaskId, int actorMemberId, bool canManage, CancellationToken ct = default)
        => _db.UseAsync(async (context, token) =>
        {
            var row = await context.StaffTasks.FirstOrDefaultAsync(t => t.StaffTaskID == staffTaskId, token);
            if (row is null) return (false, "Task not found.");
            if (!canManage && row.CreatedByMemberID != actorMemberId)
                return (false, "You cannot delete this task.");
            context.StaffTasks.Remove(row);
            await context.SaveChangesAsync(token);
            return (true, (string?)null);
        }, ct);

    private static IQueryable<StaffTask> ApplyFilter(IQueryable<StaffTask> q, StaffTaskListFilter filter)
    {
        q = q.Where(t => t.WebsiteID == filter.WebsiteID);
        if (!filter.CanManage)
            q = q.Where(t => t.AssignedMemberID == filter.ActorMemberID || t.CreatedByMemberID == filter.ActorMemberID);
        else if (filter.AssignedMemberID is int aid && aid > 0)
            q = q.Where(t => t.AssignedMemberID == aid);
        if (filter.Status is byte st)
            q = q.Where(t => t.Status == st);
        var search = filter.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            q = q.Where(t => t.Title.Contains(search)
                || (t.Description != null && t.Description.Contains(search))
                || (t.RelatedLabel != null && t.RelatedLabel.Contains(search)));
        }
        return q;
    }

    private static bool CanEdit(StaffTask row, int actorMemberId, bool canManage) =>
        canManage || row.AssignedMemberID == actorMemberId || row.CreatedByMemberID == actorMemberId;

    private async Task<(bool Ok, string? Error, string? RelatedLabel)> ValidateWriteAsync(
        AppDbContext context, StaffTaskWriteRequest request, int actorMemberId, bool canManage, CancellationToken ct)
    {
        if (request.WebsiteID <= 0) return (false, "Website is required.", null);
        if (string.IsNullOrWhiteSpace(request.Title)) return (false, "Title is required.", null);
        if (request.Title.Trim().Length > 200) return (false, "Title is too long.", null);
        if (actorMemberId <= 0) return (false, "You must be signed in.", null);

        if (!canManage && request.AssignedMemberID != actorMemberId)
            return (false, "You can only create tasks for yourself.", null);

        if (request.AssignedMemberID <= 0)
            request.AssignedMemberID = actorMemberId;

        var assignee = await context.Members.AsNoTracking()
            .FirstOrDefaultAsync(m => m.MemberID == request.AssignedMemberID, ct);
        if (assignee is null || !assignee.Active)
            return (false, "Assignee not found.", null);
        if (assignee.WebsiteID != request.WebsiteID && assignee.MemberID != actorMemberId)
            return (false, "Assignee must be a colleague on this website.", null);

        var kind = (StaffTaskRelatedKind)request.RelatedKind;
        if (kind == StaffTaskRelatedKind.None)
            return (true, null, null);
        if (request.RelatedEntityID is not int rid || rid <= 0)
            return (false, "Pick a related record.", null);

        var (exists, label) = await ResolveRelatedAsync(context, request.WebsiteID, kind, rid, ct);
        if (!exists) return (false, "Related record was not found on this website.", null);
        return (true, null, label);
    }

    private static async Task<(bool Exists, string? Label)> ResolveRelatedAsync(
        AppDbContext context, int websiteId, StaffTaskRelatedKind kind, int id, CancellationToken ct)
    {
        if (kind == StaffTaskRelatedKind.Order)
        {
            var n = await context.Orders.AsNoTracking()
                .Where(o => o.OrderID == id && o.WebsiteID == websiteId)
                .Select(o => o.OrderNumber)
                .FirstOrDefaultAsync(ct);
            return n is null ? (false, null) : (true, n);
        }

        var stockType = StaffTaskRelated.StockDocumentType(kind);
        if (stockType is byte st)
        {
            var n = await context.StockDocuments.AsNoTracking()
                .Where(d => d.StockDocumentID == id && d.WebsiteID == websiteId && d.DocumentType == st)
                .Select(d => d.DocumentNumber)
                .FirstOrDefaultAsync(ct);
            return n is null ? (false, null) : (true, n);
        }

        if (kind == StaffTaskRelatedKind.CustomerReturn)
        {
            var n = await context.CustomerReturnRequests.AsNoTracking()
                .Where(r => r.CustomerReturnRequestID == id && r.WebsiteID == websiteId)
                .Select(r => "#" + r.CustomerReturnRequestID + " · " + r.Order.OrderNumber)
                .FirstOrDefaultAsync(ct);
            return n is null ? (false, null) : (true, n);
        }

        if (kind == StaffTaskRelatedKind.Payment)
        {
            var n = await context.Payments.AsNoTracking()
                .Where(p => p.PaymentID == id && p.WebsiteID == websiteId)
                .Select(p => "#" + p.PaymentID)
                .FirstOrDefaultAsync(ct);
            return n is null ? (false, null) : (true, n);
        }

        if (kind == StaffTaskRelatedKind.PaymentRefund)
        {
            var n = await context.PaymentRefunds.AsNoTracking()
                .Where(r => r.PaymentRefundID == id && r.Payment.WebsiteID == websiteId)
                .Select(r => "Refund #" + r.PaymentRefundID)
                .FirstOrDefaultAsync(ct);
            return n is null ? (false, null) : (true, n);
        }

        return (false, null);
    }

    private async Task NotifyAssigneeAsync(StaffTask row, CancellationToken ct)
    {
        try
        {
            await _notifications.NotifyMemberAsync(
                row.AssignedMemberID,
                row.WebsiteID,
                AdminNotificationType.StaffTask,
                "New task assigned to you",
                row.Title,
                "/tasks",
                row.StaffTaskID,
                ct);
        }
        catch
        {
            // Task is saved; notification is best-effort.
        }
    }

    private static StaffTaskDto Map(StaffTask t)
    {
        var kind = (StaffTaskRelatedKind)t.RelatedKind;
        return new StaffTaskDto
        {
            StaffTaskID = t.StaffTaskID,
            WebsiteID = t.WebsiteID,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            AssignedMemberID = t.AssignedMemberID,
            AssignedMemberName = NameOf(t.AssignedMember),
            CreatedByMemberID = t.CreatedByMemberID,
            CreatedByMemberName = NameOf(t.CreatedByMember),
            DueAt = t.DueAt,
            CompletedAt = t.CompletedAt,
            RelatedKind = t.RelatedKind,
            RelatedEntityID = t.RelatedEntityID,
            RelatedLabel = t.RelatedLabel,
            RelatedUrl = StaffTaskRelated.Url(kind, t.RelatedEntityID),
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
        };
    }

    private static string NameOf(Member? m)
    {
        if (m is null) return "";
        var n = $"{m.Givenname} {m.Surname}".Trim();
        return string.IsNullOrEmpty(n) ? m.Username : n;
    }

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var t = value.Trim();
        return t.Length <= max ? t : t[..max];
    }
}
