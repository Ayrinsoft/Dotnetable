namespace Dotnetable.Application.DTOs;

/// <summary>
/// A flight or hotel hold recorded for the signed-in website customer.
/// The sale price, cost, tax, and description are stored on the order line.
/// </summary>
public sealed class TravelOrderWrite
{
    /// <summary><c>flight</c> or <c>hotel</c>.</summary>
    public string? Kind { get; set; }

    /// <summary><c>domestic</c> or <c>foreign</c>. Selects the hidden service product.</summary>
    public string? Scope { get; set; }

    public string? Title { get; set; }

    /// <summary>Invoice line text: route or hotel, date, and passengers.</summary>
    public string? Description { get; set; }

    /// <summary>Amount the buyer pays, in <see cref="CurrencyCode"/> when that rate exists.</summary>
    public decimal Amount { get; set; }

    /// <summary>Supplier cost in the same currency. Falls back to <see cref="Amount"/> when omitted.</summary>
    public decimal? Cost { get; set; }

    /// <summary>Tax already known for this sale. When omitted, the website tax rates are applied to <see cref="Amount"/>.</summary>
    public decimal? Tax { get; set; }

    public string? CurrencyCode { get; set; }

    /// <summary>Provider order or reserve code.</summary>
    public string? Reference { get; set; }

    /// <summary>Buyer national identifier. Stored without a check digit.</summary>
    public string? NationalId { get; set; }

    public string? Note { get; set; }
}
