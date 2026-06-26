namespace Dotnetable.Web.Services;

/// <summary>Shared constants for the customer (client) session on the public website.</summary>
public static class ClientAuth
{
    /// <summary>Name of the HttpOnly cookie holding the API JWT access token.</summary>
    public const string TokenCookie = "dn_token";
}
