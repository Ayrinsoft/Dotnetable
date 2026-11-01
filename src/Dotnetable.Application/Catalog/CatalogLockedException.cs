namespace Dotnetable.Application.Catalog;

/// <summary>
/// Thrown when a website other than website 1 tries to add a product or change a product category
/// while website 1 is marked as a platform.
/// </summary>
public sealed class CatalogLockedException : InvalidOperationException
{
    public CatalogLockedException()
        : base("On a platform, only website 1 can add products or change product categories. Submit a catalog request.")
    {
    }
}
