namespace Dotnetable.Application.DTOs;

/// <summary>Outcome of importing a translation CSV for one language.</summary>
public sealed record LocalizationImportResult(
    int Added,
    int Updated,
    int Unchanged,
    int Skipped,
    IReadOnlyList<string> Errors)
{
    public int TotalRows => Added + Updated + Unchanged + Skipped;
    public bool HasErrors => Errors.Count > 0;
}
