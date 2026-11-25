namespace Dotnetable.Web.Services;

public static class BrandStyle
{
    /// <summary>A CSS url() value, or null when the address is missing or not http(s).</summary>
    public static string? CssImage(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;
        if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            && !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return null;

        var safe = url.Replace("\\", "", StringComparison.Ordinal)
            .Replace("'", "", StringComparison.Ordinal)
            .Replace("\"", "", StringComparison.Ordinal)
            .Replace("(", "", StringComparison.Ordinal)
            .Replace(")", "", StringComparison.Ordinal)
            .Replace(";", "", StringComparison.Ordinal);
        return $"url('{safe}')";
    }
}
