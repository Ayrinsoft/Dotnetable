namespace Dotnetable.Application.DTOs;

/// <summary>Outcome of importing location reference data (countries, states, or cities).</summary>
public sealed record LocationImportResult(
    int Added,
    int Skipped,
    IReadOnlyList<string> Errors)
{
    public int TotalRows => Added + Skipped;
    public bool HasErrors => Errors.Count > 0;
}
