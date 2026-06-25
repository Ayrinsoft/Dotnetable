using System.Text;

namespace Dotnetable.Infrastructure.Services;

/// <summary>Minimal RFC 4180 CSV reader/writer — no external dependency. Handles
/// quoted fields, escaped quotes ("") and newlines embedded inside quotes.</summary>
internal static class Csv
{
    /// <summary>Quotes a field only when it contains a comma, quote or line break.</summary>
    public static string Escape(string field)
    {
        if (field.IndexOfAny([',', '"', '\r', '\n']) < 0)
            return field;
        return "\"" + field.Replace("\"", "\"\"") + "\"";
    }

    /// <summary>Parses CSV text into rows of fields. A leading UTF-8 BOM is ignored.</summary>
    public static List<string[]> Parse(string text)
    {
        var rows = new List<string[]>();
        var record = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (i == 0 && c == '﻿') continue; // strip BOM

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(c);
                continue;
            }

            switch (c)
            {
                case '"': inQuotes = true; break;
                case ',': record.Add(field.ToString()); field.Clear(); break;
                case '\r': break;
                case '\n':
                    record.Add(field.ToString()); field.Clear();
                    rows.Add(record.ToArray()); record = new();
                    break;
                default: field.Append(c); break;
            }
        }

        if (field.Length > 0 || record.Count > 0)
        {
            record.Add(field.ToString());
            rows.Add(record.ToArray());
        }

        return rows;
    }
}
