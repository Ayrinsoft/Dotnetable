namespace Dotnetable.Application.Interfaces;

/// <summary>
/// One .xlsx engine. Callers depend on this contract, not on ClosedXML or MiniExcel.
/// Register another implementation and the registry will pick it up.
/// </summary>
public interface IExcelExporter
{
    /// <summary>Stable provider key, for example <c>closedxml</c> or <c>miniexcel</c>.</summary>
    string Key { get; }

    /// <summary>True for the streaming writer used when a sheet has many rows.</summary>
    bool ForHeavyExports { get; }

    byte[] Write(string sheetName, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows);

    /// <summary>
    /// First worksheet as trimmed strings. A fully blank row is omitted.
    /// Empty cells stay in the row so column indexes still match the header.
    /// </summary>
    IReadOnlyList<string[]> Read(Stream stream);
}

/// <summary>
/// Chooses an <see cref="IExcelExporter"/>. Large sheets use the heavy writer.
/// Imports always use the standard reader so sample files and exports stay interchangeable.
/// </summary>
public interface IExcelExporterRegistry
{
    /// <summary>Row count at which <see cref="Write"/> switches to the heavy exporter.</summary>
    int HeavyRowThreshold { get; }

    IExcelExporter Standard { get; }

    IExcelExporter Heavy { get; }

    /// <summary>The exporter <see cref="Write"/> would use for this many data rows.</summary>
    IExcelExporter Select(int rowCount);

    byte[] Write(string sheetName, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows);

    IReadOnlyList<string[]> Read(Stream stream);
}
