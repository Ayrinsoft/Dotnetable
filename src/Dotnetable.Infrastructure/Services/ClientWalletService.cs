using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ClientWalletService : IClientWalletService
{
    private readonly AppDbContext _context;
    private readonly ICurrencyConversionService _currency;

    public ClientWalletService(AppDbContext context, ICurrencyConversionService currency)
    {
        _context = context;
        _currency = currency;
    }

    public async Task<IReadOnlyList<WebsiteWalletCurrency>> GetEnabledWalletCurrenciesAsync(
        int websiteId, CancellationToken ct = default)
    {
        await EnsureDefaultWalletCurrencyAsync(websiteId, ct);
        return await _context.WebsiteWalletCurrencies.AsNoTracking()
            .Include(c => c.CurrencyCodeNavigation)
            .Where(c => c.WebsiteID == websiteId && c.IsActive)
            .OrderByDescending(c => c.IsDefault)
            .ThenBy(c => c.CurrencyCode)
            .ToListAsync(ct);
    }

    public async Task<(bool Success, string? Error)> EnableWalletCurrencyAsync(
        int websiteId, string currencyCode, CancellationToken ct = default)
    {
        await EnsureDefaultWalletCurrencyAsync(websiteId, ct);
        var code = NormalizeCode(currencyCode);
        if (code is null)
            return (false, "Currency code is required.");

        var currencyExists = await _context.Currencies.AsNoTracking()
            .AnyAsync(c => c.CurrencyCode == code && c.IsActive, ct);
        if (!currencyExists)
            return (false, "Currency not found or inactive.");

        var existing = await _context.WebsiteWalletCurrencies
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.CurrencyCode == code, ct);
        if (existing is not null)
        {
            existing.IsActive = true;
            await _context.SaveChangesAsync(ct);
            return (true, null);
        }

        _context.WebsiteWalletCurrencies.Add(new WebsiteWalletCurrency
        {
            WebsiteID = websiteId,
            CurrencyCode = code,
            IsDefault = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DisableWalletCurrencyAsync(
        int websiteId, string currencyCode, CancellationToken ct = default)
    {
        var code = NormalizeCode(currencyCode);
        if (code is null)
            return (false, "Currency code is required.");

        var row = await _context.WebsiteWalletCurrencies
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.CurrencyCode == code, ct);
        if (row is null)
            return (false, "Wallet currency not configured.");
        if (row.IsDefault)
            return (false, "Cannot disable the site default wallet currency.");

        var hasBalance = await _context.ClientWallets.AsNoTracking()
            .AnyAsync(w => w.WebsiteID == websiteId && w.CurrencyCode == code && w.Balance != 0, ct);
        if (hasBalance)
            return (false, "Cannot disable a currency while customer wallets still hold a balance.");

        row.IsActive = false;
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<ClientWallet> GetOrCreateAsync(
        int websiteId, int clientId, string? currencyCode = null, CancellationToken ct = default)
    {
        var code = await ResolveCurrencyCodeAsync(websiteId, currencyCode, ct);

        var wallet = await _context.ClientWallets
            .FirstOrDefaultAsync(w => w.WebsiteClientID == clientId && w.CurrencyCode == code, ct);
        if (wallet is not null)
            return wallet;

        var enabled = await IsCurrencyEnabledAsync(websiteId, code, ct);
        if (!enabled)
            throw new InvalidOperationException($"Wallet currency {code} is not enabled for this website.");

        wallet = new ClientWallet
        {
            WebsiteID = websiteId,
            WebsiteClientID = clientId,
            CurrencyCode = code,
            Balance = 0,
            BalanceUsd = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        _context.ClientWallets.Add(wallet);
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            _context.Entry(wallet).State = EntityState.Detached;
            wallet = await _context.ClientWallets
                .FirstAsync(w => w.WebsiteClientID == clientId && w.CurrencyCode == code, ct);
        }

        return wallet;
    }

    public async Task<IReadOnlyList<ClientWallet>> ListForClientAsync(
        int websiteId, int clientId, CancellationToken ct = default)
    {
        await EnsureDefaultWalletCurrencyAsync(websiteId, ct);
        // Ensure default wallet exists so UI always shows at least one account.
        await GetOrCreateAsync(websiteId, clientId, null, ct);

        return await _context.ClientWallets.AsNoTracking()
            .Include(w => w.CurrencyCodeNavigation)
            .Where(w => w.WebsiteID == websiteId && w.WebsiteClientID == clientId)
            .OrderBy(w => w.CurrencyCode)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetBalanceAsync(
        int websiteId, int clientId, string? currencyCode = null, CancellationToken ct = default)
    {
        var code = await ResolveCurrencyCodeAsync(websiteId, currencyCode, ct);
        var wallet = await _context.ClientWallets.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteClientID == clientId && w.CurrencyCode == code, ct);
        return wallet?.Balance ?? 0m;
    }

    public async Task<PagedResult<ClientWalletTransaction>> GetHistoryAsync(
        int websiteId, int clientId, GridQuery query, string? currencyCode = null, CancellationToken ct = default)
    {
        var code = await ResolveCurrencyCodeAsync(websiteId, currencyCode, ct);
        var q = _context.ClientWalletTransactions.AsNoTracking()
            .Where(t => t.ClientWallet.WebsiteClientID == clientId
                        && t.ClientWallet.CurrencyCode == code
                        && t.WebsiteID == websiteId);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(ClientWalletTransaction.ClientWalletTransactionID), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<ClientWalletTransaction> { Items = items, TotalCount = total };
    }

    public async Task<ClientWalletTransaction> ApplyAsync(
        int websiteId,
        int clientId,
        byte type,
        decimal signedAmount,
        byte? sourceType,
        int? sourceId,
        string? note,
        int? memberId,
        string? currencyCode = null,
        CancellationToken ct = default)
    {
        try
        {
            return await ApplyOnceAsync(websiteId, clientId, type, signedAmount, sourceType, sourceId, note, memberId, currencyCode, ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return await ApplyOnceAsync(websiteId, clientId, type, signedAmount, sourceType, sourceId, note, memberId, currencyCode, ct);
        }
    }

    private async Task<ClientWalletTransaction> ApplyOnceAsync(
        int websiteId,
        int clientId,
        byte type,
        decimal signedAmount,
        byte? sourceType,
        int? sourceId,
        string? note,
        int? memberId,
        string? currencyCode,
        CancellationToken ct)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var code = await ResolveCurrencyCodeAsync(websiteId, currencyCode, ct);
                var wallet = await GetOrCreateAsync(websiteId, clientId, code, ct);
                if (_context.Entry(wallet).State == EntityState.Detached)
                {
                    wallet = await _context.ClientWallets
                        .FirstAsync(w => w.WebsiteClientID == clientId && w.CurrencyCode == code, ct);
                }

                var balanceAfter = wallet.Balance + signedAmount;
                if (signedAmount < 0 && balanceAfter < 0)
                    throw new InvalidOperationException("Insufficient wallet balance.");

                // Optional USD mirror only for sites that dual-store product prices (reporting).
                decimal amountUsd = 0, balanceAfterUsd = 0;
                if (await _currency.GetStorePricesInUsdAsync(websiteId, ct))
                {
                    try
                    {
                        amountUsd = await _currency.ToUsdAsync(websiteId, signedAmount, code, ct);
                        balanceAfterUsd = await _currency.ToUsdAsync(websiteId, balanceAfter, code, ct);
                    }
                    catch (InvalidOperationException)
                    {
                        amountUsd = 0;
                        balanceAfterUsd = 0;
                    }
                }

                var transaction = new ClientWalletTransaction
                {
                    WebsiteID = websiteId,
                    ClientWalletID = wallet.ClientWalletID,
                    Type = type,
                    Amount = signedAmount,
                    AmountUsd = amountUsd,
                    BalanceAfter = balanceAfter,
                    BalanceAfterUsd = balanceAfterUsd,
                    SourceType = sourceType,
                    SourceId = sourceId,
                    Note = note,
                    CreatedByMemberID = memberId,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.ClientWalletTransactions.Add(transaction);

                wallet.Balance = balanceAfter;
                wallet.BalanceUsd = balanceAfterUsd;

                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return transaction;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    private async Task EnsureDefaultWalletCurrencyAsync(int websiteId, CancellationToken ct)
    {
        var website = await _context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct)
            ?? throw new InvalidOperationException("Website not found.");

        var code = NormalizeCode(website.DefaultCurrencyCode)
            ?? throw new InvalidOperationException("Website has no default currency.");

        var existing = await _context.WebsiteWalletCurrencies
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.CurrencyCode == code, ct);
        if (existing is not null)
        {
            if (!existing.IsDefault || !existing.IsActive)
            {
                existing.IsDefault = true;
                existing.IsActive = true;
                // Clear other defaults
                var others = await _context.WebsiteWalletCurrencies
                    .Where(c => c.WebsiteID == websiteId && c.CurrencyCode != code && c.IsDefault)
                    .ToListAsync(ct);
                foreach (var o in others) o.IsDefault = false;
                await _context.SaveChangesAsync(ct);
            }
            return;
        }

        // Clear accidental defaults then insert.
        var oldDefaults = await _context.WebsiteWalletCurrencies
            .Where(c => c.WebsiteID == websiteId && c.IsDefault)
            .ToListAsync(ct);
        foreach (var o in oldDefaults) o.IsDefault = false;

        _context.WebsiteWalletCurrencies.Add(new WebsiteWalletCurrency
        {
            WebsiteID = websiteId,
            CurrencyCode = code,
            IsDefault = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync(ct);
    }

    private async Task<string> ResolveCurrencyCodeAsync(int websiteId, string? currencyCode, CancellationToken ct)
    {
        await EnsureDefaultWalletCurrencyAsync(websiteId, ct);
        var code = NormalizeCode(currencyCode);
        if (code is not null)
            return code;

        var def = await _context.WebsiteWalletCurrencies.AsNoTracking()
            .Where(c => c.WebsiteID == websiteId && c.IsDefault)
            .Select(c => c.CurrencyCode)
            .FirstOrDefaultAsync(ct);
        if (!string.IsNullOrEmpty(def))
            return def;

        var website = await _context.Websites.AsNoTracking()
            .FirstAsync(w => w.WebsiteID == websiteId, ct);
        return website.DefaultCurrencyCode;
    }

    private Task<bool> IsCurrencyEnabledAsync(int websiteId, string code, CancellationToken ct) =>
        _context.WebsiteWalletCurrencies.AsNoTracking()
            .AnyAsync(c => c.WebsiteID == websiteId && c.CurrencyCode == code && c.IsActive, ct);

    private static string? NormalizeCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var c = code.Trim().ToUpperInvariant();
        return c.Length == 0 ? null : (c.Length > 3 ? c[..3] : c);
    }
}
