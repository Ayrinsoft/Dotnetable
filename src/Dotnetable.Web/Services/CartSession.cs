namespace Dotnetable.Web.Services;

/// <summary>
/// Identifies a guest shopping cart across requests via a random key stored in a long-lived cookie.
/// Sent to the API as the <c>X-Cart-Session</c> header (see <c>CartController.SessionKey</c> on the API).
/// </summary>
public static class CartSession
{
    public const string CookieName = "dn_cart";

    /// <summary>Reads the existing session key from the request, or mints and persists a new one.</summary>
    public static string GetOrCreate(HttpContext context)
    {
        var existing = context.Request.Cookies[CookieName];
        if (!string.IsNullOrWhiteSpace(existing)) return existing;

        var key = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(CookieName, key, new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
        });
        return key;
    }
}
