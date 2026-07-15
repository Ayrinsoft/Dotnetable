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
    private readonly AppDbContext _context;

    public EmailTemplateService(AppDbContext context) => _context = context;

    public async Task<List<EmailTemplateInfo>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default)
    {
        var rows = await _context.EmailTemplates
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
                ? new EmailTemplateInfo
                {
                    WebsiteID = websiteId,
                    TemplateKey = def.Key,
                    Name = def.Name,
                    Subject = def.Subject,
                    HtmlBody = def.HtmlBody,
                    AccountType = def.AccountType,
                    Active = true,
                    IsOverride = false,
                }
                : ToInfo(row, isOverride: own is not null));
        }
        return result;
    }

    public async Task<EmailTemplateInfo?> GetAsync(int websiteId, string templateKey, CancellationToken ct = default)
    {
        var def = EmailTemplateDefaults.All.FirstOrDefault(d => d.Key == templateKey);
        if (def is null) return null;

        var own = websiteId != AppConstants.MasterWebsiteId
            ? await _context.EmailTemplates.FirstOrDefaultAsync(t => t.WebsiteID == websiteId && t.TemplateKey == templateKey, ct)
            : null;
        var row = own ?? await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.WebsiteID == AppConstants.MasterWebsiteId && t.TemplateKey == templateKey, ct);

        if (row is null)
        {
            return new EmailTemplateInfo
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
        }

        return ToInfo(row, isOverride: own is not null);
    }

    public async Task SaveAsync(int websiteId, EmailTemplateInfo template, CancellationToken ct = default)
    {
        var row = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.WebsiteID == websiteId && t.TemplateKey == template.TemplateKey, ct);

        if (row is null)
        {
            row = new EmailTemplate { WebsiteID = websiteId, TemplateKey = template.TemplateKey };
            _context.EmailTemplates.Add(row);
        }

        row.Name = template.Name;
        row.Subject = template.Subject;
        row.HtmlBody = template.HtmlBody;
        row.AccountType = (byte)template.AccountType;
        row.Active = template.Active;

        await _context.SaveChangesAsync(ct);
    }

    public async Task ResetToDefaultAsync(int websiteId, string templateKey, CancellationToken ct = default)
    {
        var row = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.WebsiteID == websiteId && t.TemplateKey == templateKey, ct);
        if (row is null) return;
        _context.EmailTemplates.Remove(row);
        await _context.SaveChangesAsync(ct);
    }

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
    };
}
