using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Saved bank accounts for the signed-in website customer (<see cref="WebsiteClient"/>). The customer
/// is always taken from the bearer token's <see cref="ClientClaims.ClientId"/> claim — a caller can only
/// ever see or change their own bank accounts. Each customer may hold up to
/// <see cref="Dotnetable.Application.AppConstants.MaxClientBankAccounts"/> bank accounts.
/// </summary>
[Authorize(Policy = RoleKeys.ClientProfile)]
public class ClientBankAccountsController : BaseController
{
    private readonly IClientBankAccountService _accounts;

    public ClientBankAccountsController(IClientBankAccountService accounts) => _accounts = accounts;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var clientId = CurrentClientId;
        var items = await _accounts.GetByClientIdAsync(clientId, ct);
        return Ok(items.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var account = await _accounts.GetByIdAsync(id, CurrentClientId, ct);
        return account is null ? NotFound() : Ok(ToDto(account));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClientBankAccountRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerName))
            return BadRequest(new { message = "Owner name is required." });

        var account = new ClientBankAccount
        {
            WebsiteID = CurrentWebsiteId,
            WebsiteClientID = CurrentClientId,
            BankID = request.BankID,
            OwnerName = request.OwnerName,
            AccountNumber = request.AccountNumber,
            IBAN = request.IBAN,
            CardNumber = request.CardNumber,
            IsDefault = request.IsDefault,
        };

        var result = await _accounts.CreateAsync(account, ct);
        return result switch
        {
            BankAccountSaveResult.LimitReached => Conflict(new
            {
                message = $"You can save up to {Application.AppConstants.MaxClientBankAccounts} bank accounts.",
            }),
            _ => CreatedAtAction(nameof(GetById), new { id = account.ClientBankAccountID },
                ToDto(account)),
        };
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ClientBankAccountRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerName))
            return BadRequest(new { message = "Owner name is required." });

        var account = new ClientBankAccount
        {
            ClientBankAccountID = id,
            WebsiteID = CurrentWebsiteId,
            WebsiteClientID = CurrentClientId,
            BankID = request.BankID,
            OwnerName = request.OwnerName,
            AccountNumber = request.AccountNumber,
            IBAN = request.IBAN,
            CardNumber = request.CardNumber,
            IsDefault = request.IsDefault,
        };

        var result = await _accounts.UpdateAsync(account, ct);
        return result == BankAccountSaveResult.NotFound ? NotFound() : Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default) =>
        await _accounts.DeleteAsync(id, CurrentClientId, ct) ? Ok() : NotFound();

    [HttpPost("{id:int}/default")]
    public async Task<IActionResult> SetDefault(int id, CancellationToken ct = default) =>
        await _accounts.SetDefaultAsync(id, CurrentClientId, ct) ? Ok() : NotFound();

    // ── Helpers ─────────────────────────────────────────────────────

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");

    private static ClientBankAccountDto ToDto(ClientBankAccount a) => new()
    {
        ClientBankAccountID = a.ClientBankAccountID,
        BankID = a.BankID,
        BankName = a.Bank?.Name,
        OwnerName = a.OwnerName,
        AccountNumber = a.AccountNumber,
        IBAN = a.IBAN,
        CardNumber = a.CardNumber,
        IsDefault = a.IsDefault,
    };
}
