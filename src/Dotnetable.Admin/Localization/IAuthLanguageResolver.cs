namespace Dotnetable.Admin.Localization;

/// <summary>Resolves the active UI language for a request: the "dn-lang" cookie when present,
/// otherwise the given website's default language, otherwise the master catalog's first entry.</summary>
public interface IAuthLanguageResolver
{
    Task<string> ResolveAsync(string? cookieValue, int websiteId, CancellationToken ct = default);
}
