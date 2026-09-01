using Dotnetable.Application;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Email;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

/// <inheritdoc cref="IEmailTemplateService"/>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public EmailTemplateService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<List<EmailTemplateInfo>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var rows = await _context.EmailTemplates
            .AsNoTracking()
            .Include(t => t.EmailTemplateTranslations)
            .Where(t => t.WebsiteID == websiteId || t.WebsiteID == AppConstants.MasterWebsiteId)
            .ToListAsync(ct);

        var result = new List<EmailTemplateInfo>();
        foreach (var def in EmailTemplateDefaults.All)
        {
            var own = websiteId != AppConstants.MasterWebsiteId
                ? rows.FirstOrDefault(r => r.WebsiteID == websiteId && r.TemplateKey == def.Key)
                : null;
            var fallback = rows.FirstOrDefault(r => r.WebsiteID == AppConstants.MasterWebsiteId && r.TemplateKey == def.Key);
            var row = own ?? fallback;

            result.Add(row is null
                ? FromDefault(websiteId, def)
                : ToInfo(row, isOverride: own is not null));
        }
        return result;
    }

    public async Task<EmailTemplateInfo?> GetAsync(
        int websiteId, string templateKey, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var def = EmailTemplateDefaults.All.FirstOrDefault(d => d.Key == templateKey);
        if (def is null) return null;

        var own = websiteId != AppConstants.MasterWebsiteId
            ? await _context.EmailTemplates
                .AsNoTracking()
                .Include(t => t.EmailTemplateTranslations)
                .FirstOrDefaultAsync(t => t.WebsiteID == websiteId && t.TemplateKey == templateKey, ct)
            : null;
        var row = own ?? await _context.EmailTemplates
            .AsNoTracking()
            .Include(t => t.EmailTemplateTranslations)
            .FirstOrDefaultAsync(t => t.WebsiteID == AppConstants.MasterWebsiteId && t.TemplateKey == templateKey, ct);

        var info = row is null
            ? FromDefault(websiteId, def)
            : ToInfo(row, isOverride: own is not null);

        if (!string.IsNullOrWhiteSpace(languageCode) && row is not null)
        {
            var defaultCode = await _context.Websites.AsNoTracking()
                .Where(w => w.WebsiteID == websiteId)
                .Select(w => w.DefaultLanguageCode)
                .FirstOrDefaultAsync(ct);

            if (!string.IsNullOrWhiteSpace(defaultCode)
                && !string.Equals(languageCode, defaultCode, StringComparison.OrdinalIgnoreCase))
            {
                var tr = row.EmailTemplateTranslations.FirstOrDefault(t =>
                    string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));
                if (tr is not null && !string.IsNullOrWhiteSpace(tr.Subject))
                {
                    info.Subject = tr.Subject;
                    if (!string.IsNullOrWhiteSpace(tr.HtmlBody))
                        info.HtmlBody = tr.HtmlBody;
                }
            }
        }

        // Built-in FA (etc.) defaults when the site has no own override and no matching translation.
        if (!info.IsOverride
            && !string.IsNullOrWhiteSpace(languageCode)
            && EmailTemplateDefaults.TryGetLocalizedDefault(templateKey, languageCode, out var locSubject, out var locBody))
        {
            var hasTranslation = row?.EmailTemplateTranslations.Any(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(t.Subject)) == true;
            if (!hasTranslation)
            {
                info.Subject = locSubject;
                info.HtmlBody = locBody;
            }
        }

        return info;
    }

    public async Task SaveAsync(int websiteId, EmailTemplateInfo template, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await EnsureOwnRowAsync(_context, websiteId, template.TemplateKey, ct);

        row.Name = template.Name;
        row.Subject = template.Subject;
        row.HtmlBody = template.HtmlBody;
        row.AccountType = (byte)template.AccountType;
        row.Active = template.Active;

        await _context.SaveChangesAsync(ct);
    }

    public async Task SetTranslationsAsync(
        int websiteId,
        string templateKey,
        IReadOnlyDictionary<string, (string Subject, string HtmlBody)> byLanguage,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await EnsureOwnRowAsync(_context, websiteId, templateKey, ct);
        // EnsureOwnRow may have added a new tracked entity without an ID until save.
        if (row.EmailTemplateID == 0)
            await _context.SaveChangesAsync(ct);

        var existing = await _context.EmailTemplateTranslations
            .Where(t => t.EmailTemplateID == row.EmailTemplateID)
            .ToListAsync(ct);

        foreach (var (languageCode, value) in byLanguage)
        {
            var code = languageCode.Trim().ToLowerInvariant();
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, code, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(value.Subject))
            {
                if (current is not null) _context.EmailTemplateTranslations.Remove(current);
                continue;
            }

            if (current is null)
            {
                _context.EmailTemplateTranslations.Add(new EmailTemplateTranslation
                {
                    EmailTemplateID = row.EmailTemplateID,
                    LanguageCode = code,
                    Subject = value.Subject.Trim(),
                    HtmlBody = value.HtmlBody ?? string.Empty,
                });
            }
            else
            {
                current.Subject = value.Subject.Trim();
                current.HtmlBody = value.HtmlBody ?? string.Empty;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task ResetToDefaultAsync(int websiteId, string templateKey, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await _context.EmailTemplates
            .Include(t => t.EmailTemplateTranslations)
            .FirstOrDefaultAsync(t => t.WebsiteID == websiteId && t.TemplateKey == templateKey, ct);
        if (row is null) return;
        _context.EmailTemplateTranslations.RemoveRange(row.EmailTemplateTranslations);
        _context.EmailTemplates.Remove(row);
        await _context.SaveChangesAsync(ct);
    }

    private async Task<EmailTemplate> EnsureOwnRowAsync(AppDbContext _context, int websiteId, string templateKey, CancellationToken ct)
    {
        var row = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.WebsiteID == websiteId && t.TemplateKey == templateKey, ct);
        if (row is not null) return row;

        var def = EmailTemplateDefaults.All.FirstOrDefault(d => d.Key == templateKey);
        var fallback = await _context.EmailTemplates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.WebsiteID == AppConstants.MasterWebsiteId && t.TemplateKey == templateKey, ct);

        row = new EmailTemplate
        {
            WebsiteID = websiteId,
            TemplateKey = templateKey,
            Name = fallback?.Name ?? def?.Name ?? templateKey,
            Subject = fallback?.Subject ?? def?.Subject ?? templateKey,
            HtmlBody = fallback?.HtmlBody ?? def?.HtmlBody ?? string.Empty,
            AccountType = fallback?.AccountType ?? (byte)(def?.AccountType ?? EmailAccountType.NoReply),
            Active = fallback?.Active ?? true,
        };
        _context.EmailTemplates.Add(row);
        return row;
    }

    private static EmailTemplateInfo FromDefault(int websiteId, EmailTemplateDefault def) => new()
    {
        WebsiteID = websiteId,
        TemplateKey = def.Key,
        Name = def.Name,
        Subject = def.Subject,
        HtmlBody = def.HtmlBody,
        AccountType = def.AccountType,
        Active = true,
        IsOverride = false,
    };

    private static EmailTemplateInfo ToInfo(EmailTemplate t, bool isOverride) => new()
    {
        EmailTemplateID = t.EmailTemplateID,
        WebsiteID = t.WebsiteID,
        TemplateKey = t.TemplateKey,
        Name = t.Name,
        Subject = t.Subject,
        HtmlBody = t.HtmlBody,
        AccountType = (EmailAccountType)t.AccountType,
        Active = t.Active,
        IsOverride = isOverride,
        Translations = t.EmailTemplateTranslations
            .OrderBy(x => x.LanguageCode)
            .Select(x => new EmailTemplateTranslationInfo
            {
                LanguageCode = x.LanguageCode,
                Subject = x.Subject,
                HtmlBody = x.HtmlBody,
            })
            .ToList(),
    };
}
