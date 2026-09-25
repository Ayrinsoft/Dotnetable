namespace Dotnetable.Admin.Auth;

/// <summary>
/// Where a successful sign-in may send the browser. Login, logout and the sign-out flag are not
/// safe targets: following them signs the member out again and posts the form back to the same
/// query string, which is the infinite /Login?signedOut=1 loop.
/// </summary>
public static class AuthReturnUrl
{
    public static bool IsSafe(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        if (!url.StartsWith('/') || url.StartsWith("//", StringComparison.Ordinal) || url.Contains('\\'))
            return false;

        var cut = url.IndexOfAny(['?', '#']);
        var path = cut >= 0 ? url[..cut] : url;
        if (IsAuthPath(path)) return false;

        if (cut >= 0 && url[cut] == '?')
        {
            var query = url[(cut + 1)..];
            var hash = query.IndexOf('#');
            if (hash >= 0) query = query[..hash];
            foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var key = Uri.UnescapeDataString(pair.Split('=', 2)[0]);
                if (IsSignOutKey(key)) return false;
            }
        }

        return true;
    }

    public static bool IsAuthPath(string path) =>
        path.Equals("/Login", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/Login/", StringComparison.OrdinalIgnoreCase)
        || path.Equals("/Logout", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/Logout/", StringComparison.OrdinalIgnoreCase)
        || path.Equals("/AccessDenied", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/AccessDenied/", StringComparison.OrdinalIgnoreCase)
        || path.Equals("/ForgotPassword", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/ForgotPassword/", StringComparison.OrdinalIgnoreCase)
        || path.Equals("/ResetPassword", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/ResetPassword/", StringComparison.OrdinalIgnoreCase);

    public static bool IsSignOutKey(string key) =>
        key.Equals("signedOut", StringComparison.OrdinalIgnoreCase)
        || key.Equals("signout", StringComparison.OrdinalIgnoreCase)
        || key.Equals("logout", StringComparison.OrdinalIgnoreCase);
}
