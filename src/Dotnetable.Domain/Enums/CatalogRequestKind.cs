namespace Dotnetable.Domain.Enums;

/// <summary>What a non-owner website is asking website 1 to add to the shared catalog.</summary>
public enum CatalogRequestKind : byte
{
    /// <summary>A product that should use a category that already exists.</summary>
    Product = 0,

    /// <summary>A new category and a product that belongs in it.</summary>
    CategoryAndProduct = 1,
}
