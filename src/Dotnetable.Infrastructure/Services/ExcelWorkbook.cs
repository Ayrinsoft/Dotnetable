using OfficeOpenXml;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Thin EPPlus wrapper for admin Excel (.xlsx) sample/export/import.
/// Polyform Noncommercial license is set once for the process.
/// </summary>
internal static class ExcelWorkbook
{
    static ExcelWorkbook()
    {
        ExcelPackage.License.SetNonCommercialOrganization("Dotnetable");
    }

    public static byte[] Write(
        string sheetName,
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<object?>> rows)
    {
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add(string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName);

        for (var c = 0; c < headers.Count; c++)
            sheet.Cells[1, c + 1].Value = headers[c];

        if (headers.Count > 0)
        {
            using var headerRange = sheet.Cells[1, 1, 1, headers.Count];
            headerRange.Style.Font.Bold = true;
        }

        var r = 2;
        foreach (var row in rows)
        {
            for (var c = 0; c < row.Count; c++)
                sheet.Cells[r, c + 1].Value = row[c]?.ToString() ?? string.Empty;
            r++;
        }

        if (headers.Count > 0)
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }

    /// <summary>
    /// Reads the first worksheet as string rows (trimmed). Empty trailing cells are dropped per row
    /// only when the whole row is blank; otherwise missing cells become empty strings up to the
    /// sheet's used column count.
    /// </summary>
    public static List<string[]> Read(Stream stream)
    {
        if (stream.CanSeek)
            stream.Position = 0;

        using var package = new ExcelPackage(stream);
        var sheet = package.Workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("Excel file has no worksheets.");

        var dim = sheet.Dimension;
        if (dim is null)
            return [];

        var rows = new List<string[]>(dim.Rows);
        for (var r = dim.Start.Row; r <= dim.End.Row; r++)
        {
            var cells = new string[dim.Columns];
            var any = false;
            for (var c = 0; c < dim.Columns; c++)
            {
                var text = sheet.Cells[r, dim.Start.Column + c].Text?.Trim() ?? string.Empty;
                cells[c] = text;
                if (text.Length > 0) any = true;
            }

            if (!any) continue;
            rows.Add(cells);
        }

        return rows;
    }
}
