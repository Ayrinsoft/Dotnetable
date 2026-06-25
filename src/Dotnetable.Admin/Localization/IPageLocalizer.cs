namespace Dotnetable.Admin.Localization;

/// <summary>
/// Per-circuit helper that resolves UI strings for the signed-in admin's website and language.
/// Call <see cref="EnsureLoadedAsync"/> once in a page's <c>OnInitializedAsync</c>, then read strings
/// with the indexer: <c>L["members.edit.title", "Edit Member"]</c>. Unknown keys fall back to the
/// supplied default and self-register so they appear on the Translations page for localization.
/// </summary>
public interface IPageLocalizer
{
    int WebsiteId { get; }
    string LanguageCode { get; }

    /// <summary>Reads the website/language from the auth state and warms the translation cache. Idempotent.</summary>
    Task EnsureLoadedAsync(CancellationToken ct = default);

    /// <summary>Localized value for <paramref name="key"/>, falling back to the key itself.</summary>
    string this[string key] { get; }

    /// <summary>Localized value for <paramref name="key"/>, falling back to <paramref name="defaultValue"/>.</summary>
    string this[string key, string defaultValue] { get; }
}
