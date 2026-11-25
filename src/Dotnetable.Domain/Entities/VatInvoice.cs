using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// A VAT invoice frozen at issue or receipt time. Later catalog and profile edits do not change it.
/// </summary>
public partial class VatInvoice
{
    public int VatInvoiceID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary><see cref="Enums.VatInvoiceDirection"/>.</summary>
    public byte Direction { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public DateTime InvoiceDate { get; set; }

    /// <summary>Set when the invoice was issued from a sales order. One issued invoice per order.</summary>
    public int? OrderID { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public string? OurName { get; set; }

    public string? OurNationalId { get; set; }

    public string? OurEconomicCode { get; set; }

    public string? OurVatNumber { get; set; }

    public string? OurRegistrationNumber { get; set; }

    public string? OurPhone { get; set; }

    public string? OurEmail { get; set; }

    public string? OurAddress { get; set; }

    public string? OtherName { get; set; }

    public string? OtherNationalId { get; set; }

    public string? OtherEconomicCode { get; set; }

    public string? OtherPhone { get; set; }

    public string? OtherEmail { get; set; }

    public string? OtherAddress { get; set; }

    public string? OtherPostalCode { get; set; }

    public decimal NetTotal { get; set; }

    public decimal DiscountTotal { get; set; }

    public decimal TaxTotal { get; set; }

    public decimal PayableTotal { get; set; }

    public bool PricesIncludeTax { get; set; }

    public string? Note { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual Order? Order { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual ICollection<VatInvoiceLine> Lines { get; set; } = new List<VatInvoiceLine>();
}
