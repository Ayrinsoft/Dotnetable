using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Cash wallet for the signed-in website customer (<see cref="WebsiteClient"/>). The customer is
/// always taken from the bearer token's <see cref="ClientClaims.ClientId"/> claim — a caller can only
/// ever see or change their own wallet.
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

    [HttpGet]
    public async Task<IActionResult> GetBalance(CancellationToken ct = default)
    {
        var balance = await _wallet.GetBalanceAsync(CurrentClientId, ct);
        return Ok(new WalletBalanceDto { BalanceUsd = balance, IsActive = true });
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var query = new GridQuery { PageIndex = pageIndex, PageSize = pageSize };
        var result = await _wallet.GetHistoryAsync(CurrentClientId, query, ct);
        return Ok(new PagedResult<WalletTransactionDto>
        {
            Items = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
        });
    }

    [HttpPost("withdrawals")]
    public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequest request, CancellationToken ct = default)
    {
        if (request.AmountUsd <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        try
        {
            var withdrawal = await _withdrawals.RequestAsync(website.WebsiteID, CurrentClientId, request.ClientBankAccountId, request.AmountUsd, ct);
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

    // ── Helpers ─────────────────────────────────────────────────────

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");

    private static WalletTransactionDto ToDto(ClientWalletTransaction t) => new()
    {
        ClientWalletTransactionID = t.ClientWalletTransactionID,
        Type = t.Type,
        AmountUsd = t.AmountUsd,
        BalanceAfterUsd = t.BalanceAfterUsd,
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
        AmountUsd = w.AmountUsd,
        Status = w.Status,
        ReviewedByMemberID = w.ReviewedByMemberID,
        ReviewedAt = w.ReviewedAt,
        RejectReason = w.RejectReason,
        PaymentRefNumber = w.PaymentRefNumber,
        PaidAt = w.PaidAt,
        RequestedAt = w.RequestedAt,
    };
}
