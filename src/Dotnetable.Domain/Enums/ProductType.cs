namespace Dotnetable.Domain.Enums;

/// <summary>
/// Catalog fulfillment kind stored on <c>Product.ProductType</c>.
/// Physical goods need shipping; digital kinds do not.
/// </summary>
public enum ProductType : byte
{
    /// <summary>Physical goods that require shipping / freight.</summary>
    Physical = 0,

    /// <summary>Downloadable digital asset (PDF, zip, media file).</summary>
    DigitalDownload = 1,

    /// <summary>Textual code / license key delivered after purchase.</summary>
    DigitalCode = 2,

    /// <summary>Access via a service URL / endpoint after purchase.</summary>
    DigitalService = 3,
}
