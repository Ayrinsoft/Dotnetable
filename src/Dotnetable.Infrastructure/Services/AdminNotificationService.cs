using Dotnetable.Application.DTOs;
using Dotnetable.Application.Email;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Uses a short-lived DbContext from the factory (not the circuit-scoped one) so concurrent
/// Blazor component init after login (layout + dashboard + nav) cannot race on one context.
/// </summary>
public class AdminNotificationService : IAdminNotificationService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IEmailService _email;
    private readonly ILogger<AdminNotificationService> _logger;

    public AdminNotificationService(
        IDbContextFactory<AppDbContext> contextFactory,
        IEmailService email,
        ILogger<AdminNotificationService> logger)
    {
        _contextFactory = contextFactory;
        _email = email;
        _logger = logger;
    }

    public async Task NotifySiteAdminsAsync(
        int websiteId,
        AdminNotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        int? relatedEntityId = null,
        CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var admins = await context.Members.AsNoTracking()
            .Where(m => m.WebsiteID == websiteId && m.Active && m.IsSiteAdmin && m.Email != "")
            .Select(m => new { m.MemberID, m.Email, m.Givenname })
            .ToListAsync(ct);

        if (admins.Count == 0) return;

        var now = DateTime.UtcNow;
        var safeTitle = Truncate(title, 200);
        var safeMessage = Truncate(message, 1000);
        var safeUrl = actionUrl is null ? null : Truncate(actionUrl, 256);

        foreach (var admin in admins)
        {
            context.AdminNotifications.Add(new AdminNotification
            {
                MemberID = admin.MemberID,
                WebsiteID = websiteId,
                NotificationType = (byte)type,
                Title = safeTitle,
                Message = safeMessage,
                ActionUrl = safeUrl,
                RelatedEntityID = relatedEntityId,
                IsRead = false,
                CreatedAt = now,
            });
        }

        await context.SaveChangesAsync(ct);

        if (!await _email.IsConfiguredAsync(websiteId, ct))
            return;

        foreach (var admin in admins)
        {
            try
            {
                await _email.SendTemplateAsync(
                    websiteId,
                    EmailTemplateKeys.AdminSiteNotification,
                    admin.Email,
                    new Dictionary<string, string>
                    {
                        ["AdminName"] = string.IsNullOrWhiteSpace(admin.Givenname) ? admin.Email : admin.Givenname,
                        ["Title"] = safeTitle,
                        ["MessageBody"] = safeMessage,
                        ["ActionUrl"] = safeUrl ?? string.Empty,
                    },
                    languageCode: null,
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to email site admin notification to {Email} for website {WebsiteId}", admin.Email, websiteId);
            }
        }
    }

    public async Task NotifyMemberAsync(
        int memberId,
        int websiteId,
        AdminNotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        int? relatedEntityId = null,
        CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var member = await context.Members.AsNoTracking()
            .Where(m => m.MemberID == memberId && m.Active)
            .Select(m => new { m.MemberID, m.Email, m.Givenname })
            .FirstOrDefaultAsync(ct);
        if (member is null) return;

        var now = DateTime.UtcNow;
        var safeTitle = Truncate(title, 200);
        var safeMessage = Truncate(message, 1000);
        var safeUrl = actionUrl is null ? null : Truncate(actionUrl, 256);

        context.AdminNotifications.Add(new AdminNotification
        {
            MemberID = member.MemberID,
            WebsiteID = websiteId,
            NotificationType = (byte)type,
            Title = safeTitle,
            Message = safeMessage,
            ActionUrl = safeUrl,
            RelatedEntityID = relatedEntityId,
            IsRead = false,
            CreatedAt = now,
        });
        await context.SaveChangesAsync(ct);

        if (string.IsNullOrWhiteSpace(member.Email) || !await _email.IsConfiguredAsync(websiteId, ct))
            return;

        try
        {
            await _email.SendTemplateAsync(
                websiteId,
                EmailTemplateKeys.AdminSiteNotification,
                member.Email,
                new Dictionary<string, string>
                {
                    ["AdminName"] = string.IsNullOrWhiteSpace(member.Givenname) ? member.Email : member.Givenname,
                    ["Title"] = safeTitle,
                    ["MessageBody"] = safeMessage,
                    ["ActionUrl"] = safeUrl ?? string.Empty,
                },
                languageCode: null,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to email member notification to {Email} for website {WebsiteId}", member.Email, websiteId);
        }
    }

    public async Task<PagedResult<AdminNotification>> GetPagedAsync(int memberId, GridQuery query, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var q = context.AdminNotifications.AsNoTracking()
            .Where(n => n.MemberID == memberId);

        if (query.GetSearch(nameof(AdminNotification.IsRead)) is string read && bool.TryParse(read, out var isRead))
            q = q.Where(n => n.IsRead == isRead);
        if (query.GetSearch(nameof(AdminNotification.Title)) is string title)
            q = q.Where(n => n.Title.Contains(title));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(AdminNotification.CreatedAt), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<AdminNotification> { Items = items, TotalCount = total };
    }

    public async Task<IReadOnlyList<AdminNotification>> GetRecentAsync(int memberId, int take = 10, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.AdminNotifications.AsNoTracking()
            .Where(n => n.MemberID == memberId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(int memberId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.AdminNotifications.CountAsync(n => n.MemberID == memberId && !n.IsRead, ct);
    }

    public async Task MarkReadAsync(int notificationId, int memberId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await context.AdminNotifications
            .Where(n => n.AdminNotificationID == notificationId && n.MemberID == memberId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }

    public async Task MarkAllReadAsync(int memberId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await context.AdminNotifications
            .Where(n => n.MemberID == memberId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }

    public async Task DeleteAsync(int notificationId, int memberId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var entity = await context.AdminNotifications
            .FirstOrDefaultAsync(n => n.AdminNotificationID == notificationId && n.MemberID == memberId, ct);
        if (entity is null) return;
        context.AdminNotifications.Remove(entity);
        await context.SaveChangesAsync(ct);
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
