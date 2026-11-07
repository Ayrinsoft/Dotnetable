using System.Net.Http;
using System.Text;
using Dotnetable.Application.Instagram;

namespace Dotnetable.Infrastructure.Social;

/// <summary>
/// Turns an official API URL into <c>{proxy}/proxy/b64/{base64url}</c>.
/// One path segment avoids <c>?</c> and <c>//</c> being parsed as a query or a new host.
/// </summary>
public static class SocialProxyAddress
{
    public const string KeyHeader = "X-Proxy-Key";

    public static string? NormalizeBase(string? value)
    {
        var text = (value ?? "").Trim();
        if (text.Length == 0) return null;
        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(uri.Host))
            throw new SocialPublishException("The proxy address must be an https URL, such as https://worker-blue-v2.xapi.workers.dev/");

        return uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }

    public static Uri Wrap(string baseUrl, Uri target)
    {
        var bytes = Encoding.UTF8.GetBytes(target.AbsoluteUri);
        var b64 = Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return new Uri(baseUrl.TrimEnd('/') + "/proxy/b64/" + b64);
    }

    /// <summary>
    /// Rewrites this request before it is sent. The handler may run where the ambient value is empty,
    /// so each social client calls this while the lease is still visible.
    /// </summary>
    public static void Apply(HttpRequestMessage request)
    {
        var scope = SocialProxyAmbient.Current;
        if (scope is null || request.RequestUri is not { IsAbsoluteUri: true } uri) return;
        if (string.Equals(uri.Host, new Uri(scope.BaseUrl).Host, StringComparison.OrdinalIgnoreCase)) return;
        var wrapped = Wrap(scope.BaseUrl, uri);
        var key = (scope.Key ?? "").Trim();
        if (key.Length > 0)
        {
            wrapped = new Uri(wrapped.AbsoluteUri + "?secret=" + Uri.EscapeDataString(key));
            request.Headers.TryAddWithoutValidation(KeyHeader, key);
        }

        request.RequestUri = wrapped;
        request.Version = new Version(1, 1);
        request.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        request.Headers.ExpectContinue = false;
    }
}

/// <summary>Active proxy for the website whose publish or account check is running.</summary>
public static class SocialProxyAmbient
{
    private static readonly AsyncLocal<State?> Slot = new();

    internal static State? Current => Slot.Value;

    internal static void Set(State? state) => Slot.Value = state;

    internal sealed record State(string BaseUrl, string? Key);
}
