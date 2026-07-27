using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Supplier
{
    public int SupplierID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Display / trade name.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Legal registered name for invoices and tax filings (when different from Name).</summary>
    public string? LegalName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? AddressLine { get; set; }

    public string? CityName { get; set; }

    public string? PostalCode { get; set; }

    public int? CountryID { get; set; }

    /// <summary>National company ID / TIN / شناسه ملی / کد ملی.</summary>
    public string? TaxIdentificationNumber { get; set; }

    /// <summary>Economic code (کد اقتصادی) where required by local law.</summary>
    public string? EconomicCode { get; set; }

    /// <summary>VAT / GST registration number.</summary>
    public string? VatNumber { get; set; }

    /// <summary>Company registration / commercial registry number.</summary>
    public string? RegistrationNumber { get; set; }

    public bool IsVatRegistered { get; set; }

    public string? BankName { get; set; }

    public string? BankIban { get; set; }

    public string? BankAccountNumber { get; set; }

    public string? DefaultCurrencyCode { get; set; }

    /// <summary><see cref="Enums.SupplierType"/>.</summary>
    public byte SupplierType { get; set; }

    /// <summary>When type is LinkedWebsite — the source site that supplies catalog / stock virtually.</summary>
    public int? LinkedWebsiteID { get; set; }

    /// <summary>When type is LinkedVendor — optional catalog vendor on this website.</summary>
    public int? LinkedVendorID { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Country? Country { get; set; }

    public virtual Currency? DefaultCurrencyCodeNavigation { get; set; }

    public virtual Website? LinkedWebsite { get; set; }

    public virtual Vendor? LinkedVendor { get; set; }

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual Website Website { get; set; } = null!;
}
