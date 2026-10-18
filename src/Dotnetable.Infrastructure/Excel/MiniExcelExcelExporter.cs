using Dotnetable.Application.Interfaces;
using MiniExcelLibs;

namespace Dotnetable.Infrastructure.Excel;

/// <summary>
/// Streaming writer for large sheets. It skips column fitting, which is the slow part of ClosedXML.
/// </summary>
public sealed class MiniExcelExcelExporter : IExcelExporter
{
    public string Key => "miniexcel";

    public bool ForHeavyExports => true;

    public byte[] Write(string sheetName, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows)
    {
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var keys = new string[Math.Max(headers.Count, 1)];
        for (var c = 0; c < keys.Length; c++)
            keys[c] = UniqueHeader(headers, c, used);

        var table = new List<Dictionary<string, object?>>();
        foreach (var row in rows)
        {
            var dict = new Dictionary<string, object?>(keys.Length);
            for (var c = 0; c < keys.Length; c++)
                dict[keys[c]] = c < row.Count ? row[c]?.ToString() ?? string.Empty : string.Empty;
            table.Add(dict);
        }

        if (table.Count == 0)
        {
            var blank = new Dictionary<string, object?>(keys.Length);
            foreach (var key in keys)
                blank[key] = string.Empty;
            table.Add(blank);
        }

        using var buffer = new MemoryStream();
        buffer.SaveAs(table, sheetName: ExcelSheetNames.Clean(sheetName));
        return buffer.ToArray();
    }

    public IReadOnlyList<string[]> Read(Stream stream)
    {
        var source = ExcelSheetNames.Rewind(stream, out var owned);
        try
        {
            var raw = source.Query(useHeaderRow: false).ToList();
            var parsed = new List<string[]>(raw.Count);
            var width = 0;
            foreach (var row in raw)
            {
                if (row is not IDictionary<string, object> map)
                    continue;
                var cells = map.Values.Select(value => value?.ToString()?.Trim() ?? string.Empty).ToArray();
                if (cells.Length > width)
                    width = cells.Length;
                parsed.Add(cells);
            }

            var rows = new List<string[]>(parsed.Count);
            foreach (var cells in parsed)
            {
                if (cells.Length < width)
                {
                    var padded = new string[width];
                    Array.Copy(cells, padded, cells.Length);
                    for (var i = cells.Length; i < width; i++)
                        padded[i] = string.Empty;
                    if (padded.Any(cell => cell.Length > 0))
                        rows.Add(padded);
                }
                else if (cells.Any(cell => cell.Length > 0))
                {
                    rows.Add(cells);
                }
            }

            return rows;
        }
        finally
        {
            owned?.Dispose();
        }
    }

    private static string UniqueHeader(IReadOnlyList<string> headers, int index, HashSet<string> used)
    {
        var baseName = index < headers.Count && !string.IsNullOrWhiteSpace(headers[index])
            ? headers[index].Trim()
            : $"Column{index + 1}";
        var name = baseName;
        var n = 2;
        while (!used.Add(name))
            name = $"{baseName} ({n++})";
        return name;
    }
}
