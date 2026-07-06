using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Admin-managed visual themes. Each theme is a named bag of design tokens (JSON); exactly one can
/// be active per website. Front-ends (React SPA in serverless mode, and any theme-aware layout)
/// fetch the active theme at runtime and apply it as CSS custom properties — re-theming a deployed
/// site never needs a rebuild.
/// </summary>
public interface IThemeService
{
    Task<List<WebsiteTheme>> GetThemesAsync(int? websiteId, CancellationToken ct = default);
    Task<WebsiteTheme?> GetThemeAsync(int themeId, CancellationToken ct = default);
    Task<WebsiteTheme> CreateThemeAsync(WebsiteTheme theme, CancellationToken ct = default);
    Task UpdateThemeAsync(WebsiteTheme theme, CancellationToken ct = default);
    Task DeleteThemeAsync(int themeId, CancellationToken ct = default);

    /// <summary>Marks a theme active and deactivates every other theme of the same website.</summary>
    Task ActivateThemeAsync(int themeId, CancellationToken ct = default);

    /// <summary>The website's active theme, or null when none is configured (front-ends fall back
    /// to their built-in defaults).</summary>
    Task<WebsiteTheme?> GetActiveThemeAsync(int websiteId, CancellationToken ct = default);
}
