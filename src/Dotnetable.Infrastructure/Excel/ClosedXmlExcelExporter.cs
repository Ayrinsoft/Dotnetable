using ClosedXML.Excel;
using Dotnetable.Application.Interfaces;

namespace Dotnetable.Infrastructure.Excel;

/// <summary>
/// Ordinary admin workbooks: a bold header row and fitted columns.
/// Column fitting uses SixLabors.Fonts. If no font is available the file is still written.
/// </summary>
public sealed class ClosedXmlExcelExporter : IExcelExporter
{
    public string Key => "closedxml";

    public bool ForHeavyExports => false;

    public byte[] Write(string sheetName, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(ExcelSheetNames.Clean(sheetName));

        for (var c = 0; c < headers.Count; c++)
            sheet.Cell(1, c + 1).Value = headers[c];

        if (headers.Count > 0)
            sheet.Row(1).Style.Font.Bold = true;

        var r = 2;
        foreach (var row in rows)
        {
            for (var c = 0; c < row.Count; c++)
                sheet.Cell(r, c + 1).Value = row[c]?.ToString() ?? string.Empty;
            r++;
        }

        if (headers.Count > 0)
        {
            try
            {
                sheet.Columns(1, headers.Count).AdjustToContents();
            }
            catch (Exception)
            {
                // Font measurement is optional. A missing font must not fail the download.
            }
        }

        using var buffer = new MemoryStream();
        workbook.SaveAs(buffer);
        return buffer.ToArray();
    }

    public IReadOnlyList<string[]> Read(Stream stream)
    {
        var source = ExcelSheetNames.Rewind(stream, out var owned);
        try
        {
            using var workbook = new XLWorkbook(source);
            var sheet = workbook.Worksheets.FirstOrDefault()
                ?? throw new InvalidOperationException("Excel file has no worksheets.");
            var range = sheet.RangeUsed();
            if (range is null)
                return [];

            var width = range.ColumnCount();
            var rows = new List<string[]>(range.RowCount());
            foreach (var row in range.Rows())
            {
                var cells = new string[width];
                var any = false;
                for (var c = 0; c < width; c++)
                {
                    var text = row.Cell(c + 1).GetString().Trim();
                    cells[c] = text;
                    if (text.Length > 0)
                        any = true;
                }

                if (any)
                    rows.Add(cells);
            }

            return rows;
        }
        finally
        {
            owned?.Dispose();
        }
    }
}
