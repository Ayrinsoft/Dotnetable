using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.DTOs;

/// <summary>
/// Resolved shipping option for a destination + weight, with prepaid and/or COD prices in USD dual.
/// </summary>
public sealed class ShippingQuoteDto
{
    public ShippingMethod Method { get; init; } = null!;

    /// <summary>Zone-matched base rate in USD dual (before prepaid/COD floors), when a rate row matched.</summary>
    public decimal? ZoneRateUsd { get; init; }

    /// <summary>Prepaid (پیش‌کرایه) total in USD dual when the method supports prepaid; otherwise null.</summary>
    public decimal? PrepaidPriceUsd { get; init; }

    /// <summary>COD / postpay (پس‌کرایه) total in USD dual when the method supports COD; otherwise null.</summary>
    public decimal? CodPriceUsd { get; init; }

    /// <summary>
    /// Default charge used when the caller does not pick a payment mode:
    /// prefers prepaid, else COD. Always set when the quote is returned.
    /// </summary>
    public decimal PriceUsd => PrepaidPriceUsd ?? CodPriceUsd ?? 0m;
}
