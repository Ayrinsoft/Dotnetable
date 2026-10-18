using Dotnetable.Application.Interfaces;

namespace Dotnetable.Infrastructure.Excel;

/// <summary>
/// Routes a sheet to ClosedXML or MiniExcel. Imports stay on the standard reader.
/// </summary>
public sealed class ExcelExporterRegistry : IExcelExporterRegistry
{
    public const int DefaultHeavyRowThreshold = 2000;

    private readonly IReadOnlyList<IExcelExporter> _exporters;

    public ExcelExporterRegistry(IEnumerable<IExcelExporter> exporters)
    {
        _exporters = exporters.ToList();
        if (_exporters.Count == 0)
            throw new InvalidOperationException("At least one IExcelExporter is required.");
    }

    public int HeavyRowThreshold => DefaultHeavyRowThreshold;

    public IExcelExporter Standard =>
        _exporters.FirstOrDefault(exporter => !exporter.ForHeavyExports) ?? _exporters[0];

    public IExcelExporter Heavy =>
        _exporters.FirstOrDefault(exporter => exporter.ForHeavyExports) ?? Standard;

    public IExcelExporter Select(int rowCount) =>
        rowCount >= HeavyRowThreshold ? Heavy : Standard;

    public byte[] Write(string sheetName, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows)
    {
        var materialized = rows as IReadOnlyList<IReadOnlyList<object?>> ?? rows.ToList();
        return Select(materialized.Count).Write(sheetName, headers, materialized);
    }

    public IReadOnlyList<string[]> Read(Stream stream) => Standard.Read(stream);
}
