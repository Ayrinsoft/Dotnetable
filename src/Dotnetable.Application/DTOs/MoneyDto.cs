namespace Dotnetable.Application.DTOs;

/// <summary>
/// A monetary amount expressed both in the canonical USD storage value and in the currency it should
/// be displayed in. Every price-bearing DTO (product listing, cart line, order total, wallet balance,
/// payment amount) should embed one of these instead of a bare decimal, so callers never have to guess
/// which currency a number is in.
/// </summary>
public sealed class MoneyDto
{
    public decimal Amount { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public decimal AmountUsd { get; set; }
}
