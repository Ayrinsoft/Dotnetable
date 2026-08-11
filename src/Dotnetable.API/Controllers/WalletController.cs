using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Cash wallets for the signed-in website customer. One ledger per enabled currency.
/// </summary>
[Authorize(Policy = RoleKeys.ClientProfile)]
public class WalletController : BaseController
{
    private readonly IClientWalletService _wallet;
    private readonly IClientWalletWithdrawalService _withdrawals;
    private readonly IWebsiteService _websiteService;

    public WalletController(IClientWalletService wallet, IClientWalletWithdrawalService withdrawals, IWebsiteService websiteService)
    {
        _wallet = wallet;
        _withdrawals = withdrawals;
        _websiteService = websiteService;
    }

    /// <summary>All balances (one row per currency wallet).</summary>
    [HttpGet]
    public async Task<IActionResult> GetBalances(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var wallets = await _wallet.ListForClientAsync(website.WebsiteID, CurrentClientId, ct);
        var enabled = await _wallet.GetEnabledWalletCurrenciesAsync(website.WebsiteID, ct);
        var defaultCode = enabled.FirstOrDefault(c => c.IsDefault)?.CurrencyCode ?? website.DefaultCurrencyCode;

        return Ok(wallets.Select(w => new WalletBalanceDto
        {
            CurrencyCode = w.CurrencyCode,
            Balance = w.Balance,
            BalanceUsd = w.Balance, // obsolete alias
            IsActive = w.IsActive,
            IsDefaultCurrency = string.Equals(w.CurrencyCode, defaultCode, StringComparison.OrdinalIgnoreCase),
        }).ToList());
    }

    /// <summary>Single currency balance (default when currency omitted).</summary>
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance([FromQuery] string? currencyCode = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var balance = await _wallet.GetBalanceAsync(website.WebsiteID, CurrentClientId, currencyCode, ct);
        var code = currencyCode ?? website.DefaultCurrencyCode;
        return Ok(new WalletBalanceDto
        {
            CurrencyCode = code,
            Balance = balance,
            BalanceUsd = balance,
            IsActive = true,
            IsDefaultCurrency = string.Equals(code, website.DefaultCurrencyCode, StringComparison.OrdinalIgnoreCase),
        });
    }

    [HttpGet("currencies")]
    public async Task<IActionResult> GetWalletCurrencies(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var rows = await _wallet.GetEnabledWalletCurrenciesAsync(website.WebsiteID, ct);
        return Ok(rows.Select(c => new WebsiteWalletCurrencyDto
        {
            CurrencyCode = c.CurrencyCode,
            CurrencyName = c.CurrencyCodeNavigation?.Name,
            IsDefault = c.IsDefault,
            IsActive = c.IsActive,
        }).ToList());
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? currencyCode = null,
        CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var query = new GridQuery { PageIndex = pageIndex, PageSize = pageSize };
        var result = await _wallet.GetHistoryAsync(website.WebsiteID, CurrentClientId, query, currencyCode, ct);
        var code = currencyCode ?? website.DefaultCurrencyCode;
        return Ok(new PagedResult<WalletTransactionDto>
        {
            Items = result.Items.Select(t => ToDto(t, code)).ToList(),
            TotalCount = result.TotalCount,
        });
    }

    [HttpPost("withdrawals")]
    public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequest request, CancellationToken ct = default)
    {
        var amount = request.Amount > 0 ? request.Amount : request.AmountUsd;
        if (amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        try
        {
            var withdrawal = await _withdrawals.RequestAsync(
                website.WebsiteID, CurrentClientId, request.ClientBankAccountId, amount, request.CurrencyCode, ct);
            return Ok(ToDto(withdrawal, null));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("withdrawals")]
    public async Task<IActionResult> GetWithdrawals([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var query = new GridQuery { PageIndex = pageIndex, PageSize = pageSize };
        var result = await _withdrawals.GetByClientIdAsync(CurrentClientId, query, ct);
        return Ok(new PagedResult<WithdrawalDto>
        {
            Items = result.Items.Select(w => ToDto(w, null)).ToList(),
            TotalCount = result.TotalCount,
        });
    }

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");

    private static WalletTransactionDto ToDto(ClientWalletTransaction t, string currencyCode) => new()
    {
        ClientWalletTransactionID = t.ClientWalletTransactionID,
        CurrencyCode = currencyCode,
        Type = t.Type,
        Amount = t.Amount,
        AmountUsd = t.Amount,
        BalanceAfter = t.BalanceAfter,
        BalanceAfterUsd = t.BalanceAfter,
        SourceType = t.SourceType,
        SourceId = t.SourceId,
        Note = t.Note,
        CreatedAt = t.CreatedAt,
    };

    private static WithdrawalDto ToDto(ClientWalletWithdrawal w, string? clientName) => new()
    {
        ClientWalletWithdrawalID = w.ClientWalletWithdrawalID,
        WebsiteID = w.WebsiteID,
        WebsiteClientID = w.WebsiteClientID,
        ClientName = clientName,
        ClientBankAccountID = w.ClientBankAccountID,
        CurrencyCode = w.CurrencyCode,
        Amount = w.Amount,
        AmountUsd = w.Amount,
        Status = w.Status,
        ReviewedByMemberID = w.ReviewedByMemberID,
        ReviewedAt = w.ReviewedAt,
        RejectReason = w.RejectReason,
        PaymentRefNumber = w.PaymentRefNumber,
        PaidAt = w.PaidAt,
        RequestedAt = w.RequestedAt,
    };
}
