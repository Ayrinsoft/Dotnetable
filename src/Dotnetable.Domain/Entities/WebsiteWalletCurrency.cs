namespace Dotnetable.Domain.Entities;

/// <summary>
/// Currencies the website allows for customer wallet accounts.
/// The site default currency is always enabled (IsDefault=true). Extra rows let admins open
/// separate wallet ledgers (e.g. IRR primary + optional USD account) without dual-storing balances.
/// </summary>
public partial class WebsiteWalletCurrency
{
    public int WebsiteWalletCurrencyID { get; set; }

    public int WebsiteID { get; set; }

    public string CurrencyCode { get; set; } = null!;

    /// <summary>True for the website's operational default currency (exactly one per site).</summary>
    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
