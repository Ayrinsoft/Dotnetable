using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Saved addresses for the signed-in website customer (<see cref="WebsiteClient"/>). The customer is
/// always taken from the bearer token's <see cref="ClientClaims.ClientId"/> claim — a caller can only
/// ever see or change their own addresses. Each customer may hold up to
/// <see cref="Dotnetable.Application.AppConstants.MaxClientAddresses"/> addresses.
/// </summary>
[Authorize(Policy = RoleKeys.ClientProfile)]
public class AddressesController : BaseController
{
    private readonly IWebsiteClientAddressService _addresses;

    public AddressesController(IWebsiteClientAddressService addresses) => _addresses = addresses;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var clientId = CurrentClientId;
        var items = await _addresses.GetByClientIdAsync(clientId, ct);
        return Ok(items.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var address = await _addresses.GetByIdAsync(id, CurrentClientId, ct);
        return address is null ? NotFound() : Ok(ToDto(address));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddressRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.AddressLine))
            return BadRequest(new { message = "Address line is required." });

        var address = new WebsiteClientAddress
        {
            WebsiteClientID = CurrentClientId,
            Title = request.Title,
            ReceiverName = request.ReceiverName,
            CountryId = request.CountryId,
            CityId = request.CityId,
            AddressLine = request.AddressLine,
            PostalCode = request.PostalCode,
            Phone = request.Phone,
            IsDefault = request.IsDefault,
        };

        var result = await _addresses.CreateAsync(address, ct);
        return result switch
        {
            AddressSaveResult.LimitReached => Conflict(new
            {
                message = $"You can save up to {Application.AppConstants.MaxClientAddresses} addresses.",
            }),
            _ => CreatedAtAction(nameof(GetById), new { id = address.WebsiteClientAddressID },
                ToDto(address)),
        };
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AddressRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.AddressLine))
            return BadRequest(new { message = "Address line is required." });

        var address = new WebsiteClientAddress
        {
            WebsiteClientAddressID = id,
            WebsiteClientID = CurrentClientId,
            Title = request.Title,
            ReceiverName = request.ReceiverName,
            CountryId = request.CountryId,
            CityId = request.CityId,
            AddressLine = request.AddressLine,
            PostalCode = request.PostalCode,
            Phone = request.Phone,
            IsDefault = request.IsDefault,
        };

        var result = await _addresses.UpdateAsync(address, ct);
        return result == AddressSaveResult.NotFound ? NotFound() : Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default) =>
        await _addresses.DeleteAsync(id, CurrentClientId, ct) ? Ok() : NotFound();

    [HttpPost("{id:int}/default")]
    public async Task<IActionResult> SetDefault(int id, CancellationToken ct = default) =>
        await _addresses.SetDefaultAsync(id, CurrentClientId, ct) ? Ok() : NotFound();

    // ── Helpers ─────────────────────────────────────────────────────

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");

    private static AddressDto ToDto(WebsiteClientAddress a) => new()
    {
        WebsiteClientAddressID = a.WebsiteClientAddressID,
        Title = a.Title,
        ReceiverName = a.ReceiverName,
        CountryId = a.CountryId,
        CountryName = a.Country?.Title,
        CityId = a.CityId,
        CityName = a.City?.Title,
        AddressLine = a.AddressLine,
        PostalCode = a.PostalCode,
        Phone = a.Phone,
        IsDefault = a.IsDefault,
    };
}
