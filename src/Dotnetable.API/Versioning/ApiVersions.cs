using Asp.Versioning;

namespace Dotnetable.API.Versioning;

/// <summary>
/// Storefront API versions. The version is selected by the <see cref="HeaderName"/> request header
/// (never by the URL). Existing clients that omit the header stay on <see cref="V1"/>.
/// </summary>
public static class ApiVersions
{
    /// <summary>Request header that carries the caller's API version (e.g. <c>1.0</c>).</summary>
    public const string HeaderName = "X-Api-Version";

    public const string V1String = "1.0";

    /// <summary>Current public contract. Additive (non-breaking) changes stay on this version.</summary>
    public static ApiVersion V1 { get; } = new(1, 0);
}
