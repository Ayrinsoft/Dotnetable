namespace Dotnetable.Infrastructure.Excel;

internal static class ExcelSheetNames
{
    public static string Clean(string? name)
    {
        var raw = string.IsNullOrWhiteSpace(name) ? "Sheet1" : name.Trim();
        var chars = raw.Select(ch => ch is ':' or '\\' or '/' or '?' or '*' or '[' or ']' ? '_' : ch).ToArray();
        var text = new string(chars);
        if (text.Length > 31)
            text = text[..31];
        return text.Length == 0 ? "Sheet1" : text;
    }

    /// <summary>Rewinds a seekable stream, or copies a forward-only stream so the reader can scan it.</summary>
    public static Stream Rewind(Stream stream, out MemoryStream? owned)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
            owned = null;
            return stream;
        }

        owned = new MemoryStream();
        stream.CopyTo(owned);
        owned.Position = 0;
        return owned;
    }
}
