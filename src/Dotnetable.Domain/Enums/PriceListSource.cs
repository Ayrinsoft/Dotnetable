namespace Dotnetable.Domain.Enums;

/// <summary>Where a published price list gets its rows.</summary>
public enum PriceListSource : byte
{
    /// <summary>Operator-authored rows (title, spec, unit, price). No catalog products required — e.g. a steel rate list.</summary>
    Custom = 0,

    /// <summary>Live rows from this website's published catalog products.</summary>
    CatalogProducts = 1,
}
