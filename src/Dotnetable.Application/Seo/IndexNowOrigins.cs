namespace Dotnetable.Application.Seo;

/// <summary>
/// Turns <c>Websites.WebsiteAddress</c> into the public origin IndexNow must be told about.
/// Stored values are sometimes a bare host and sometimes a full URL.
/// </summary>
public static class IndexNowOrigins
{
    public static bool TryParse(string? websiteAddress, out string origin, out string host)
    {
        origin = string.Empty;
        host = string.Empty;

        var raw = websiteAddress?.Trim();
        if (string.IsNullOrEmpty(raw)) return false;
        if (!raw.Contains("://", StringComparison.Ordinal))
            raw = "https://" + raw;

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;
        if (string.IsNullOrEmpty(uri.IdnHost)) return false;

        host = uri.IsDefaultPort ? uri.IdnHost : $"{uri.IdnHost}:{uri.Port}";
        origin = $"{uri.Scheme}://{host}";
        return true;
    }
}
