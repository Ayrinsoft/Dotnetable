namespace Dotnetable.Web.Services;

/// <summary>
/// Anonymous id for a person browsing the storefront. Sent to the API as <c>X-Visitor-Id</c> so view
/// counts and the 24-hour comment lock follow the visitor instead of this server's address.
/// </summary>
public static class VisitorIdentity
{
    public const string CookieName = "dn_vid";

    public static string GetOrCreate(HttpContext context)
    {
        var existing = context.Request.Cookies[CookieName];
        if (IsValid(existing)) return existing!;

        var key = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(CookieName, key, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(400),
        });
        return key;
    }

    public static bool IsValid(string? value) =>
        value is { Length: >= 8 and <= 64 } && value.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '_' or '-');
}
