namespace Dotnetable.Domain.Enums;

/// <summary>Sales invoices are issued. Purchase invoices are received from a supplier.</summary>
public enum VatInvoiceDirection : byte
{
    Issued = 1,
    Received = 2,
}
