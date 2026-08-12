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
/// Channels: in-app + email template + WhatsApp when gateway is configured.
/// </summary>
public class AdminNotificationService : IAdminNotificationService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IEmailService _email;
    private readonly IWhatsAppSender _whatsApp;
    private readonly ILogger<AdminNotificationService> _logger;

    public AdminNotificationService(
        IDbContextFactory<AppDbContext> contextFactory,
        IEmailService email,
        IWhatsAppSender whatsApp,
        ILogger<AdminNotificationService> logger)
    {
        _contextFactory = contextFactory;
        _email = email;
        _whatsApp = whatsApp;
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
            .Where(m => m.WebsiteID == websiteId && m.Active && m.IsSiteAdmin)
            .Select(m => new Recipient(m.MemberID, m.Email, m.Givenname, m.CellphoneNumber, m.CountryCode))
            .ToListAsync(ct);

        if (admins.Count == 0) return;
        await DeliverAsync(context, websiteId, type, title, message, actionUrl, relatedEntityId, admins, ct);
    }

    public async Task NotifyRoleAsync(
        int websiteId,
        IReadOnlyList<string> roleKeys,
        AdminNotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        int? relatedEntityId = null,
        CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var keys = roleKeys.Where(k => !string.IsNullOrWhiteSpace(k)).Select(k => k.Trim()).Distinct().ToList();
        List<Recipient> recipients;

        if (keys.Count == 0)
        {
            recipients = await context.Members.AsNoTracking()
                .Where(m => m.WebsiteID == websiteId && m.Active && m.IsSiteAdmin)
                .Select(m => new Recipient(m.MemberID, m.Email, m.Givenname, m.CellphoneNumber, m.CountryCode))
                .ToListAsync(ct);
        }
        else
        {
            // Members whose policy includes any of the role keys, plus site admins.
            var policyIds = await context.PolicyRoles.AsNoTracking()
                .Where(pr => pr.Active && pr.Role.Active && keys.Contains(pr.Role.RoleKey))
                .Select(pr => pr.PolicyID)
                .Distinct()
                .ToListAsync(ct);

            recipients = await context.Members.AsNoTracking()
                .Where(m => m.WebsiteID == websiteId && m.Active
                            && (m.IsSiteAdmin || policyIds.Contains(m.PolicyID)))
                .Select(m => new Recipient(m.MemberID, m.Email, m.Givenname, m.CellphoneNumber, m.CountryCode))
                .ToListAsync(ct);
        }

        if (recipients.Count == 0) return;
        // De-dupe by member id
        recipients = recipients.GroupBy(r => r.MemberID).Select(g => g.First()).ToList();
        await DeliverAsync(context, websiteId, type, title, message, actionUrl, relatedEntityId, recipients, ct);
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
            .Select(m => new Recipient(m.MemberID, m.Email, m.Givenname, m.CellphoneNumber, m.CountryCode))
            .FirstOrDefaultAsync(ct);
        if (member is null) return;

        await DeliverAsync(context, websiteId, type, title, message, actionUrl, relatedEntityId, [member], ct);
    }

    private async Task DeliverAsync(
        AppDbContext context,
        int websiteId,
        AdminNotificationType type,
        string title,
        string message,
        string? actionUrl,
        int? relatedEntityId,
        IReadOnlyList<Recipient> recipients,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var safeTitle = Truncate(title, 200);
        var safeMessage = Truncate(message, 1000);
        var safeUrl = actionUrl is null ? null : Truncate(actionUrl, 256);

        foreach (var r in recipients)
        {
            context.AdminNotifications.Add(new AdminNotification
            {
                MemberID = r.MemberID,
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

        var emailOk = await _email.IsConfiguredAsync(websiteId, ct);
        foreach (var r in recipients)
        {
            if (emailOk && !string.IsNullOrWhiteSpace(r.Email))
            {
                try
                {
                    await _email.SendTemplateAsync(
                        websiteId,
                        EmailTemplateKeys.AdminSiteNotification,
                        r.Email,
                        new Dictionary<string, string>
                        {
                            ["AdminName"] = string.IsNullOrWhiteSpace(r.Givenname) ? r.Email : r.Givenname,
                            ["Title"] = safeTitle,
                            ["MessageBody"] = safeMessage,
                            ["ActionUrl"] = safeUrl ?? string.Empty,
                        },
                        languageCode: null,
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to email admin notification to {Email}", r.Email);
                }
            }

            if (_whatsApp.IsConfigured
                && !string.IsNullOrWhiteSpace(r.Cellphone)
                && !string.IsNullOrWhiteSpace(r.CountryCode))
            {
                try
                {
                    var wa = string.IsNullOrWhiteSpace(safeUrl)
                        ? $"{safeTitle}\n{safeMessage}"
                        : $"{safeTitle}\n{safeMessage}\n{safeUrl}";
                    await _whatsApp.SendAsync(r.CountryCode, r.Cellphone, wa, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to WhatsApp admin notification to {Phone}", r.Cellphone);
                }
            }
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
        var n = await context.AdminNotifications.FirstOrDefaultAsync(x => x.AdminNotificationID == notificationId && x.MemberID == memberId, ct);
        if (n is null) return;
        n.IsRead = true;
        await context.SaveChangesAsync(ct);
    }

    public async Task MarkAllReadAsync(int memberId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var list = await context.AdminNotifications.Where(n => n.MemberID == memberId && !n.IsRead).ToListAsync(ct);
        foreach (var n in list) n.IsRead = true;
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int notificationId, int memberId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var n = await context.AdminNotifications.FirstOrDefaultAsync(x => x.AdminNotificationID == notificationId && x.MemberID == memberId, ct);
        if (n is null) return;
        context.AdminNotifications.Remove(n);
        await context.SaveChangesAsync(ct);
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];

    private sealed record Recipient(int MemberID, string Email, string Givenname, string Cellphone, string CountryCode);
}
