namespace Dotnetable.Application.DTOs;

/// <summary>Outcome of bulk price/stock Excel import for vendor listings.</summary>
public sealed record VendorListingImportResult(
    int Updated,
    int Unchanged,
    int Skipped,
    IReadOnlyList<string> Errors)
{
    public int TotalRows => Updated + Unchanged + Skipped;
    public bool HasErrors => Errors.Count > 0;
}
