using System.Security.Cryptography;
using System.Text;

namespace Dotnetable.Infrastructure.Social;

public sealed record XCredentials(string ApiKey, string ApiSecret, string AccessToken, string AccessTokenSecret);

/// <summary>OAuth 1.0a header for the X user-context API. Multipart and JSON bodies are not part of the signature.</summary>
internal static class XOAuth1
{
    public static string Header(string method, string url, XCredentials credentials, IEnumerable<KeyValuePair<string, string>>? query = null)
    {
        var oauth = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["oauth_consumer_key"] = credentials.ApiKey,
            ["oauth_nonce"] = Guid.NewGuid().ToString("N"),
            ["oauth_signature_method"] = "HMAC-SHA1",
            ["oauth_timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["oauth_token"] = credentials.AccessToken,
            ["oauth_version"] = "1.0",
        };

        var signing = new SortedDictionary<string, string>(oauth, StringComparer.Ordinal);
        if (query is not null)
        {
            foreach (var pair in query)
                signing[pair.Key] = pair.Value;
        }

        var encoded = string.Join("&", signing.Select(pair => Encode(pair.Key) + "=" + Encode(pair.Value)));
        var baseString = method.ToUpperInvariant() + "&" + Encode(url) + "&" + Encode(encoded);
        var key = Encode(credentials.ApiSecret) + "&" + Encode(credentials.AccessTokenSecret);
        using var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(key));
        oauth["oauth_signature"] = Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(baseString)));
        return "OAuth " + string.Join(", ", oauth.Select(pair => Encode(pair.Key) + "=\"" + Encode(pair.Value) + "\""));
    }

    private static string Encode(string value) => Uri.EscapeDataString(value)
        .Replace("!", "%21", StringComparison.Ordinal)
        .Replace("*", "%2A", StringComparison.Ordinal)
        .Replace("'", "%27", StringComparison.Ordinal)
        .Replace("(", "%28", StringComparison.Ordinal)
        .Replace(")", "%29", StringComparison.Ordinal);
}
