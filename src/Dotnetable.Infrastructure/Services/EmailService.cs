using System.Net;
using System.Net.Mail;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Persists the single default SMTP configuration in the <c>EmailSetting</c> table and sends mail
/// through it with <see cref="SmtpClient"/>.
/// </summary>
public class EmailService : IEmailService
{
    private readonly AppDbContext _context;

    public EmailService(AppDbContext context) => _context = context;

    public async Task<EmailSettingsInfo?> GetDefaultAsync(CancellationToken ct = default)
    {
        var row = await GetDefaultRowAsync(ct);
        if (row is null) return null;

        return new EmailSettingsInfo
        {
            MailServer = row.MailServer,
            SmtpPort = row.SMTPPort,
            EnableSSL = row.EnableSSL,
            EmailAddress = row.EmailAddress,
            Password = row.Password,
            MailName = row.MailName,
        };
    }

    public async Task SaveDefaultAsync(EmailSettingsInfo settings, CancellationToken ct = default)
    {
        var row = await GetDefaultRowAsync(ct);
        if (row is null)
        {
            row = new EmailSetting { DefaultEMail = true, EmailTypeID = 0 };
            _context.EmailSettings.Add(row);
        }

        row.MailServer = settings.MailServer.Trim();
        row.SMTPPort = settings.SmtpPort;
        row.EnableSSL = settings.EnableSSL;
        row.EmailAddress = settings.EmailAddress.Trim();
        row.Password = settings.Password;
        row.MailName = string.IsNullOrWhiteSpace(settings.MailName) ? settings.EmailAddress.Trim() : settings.MailName.Trim();
        row.DefaultEMail = true;
        row.Active = settings.IsConfigured;

        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> IsConfiguredAsync(CancellationToken ct = default)
    {
        var row = await GetDefaultRowAsync(ct);
        return row is not null && !string.IsNullOrWhiteSpace(row.MailServer) && !string.IsNullOrWhiteSpace(row.EmailAddress);
    }

    public async Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken ct = default)
    {
        var row = await GetDefaultRowAsync(ct)
            ?? throw new InvalidOperationException("Email has not been configured.");
        if (string.IsNullOrWhiteSpace(row.MailServer) || string.IsNullOrWhiteSpace(row.EmailAddress))
            throw new InvalidOperationException("Email has not been configured.");

        using var message = new MailMessage
        {
            From = new MailAddress(row.EmailAddress, string.IsNullOrWhiteSpace(row.MailName) ? row.EmailAddress : row.MailName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(toAddress);

        using var client = new SmtpClient(row.MailServer, row.SMTPPort)
        {
            EnableSsl = row.EnableSSL,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = string.IsNullOrWhiteSpace(row.Password)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(row.EmailAddress, row.Password),
        };

        await client.SendMailAsync(message, ct);
    }

    private async Task<EmailSetting?> GetDefaultRowAsync(CancellationToken ct) =>
        await _context.EmailSettings
            .OrderByDescending(e => e.DefaultEMail)
            .ThenByDescending(e => e.EmailSettingID)
            .FirstOrDefaultAsync(ct);
}
