using Dotnetable.Application;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

/// <inheritdoc cref="IEmailAccountService"/>
public class EmailAccountService : IEmailAccountService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public EmailAccountService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<List<EmailAccountInfo>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.EmailAccounts
            .Where(a => a.WebsiteID == websiteId)
            .OrderByDescending(a => a.EmailAccountID)
            .Select(a => ToInfo(a))
            .ToListAsync(ct);
    }

    public async Task<EmailAccountInfo?> GetByIdAsync(int emailAccountId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await _context.EmailAccounts.FirstOrDefaultAsync(a => a.EmailAccountID == emailAccountId, ct);
        return row is null ? null : ToInfo(row);
    }

    public async Task<int> SaveAsync(EmailAccountInfo account, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        EmailAccount row;
        if (account.EmailAccountID == 0)
        {
            row = new EmailAccount { WebsiteID = account.WebsiteID };
            _context.EmailAccounts.Add(row);
        }
        else
        {
            row = await _context.EmailAccounts.FirstOrDefaultAsync(a => a.EmailAccountID == account.EmailAccountID, ct)
                ?? throw new InvalidOperationException("Email account not found.");
        }

        row.AccountType = (byte)account.AccountType;
        row.Name = string.IsNullOrWhiteSpace(account.Name) ? account.AccountType.ToString() : account.Name.Trim();
        row.MailServer = account.MailServer.Trim();
        row.SMTPPort = account.SmtpPort;
        row.EnableSSL = account.EnableSSL;
        row.EmailAddress = account.EmailAddress.Trim();
        row.Password = account.Password;
        row.MailName = string.IsNullOrWhiteSpace(account.MailName) ? account.EmailAddress.Trim() : account.MailName.Trim();
        row.IsDefault = account.IsDefault;
        row.Active = account.Active;

        // Only one default account per website — demote any previous default.
        if (row.IsDefault)
        {
            var others = await _context.EmailAccounts
                .Where(a => a.WebsiteID == row.WebsiteID && a.IsDefault && a.EmailAccountID != row.EmailAccountID)
                .ToListAsync(ct);
            foreach (var other in others) other.IsDefault = false;
        }

        await _context.SaveChangesAsync(ct);
        return row.EmailAccountID;
    }

    public async Task DeleteAsync(int emailAccountId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await _context.EmailAccounts.FirstOrDefaultAsync(a => a.EmailAccountID == emailAccountId, ct);
        if (row is null) return;
        _context.EmailAccounts.Remove(row);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> IsConfiguredAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await ResolveAsync(_context, websiteId, EmailAccountType.NoReply, ct);
        return row is not null && !string.IsNullOrWhiteSpace(row.MailServer) && !string.IsNullOrWhiteSpace(row.EmailAddress);
    }

    /// <summary>
    /// Resolves the best account for (websiteId, accountType): the website's own account of that type,
    /// else its own default account, else the master website's account of that type, else the master
    /// website's default account.
    /// </summary>
    internal static async Task<EmailAccount?> ResolveAsync(
        AppDbContext context, int websiteId, EmailAccountType accountType, CancellationToken ct)
    {
        var type = (byte)accountType;

        var candidates = await context.EmailAccounts
            .Where(a => a.Active && (a.WebsiteID == websiteId || a.WebsiteID == AppConstants.MasterWebsiteId))
            .ToListAsync(ct);

        return candidates.FirstOrDefault(a => a.WebsiteID == websiteId && a.AccountType == type)
            ?? candidates.FirstOrDefault(a => a.WebsiteID == websiteId && a.IsDefault)
            ?? candidates.FirstOrDefault(a => a.WebsiteID == AppConstants.MasterWebsiteId && a.AccountType == type)
            ?? candidates.FirstOrDefault(a => a.WebsiteID == AppConstants.MasterWebsiteId && a.IsDefault);
    }

    private static EmailAccountInfo ToInfo(EmailAccount a) => new()
    {
        EmailAccountID = a.EmailAccountID,
        WebsiteID = a.WebsiteID,
        AccountType = (EmailAccountType)a.AccountType,
        Name = a.Name,
        MailServer = a.MailServer,
        SmtpPort = a.SMTPPort,
        EnableSSL = a.EnableSSL,
        EmailAddress = a.EmailAddress,
        Password = a.Password,
        MailName = a.MailName,
        IsDefault = a.IsDefault,
        Active = a.Active,
    };
}
