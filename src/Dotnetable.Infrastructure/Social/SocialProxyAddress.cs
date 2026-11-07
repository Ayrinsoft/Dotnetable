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
}

/// <summary>Active proxy for the website whose publish or account check is running.</summary>
public static class SocialProxyAmbient
{
    private static readonly AsyncLocal<State?> Slot = new();

    internal static State? Current => Slot.Value;

    internal static void Set(State? state) => Slot.Value = state;

    internal sealed record State(string BaseUrl, string? Key);
}
