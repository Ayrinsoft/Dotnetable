using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Sends mail through the <c>EmailAccount</c> resolved for a website/type, and renders
/// <c>EmailTemplate</c>s ({{Token}} placeholders) before sending.
/// </summary>
public partial class EmailService : IEmailService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IEmailTemplateService _templates;

    public EmailService(IDbContextFactory<AppDbContext> contextFactory, IEmailTemplateService templates)
    {
        _contextFactory = contextFactory;
        _templates = templates;
    }

    public async Task<bool> IsConfiguredAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await EmailAccountService.ResolveAsync(_context, websiteId, EmailAccountType.NoReply, ct);
        return row is not null && !string.IsNullOrWhiteSpace(row.MailServer) && !string.IsNullOrWhiteSpace(row.EmailAddress);
    }

    public async Task SendAsync(
        int websiteId, EmailAccountType accountType, string toAddress, string subject, string htmlBody,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await EmailAccountService.ResolveAsync(_context, websiteId, accountType, ct)
            ?? throw new InvalidOperationException("Email has not been configured.");
        if (string.IsNullOrWhiteSpace(row.MailServer) || string.IsNullOrWhiteSpace(row.EmailAddress))
            throw new InvalidOperationException("Email has not been configured.");

        await SendViaAsync(row, toAddress, subject, htmlBody, ct);
    }

    public async Task SendTemplateAsync(
        int websiteId, string templateKey, string toAddress, IDictionary<string, string>? tokens = null,
        string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var website = await _context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);

        var resolvedLanguage = string.IsNullOrWhiteSpace(languageCode)
            ? website?.DefaultLanguageCode
            : languageCode.Trim().ToLowerInvariant();

        var template = await _templates.GetAsync(websiteId, templateKey, resolvedLanguage, ct)
            ?? throw new InvalidOperationException($"Unknown email template '{templateKey}'.");

        var all = new Dictionary<string, string>(tokens ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase)
        {
            ["SiteName"] = website?.BrandName ?? website?.TradeName ?? string.Empty,
            ["SiteUrl"] = website?.WebsiteAddress ?? string.Empty,
            ["Year"] = DateTime.UtcNow.Year.ToString(),
        };

        var subject = Render(template.Subject, all);
        var body = Render(template.HtmlBody, all);

        if (!string.IsNullOrWhiteSpace(resolvedLanguage))
        {
            var rtl = await _context.Languages.AsNoTracking()
                .Where(l => l.WebsiteID == websiteId && l.LanguageCode == resolvedLanguage && l.Active)
                .Select(l => (bool?)l.RTLDesign)
                .FirstOrDefaultAsync(ct)
                ?? await _context.Languages.AsNoTracking()
                    .Where(l => l.LanguageCode == resolvedLanguage && l.Active)
                    .Select(l => (bool?)l.RTLDesign)
                    .FirstOrDefaultAsync(ct)
                ?? false;

            body = ApplyDocumentDirection(body, resolvedLanguage, rtl);
        }

        await SendAsync(websiteId, template.AccountType, toAddress, subject, body, ct);
    }

    private static async Task SendViaAsync(EmailAccount account, string toAddress, string subject, string htmlBody, CancellationToken ct)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(account.EmailAddress, string.IsNullOrWhiteSpace(account.MailName) ? account.EmailAddress : account.MailName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(toAddress);

        using var client = new SmtpClient(account.MailServer, account.SMTPPort)
        {
            EnableSsl = account.EnableSSL,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = string.IsNullOrWhiteSpace(account.Password)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(account.EmailAddress, account.Password),
        };

        await client.SendMailAsync(message, ct);
    }

    private static string Render(string template, IDictionary<string, string> tokens) =>
        TokenPattern().Replace(template, m =>
            tokens.TryGetValue(m.Groups[1].Value, out var value) ? value : m.Value);

    /// <summary>
    /// Ensures the outgoing HTML declares <c>dir</c>/<c>lang</c> so RTL languages render correctly
    /// in email clients. Replaces existing attributes on <c>&lt;html&gt;</c> when present; otherwise
    /// wraps the fragment.
    /// </summary>
    public static string ApplyDocumentDirection(string html, string languageCode, bool rtl)
    {
        if (string.IsNullOrWhiteSpace(html)) return html;

        var dir = rtl ? "rtl" : "ltr";
        var lang = languageCode.Trim().ToLowerInvariant();

        if (HtmlOpenTag().IsMatch(html))
        {
            return HtmlOpenTag().Replace(html, m =>
            {
                var rest = m.Groups[1].Value;
                rest = DirAttr().Replace(rest, string.Empty);
                rest = LangAttr().Replace(rest, string.Empty);
                rest = rest.TrimEnd();
                return string.IsNullOrEmpty(rest)
                    ? $"<html dir=\"{dir}\" lang=\"{lang}\">"
                    : $"<html dir=\"{dir}\" lang=\"{lang}\"{rest}>";
            }, 1);
        }

        return $"<div dir=\"{dir}\" lang=\"{lang}\">{html}</div>";
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex TokenPattern();

    [GeneratedRegex(@"<html(\s[^>]*)?>", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlOpenTag();

    [GeneratedRegex(@"\sdir\s*=\s*[""'][^""']*[""']", RegexOptions.IgnoreCase)]
    private static partial Regex DirAttr();

    [GeneratedRegex(@"\slang\s*=\s*[""'][^""']*[""']", RegexOptions.IgnoreCase)]
    private static partial Regex LangAttr();
}
