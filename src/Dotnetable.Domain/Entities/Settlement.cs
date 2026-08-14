using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Settlement
{
    public int SettlementID { get; set; }

    public int WebsiteID { get; set; }

    public byte TargetType { get; set; }

    public int? VendorID { get; set; }

    public int? TargetWebsiteID { get; set; }

    public int? SupplierID { get; set; }

    public DateOnly PeriodFrom { get; set; }

    public DateOnly PeriodTo { get; set; }

    /// <summary>Net amount excluding tax (when tax is broken out).</summary>
    public decimal NetAmount { get; set; }

    /// <summary>Tax amount on this settlement (VAT/sales tax counterparty reporting).</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Gross payable / receivable: NetAmount + TaxAmount (legacy rows may only fill this).</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Optional snapshot of rate fraction used when TaxAmount was computed.</summary>
    public decimal? TaxRateSnapshot { get; set; }

    /// <summary>Currency paid to the vendor (destination).</summary>
    public string CurrencyCode { get; set; } = null!;

    /// <summary>Site / order currency we spent to fund this settlement.</summary>
    public string? SourceCurrencyCode { get; set; }

    public decimal SourceNetAmount { get; set; }

    public decimal SourceTaxAmount { get; set; }

    public decimal SourceTotalAmount { get; set; }

    /// <summary>USD bridge amount used for FX (reporting).</summary>
    public decimal BridgeUsdAmount { get; set; }

    /// <summary>Source-currency units per 1 USD at settlement time.</summary>
    public decimal? ExchangeRateToUsd { get; set; }

    /// <summary>Destination-currency units per 1 USD at settlement time.</summary>
    public decimal? ExchangeRateUsdToSettle { get; set; }

    public byte Status { get; set; }

    public int? BankAccountID { get; set; }

    public string? PaymentRefNumber { get; set; }

    public string? Note { get; set; }

    public int? CreatedByMemberID { get; set; }

    public int? ApprovedByMemberID { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Member? ApprovedByMember { get; set; }

    public virtual BankAccount? BankAccount { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;

    public virtual Currency? SourceCurrency { get; set; }

    public virtual ICollection<SettlementItem> SettlementItems { get; set; } = new List<SettlementItem>();

    public virtual Supplier? Supplier { get; set; }

    public virtual Website? TargetWebsite { get; set; }

    public virtual Vendor? Vendor { get; set; }

    public virtual Website Website { get; set; } = null!;
}
