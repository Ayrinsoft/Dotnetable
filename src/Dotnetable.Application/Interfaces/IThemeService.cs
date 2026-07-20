using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// WordPress-style website theme packages: install from zip, activate one per website, export zip.
/// Built-in "Default" always appears and needs no package row.
/// </summary>
public interface IThemeService
{
    Task<List<ThemePackageDto>> GetThemesAsync(int websiteId, CancellationToken ct = default);
    Task<ThemePackageDto?> GetThemeAsync(int websiteId, string slug, CancellationToken ct = default);
    Task<WebsiteTheme?> GetThemeEntityAsync(int themeId, CancellationToken ct = default);

    Task ActivateThemeAsync(int websiteId, string slug, CancellationToken ct = default);
    Task DeleteThemeAsync(int websiteId, string slug, CancellationToken ct = default);

    /// <summary>Install or replace a theme package from a zip stream.</summary>
    Task<ThemePackageDto> InstallFromZipAsync(int websiteId, Stream zipStream, string? originalFileName = null, CancellationToken ct = default);

    /// <summary>Export a package as zip bytes. Built-in Default is exported from the shared Default folder.</summary>
    Task<(byte[] Bytes, string FileName)> ExportZipAsync(int websiteId, string slug, CancellationToken ct = default);

    Task<ActiveThemeDto> GetActiveThemeAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Absolute path to a screenshot file, or null.</summary>
    string? GetScreenshotPath(int websiteId, string slug);

    /// <summary>Absolute filesystem root where theme packages are stored.</summary>
    string ThemesRoot { get; }
}
