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
    private readonly AppDbContext _context;
    private readonly IEmailTemplateService _templates;

    public EmailService(AppDbContext context, IEmailTemplateService templates)
    {
        _context = context;
        _templates = templates;
    }

    public async Task<bool> IsConfiguredAsync(int websiteId, CancellationToken ct = default)
    {
        var row = await EmailAccountService.ResolveAsync(_context, websiteId, EmailAccountType.NoReply, ct);
        return row is not null && !string.IsNullOrWhiteSpace(row.MailServer) && !string.IsNullOrWhiteSpace(row.EmailAddress);
    }

    public async Task SendAsync(
        int websiteId, EmailAccountType accountType, string toAddress, string subject, string htmlBody,
        CancellationToken ct = default)
    {
        var row = await EmailAccountService.ResolveAsync(_context, websiteId, accountType, ct)
            ?? throw new InvalidOperationException("Email has not been configured.");
        if (string.IsNullOrWhiteSpace(row.MailServer) || string.IsNullOrWhiteSpace(row.EmailAddress))
            throw new InvalidOperationException("Email has not been configured.");

        await SendViaAsync(row, toAddress, subject, htmlBody, ct);
    }

    public async Task SendTemplateAsync(
        int websiteId, string templateKey, string toAddress, IDictionary<string, string>? tokens = null,
        CancellationToken ct = default)
    {
        var template = await _templates.GetAsync(websiteId, templateKey, ct)
            ?? throw new InvalidOperationException($"Unknown email template '{templateKey}'.");

        var website = await _context.Websites.FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);
        var all = new Dictionary<string, string>(tokens ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase)
        {
            ["SiteName"] = website?.BrandName ?? website?.TradeName ?? string.Empty,
            ["SiteUrl"] = website?.WebsiteAddress ?? string.Empty,
        };

        var subject = Render(template.Subject, all);
        var body = Render(template.HtmlBody, all);

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

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex TokenPattern();
}
