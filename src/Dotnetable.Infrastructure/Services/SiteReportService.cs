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

public class SiteReportService : ISiteReportService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IAdminNotificationService _notifications;
    private readonly IEmailService _email;
    private readonly ILogger<SiteReportService> _logger;

    public SiteReportService(
        IDbContextFactory<AppDbContext> contextFactory,
        IAdminNotificationService notifications,
        IEmailService email,
        ILogger<SiteReportService> logger)
    {
        _contextFactory = contextFactory;
        _notifications = notifications;
        _email = email;
        _logger = logger;
    }

    public async Task<PagedResult<SiteReport>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.SiteReports.AsNoTracking().Include(r => r.Website).AsQueryable();
        if (websiteId.HasValue)
            q = q.Where(r => r.WebsiteID == websiteId.Value);

        if (query.GetSearch(nameof(SiteReport.ReporterName)) is string name)
            q = q.Where(r => r.ReporterName.Contains(name));
        if (query.GetSearch(nameof(SiteReport.EmailAddress)) is string email)
            q = q.Where(r => r.EmailAddress.Contains(email));
        if (query.GetSearch(nameof(SiteReport.Subject)) is string subject)
            q = q.Where(r => r.Subject.Contains(subject));
        if (query.GetSearch(nameof(SiteReport.Kind)) is string kindText && byte.TryParse(kindText, out var kind))
            q = q.Where(r => r.Kind == kind);
        if (query.GetSearch(nameof(SiteReport.Status)) is string statusText && byte.TryParse(statusText, out var status))
            q = q.Where(r => r.Status == status);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(SiteReport.LogTime), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<SiteReport> { Items = items, TotalCount = total };
    }

    public async Task<SiteReport?> GetByIdAsync(int id, int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.SiteReports.AsNoTracking()
            .Include(r => r.Website)
            .FirstOrDefaultAsync(r => r.SiteReportID == id && (websiteId == null || r.WebsiteID == websiteId), ct);
    }

    public async Task<SiteReport> CreateAsync(SiteReport report, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (report.WebsiteClientID is int clientId)
        {
            var ownsClient = await _context.WebsiteClients.AnyAsync(
                c => c.WebsiteClientID == clientId && c.WebsiteID == report.WebsiteID, ct);
            if (!ownsClient)
                report.WebsiteClientID = null;
        }

        report.LogTime = DateTime.UtcNow;
        report.Status = (byte)SiteReportStatus.New;
        report.ReporterName = report.ReporterName.Trim();
        report.EmailAddress = report.EmailAddress.Trim();
        report.Subject = report.Subject.Trim();
        report.MessageBody = report.MessageBody.Trim();
        report.PageUrl = string.IsNullOrWhiteSpace(report.PageUrl) ? null : report.PageUrl.Trim();
        report.SenderIPAddress = string.IsNullOrWhiteSpace(report.SenderIPAddress) ? string.Empty : report.SenderIPAddress.Trim();

        _context.SiteReports.Add(report);
        await _context.SaveChangesAsync(ct);

        var kindName = KindName((SiteReportKind)report.Kind);
        var preview = string.IsNullOrWhiteSpace(report.Subject) ? kindName : report.Subject;
        await _notifications.NotifySiteAdminsAsync(
            report.WebsiteID,
            AdminNotificationType.SiteReport,
            "New site report",
            $"{report.ReporterName} ({report.EmailAddress}): {preview}",
            $"/messages/reports/{report.SiteReportID}",
            report.SiteReportID,
            ct);

        await EmailManagerAsync(_context, report, kindName, ct);
        return report;
    }

    public async Task<bool> UpdateReviewAsync(int id, int? websiteId, SiteReportStatus status, string? adminNote, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var entity = await _context.SiteReports
            .FirstOrDefaultAsync(r => r.SiteReportID == id && (websiteId == null || r.WebsiteID == websiteId), ct);
        if (entity is null) return false;

        entity.Status = (byte)status;
        entity.AdminNote = string.IsNullOrWhiteSpace(adminNote) ? null : adminNote.Trim();
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task DeleteAsync(int id, int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var entity = await _context.SiteReports
            .FirstOrDefaultAsync(r => r.SiteReportID == id && (websiteId == null || r.WebsiteID == websiteId), ct);
        if (entity is null) return;

        _context.SiteReports.Remove(entity);
        await _context.SaveChangesAsync(ct);
        await _notifications.DeleteByRelatedEntityAsync(AdminNotificationType.SiteReport, id, ct);
    }

    private async Task EmailManagerAsync(AppDbContext _context, SiteReport report, string kindName, CancellationToken ct)
    {
        var site = await _context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == report.WebsiteID)
            .Select(w => new { w.Email, w.Manager, w.DefaultLanguageCode })
            .FirstOrDefaultAsync(ct);
        if (site is null || string.IsNullOrWhiteSpace(site.Email)) return;
        if (!await _email.IsConfiguredAsync(report.WebsiteID, ct)) return;

        try
        {
            await _email.SendTemplateAsync(
                report.WebsiteID,
                EmailTemplateKeys.SiteReportNotification,
                site.Email.Trim(),
                new Dictionary<string, string>
                {
                    ["ManagerName"] = string.IsNullOrWhiteSpace(site.Manager) ? site.Email : site.Manager,
                    ["ReporterName"] = report.ReporterName,
                    ["Email"] = report.EmailAddress,
                    ["Kind"] = kindName,
                    ["Subject"] = report.Subject,
                    ["MessageBody"] = report.MessageBody,
                    ["PageUrl"] = report.PageUrl ?? "—",
                },
                site.DefaultLanguageCode,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to email site report {ReportId} to the site manager", report.SiteReportID);
        }
    }

    internal static string KindName(SiteReportKind kind) => kind switch
    {
        SiteReportKind.Bug => "Bug",
        SiteReportKind.BrokenPage => "Broken page",
        SiteReportKind.WrongContent => "Wrong or outdated content",
        SiteReportKind.InappropriateContent => "Inappropriate content",
        _ => "Other",
    };
}
