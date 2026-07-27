namespace Dotnetable.Domain.Enums;

/// <summary>Values for <see cref="Entities.TaxRate.TaxKind"/>.</summary>
public enum TaxKind : byte
{
    /// <summary>Value-added tax / GST style rates.</summary>
    Vat = 0,

    /// <summary>Sales tax (typically destination-based, non-VAT).</summary>
    SalesTax = 1,

    /// <summary>Other statutory or fee-like tax line.</summary>
    Other = 2,
}
