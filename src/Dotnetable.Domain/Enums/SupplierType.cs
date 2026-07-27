namespace Dotnetable.Domain.Enums;

/// <summary>Values for <see cref="Entities.Supplier.SupplierType"/>.</summary>
public enum SupplierType : byte
{
    /// <summary>External stock / goods supplier (manual purchase).</summary>
    External = 0,

    /// <summary>
    /// Linked marketplace source site: money is collected on the host and settled back to this site.
    /// Treated as a tax counterparty for B2B settlement reporting.
    /// </summary>
    LinkedWebsite = 1,

    /// <summary>Optional link to a catalog vendor record on this site.</summary>
    LinkedVendor = 2,
}
