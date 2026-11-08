using System.Globalization;
using System.Net;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Email;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Messaging;
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
/// The site owner (<c>Website.Email</c>, or website 1's email when this site has none) receives one
/// email for every alert. No address on either site skips that email. SMTP still uses this site's
/// account, then website 1's.
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

        if (recipients.Count > 0)
        {
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
        }

        var emailOk = await _email.IsConfiguredAsync(websiteId, ct);
        var alert = await DescribeAlertAsync(context, websiteId, safeTitle, safeMessage, relatedEntityId, ct);
        // One send per address. Site reports use their own template for the owner mailbox.
        var emailed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in recipients)
        {
            using var logScope = MessageLogScope.Begin(MessageLogSources.Notification,
                recipientType: MessageRecipientType.Member, recipientId: r.MemberID,
                recipientName: string.IsNullOrWhiteSpace(r.Givenname) ? r.Email : r.Givenname);

            if (emailOk && !string.IsNullOrWhiteSpace(r.Email))
            {
                var address = r.Email.Trim();
                if (emailed.Add(address))
                {
                    try
                    {
                        await SendAlertAsync(websiteId, address,
                            string.IsNullOrWhiteSpace(r.Givenname) ? address : r.Givenname,
                            alert, safeUrl, languageCode: null, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to email admin notification to {Email}", r.Email);
                    }
                }
            }

            if (await _whatsApp.IsConfiguredAsync(websiteId, ct)
                && !string.IsNullOrWhiteSpace(r.Cellphone)
                && !string.IsNullOrWhiteSpace(r.CountryCode))
            {
                try
                {
                    var wa = string.IsNullOrWhiteSpace(safeUrl)
                        ? alert.Plain
                        : $"{alert.Plain}\n{safeUrl}";
                    await _whatsApp.SendAsync(websiteId, r.CountryCode, r.Cellphone, wa, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to WhatsApp admin notification to {Phone}", r.Cellphone);
                }
            }
        }

        // Site reports already mail the owner with SiteReportNotification.
        if (type == AdminNotificationType.SiteReport || !emailOk)
            return;

        var owner = await SiteOwnerMail.ResolveAsync(context, websiteId, ct);
        if (owner is null || !emailed.Add(owner.Email))
            return;

        try
        {
            await SendAlertAsync(websiteId, owner.Email, owner.Name, alert, safeUrl, owner.LanguageCode, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to email admin notification to site owner {Email}", owner.Email);
        }
    }

    private Task SendAlertAsync(
        int websiteId, string toAddress, string adminName, AlertCopy alert, string? actionUrl,
        string? languageCode, CancellationToken ct) =>
        _email.SendTemplateAsync(
            websiteId,
            EmailTemplateKeys.AdminSiteNotification,
            toAddress,
            new Dictionary<string, string>
            {
                ["AdminName"] = adminName,
                ["Title"] = alert.Title,
                ["Action"] = alert.Action,
                ["SentAt"] = alert.SentAt,
                ["MessageBody"] = alert.Html,
                ["ActionUrl"] = actionUrl ?? string.Empty,
            },
            languageCode,
            ct);

    /// <summary>
    /// Subject and body name the action and the website. Several sites can share one owner mailbox,
    /// and an already-seeded template only prints <c>{{Title}}</c> and <c>{{MessageBody}}</c>.
    /// The subject ends with a unique stamp so each send is its own inbox thread.
    /// </summary>
    private static async Task<AlertCopy> DescribeAlertAsync(
        AppDbContext context, int websiteId, string action, string message, int? relatedEntityId, CancellationToken ct)
    {
        var site = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => new { w.BrandName, w.TradeName, w.WebsiteAddress })
            .FirstOrDefaultAsync(ct);

        var siteName = FirstText(site?.BrandName, site?.TradeName) ?? $"Website {websiteId}";
        var siteUrl = site?.WebsiteAddress?.Trim() ?? string.Empty;
        var siteLabel = string.IsNullOrWhiteSpace(siteUrl) ? siteName : $"{siteName} ({siteUrl})";
        var safeAction = string.IsNullOrWhiteSpace(action) ? "Notification" : action;

        var html = $"Action: <strong>{WebUtility.HtmlEncode(safeAction)}</strong><br>Site: <strong>{WebUtility.HtmlEncode(siteName)}</strong>";
        if (!string.IsNullOrWhiteSpace(siteUrl))
            html += $" ({WebUtility.HtmlEncode(siteUrl)})";
        html += "<br><br>" + WebUtility.HtmlEncode(message).Replace("\n", "<br>");

        var sentAt = SubjectStamp(relatedEntityId);
        return new AlertCopy(
            Title: $"{safeAction} — {siteLabel} — {sentAt}",
            Action: safeAction,
            SentAt: sentAt,
            Html: html,
            Plain: $"Action: {safeAction}\nSite: {siteLabel}\n\n{message}");
    }

    /// <summary>Changes on every send so mail clients do not file the message under the previous one.</summary>
    internal static string SubjectStamp(int? relatedEntityId)
    {
        var clock = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff' UTC'", CultureInfo.InvariantCulture);
        return relatedEntityId is int id ? $"#{id} — {clock}" : clock;
    }

    private static string? FirstText(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }
        return null;
    }

    private sealed record AlertCopy(string Title, string Action, string SentAt, string Html, string Plain);

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

    public async Task DeleteByRelatedEntityAsync(AdminNotificationType type, int relatedEntityId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await context.AdminNotifications
            .Where(n => n.NotificationType == (byte)type && n.RelatedEntityID == relatedEntityId)
            .ExecuteDeleteAsync(ct);
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];

    private sealed record Recipient(int MemberID, string Email, string Givenname, string Cellphone, string CountryCode);
}
