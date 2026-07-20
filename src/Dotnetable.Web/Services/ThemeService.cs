using System.Text.Json;
using Dotnetable.Application.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnetable.Web.Services;

/// <summary>
/// Resolves the active theme package for this Web host (per WebsiteKey) and exposes the view root
/// used by <see cref="Infrastructure.ThemeViewLocationExpander"/>.
/// </summary>
public interface IThemeService
{
    string ActiveTheme { get; }
    string GetViewPath(string viewName);
    string GetLayoutPath();
    IEnumerable<string> GetAvailableThemes();
    Task EnsureResolvedAsync(CancellationToken ct = default);
}

public class ThemeService : IThemeService
{
    private readonly IWebHostEnvironment _env;
    private readonly ApiClient _api;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _ttl;
    private string _activeTheme;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ThemeService(
        IWebHostEnvironment env,
        IConfiguration configuration,
        ApiClient api,
        IMemoryCache cache)
    {
        _env = env;
        _api = api;
        _cache = cache;
        _ttl = TimeSpan.FromSeconds(configuration.GetValue("Cache:WebTtlSeconds", 60));
        _activeTheme = configuration["Theme:Active"] ?? "Default";
    }

    public string ActiveTheme => _activeTheme;

    public string GetViewPath(string viewName) =>
        $"/Themes/{_activeTheme}/Views/{viewName}.cshtml";

    public string GetLayoutPath() =>
        $"/Themes/{_activeTheme}/Views/Shared/_Layout.cshtml";

    public IEnumerable<string> GetAvailableThemes()
    {
        var themesPath = Path.Combine(_env.ContentRootPath, "Themes");
        if (!Directory.Exists(themesPath)) return [];

        var result = new List<string>();
        // Built-in + any top-level folders
        foreach (var dir in Directory.GetDirectories(themesPath))
        {
            var name = Path.GetFileName(dir);
            if (name is null) continue;
            if (int.TryParse(name, out _))
            {
                // Per-website package folders: Themes/{websiteId}/{slug}
                foreach (var pkg in Directory.GetDirectories(dir))
                {
                    var slug = Path.GetFileName(pkg);
                    if (slug is not null)
                        result.Add($"{name}/{slug}");
                }
            }
            else
            {
                result.Add(name);
            }
        }
        return result;
    }

    public async Task EnsureResolvedAsync(CancellationToken ct = default)
    {
        var dto = await _cache.GetOrCreateAsync("web:active-theme", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _ttl;
            return await _api.GetActiveThemeAsync(ct);
        });

        if (dto is not null && !string.IsNullOrWhiteSpace(dto.ViewRoot))
        {
            var root = dto.ViewRoot.Replace('\\', '/').Trim('/');
            var full = Path.Combine(_env.ContentRootPath, "Themes", root.Replace('/', Path.DirectorySeparatorChar));
            if (Directory.Exists(full))
                _activeTheme = root;
            else if (string.Equals(root, "Default", StringComparison.OrdinalIgnoreCase) ||
                     Directory.Exists(Path.Combine(_env.ContentRootPath, "Themes", "Default")))
                _activeTheme = "Default";
        }
    }
}
