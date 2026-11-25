namespace Dotnetable.Domain.Entities;

/// <summary>One row of a VAT invoice, with the price and tax frozen on the row.</summary>
public partial class VatInvoiceLine
{
    public int VatInvoiceLineID { get; set; }

    public int VatInvoiceID { get; set; }

    public string Description { get; set; } = null!;

    public decimal Quantity { get; set; }

    public string? Unit { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal NetAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal PayableAmount { get; set; }

    public virtual VatInvoice VatInvoice { get; set; } = null!;
}
