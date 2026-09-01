using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dotnetable.Infrastructure.Services;

public class ThemeService : IThemeService
{
    public const string BuiltinSlug = "Default";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private static readonly Regex SlugRegex = new(@"^[a-zA-Z0-9][a-zA-Z0-9_-]{0,63}$", RegexOptions.Compiled);

    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly string _themesRoot;

    public ThemeService(IDbContextFactory<AppDbContext> contextFactory, IConfiguration configuration, IHostEnvironment env)
    {
        _contextFactory = contextFactory;
        _themesRoot = ResolveThemesRoot(configuration, env.ContentRootPath);
    }

    /// <summary>Test-friendly constructor with an explicit themes root.</summary>
    public ThemeService(IDbContextFactory<AppDbContext> contextFactory, string themesRoot)
    {
        _contextFactory = contextFactory;
        _themesRoot = Path.GetFullPath(themesRoot);
    }

    private static string ResolveThemesRoot(IConfiguration configuration, string contentRoot)
    {
        var configured = configuration["Themes:RootPath"];
        if (string.IsNullOrWhiteSpace(configured))
            return Path.GetFullPath(Path.Combine(contentRoot, "..", "Dotnetable.Web", "Themes"));
        return Path.IsPathRooted(configured)
            ? Path.GetFullPath(configured)
            : Path.GetFullPath(Path.Combine(contentRoot, configured));
    }

    public string ThemesRoot => _themesRoot;

    public async Task<List<ThemePackageDto>> GetThemesAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var packages = await _context.WebsiteThemes.AsNoTracking()
            .Where(t => t.WebsiteID == websiteId)
            .OrderByDescending(t => t.IsActive)
            .ThenBy(t => t.Name)
            .ToListAsync(ct);

        var activeSlug = packages.FirstOrDefault(t => t.IsActive)?.Slug;
        var list = new List<ThemePackageDto>
        {
            BuiltinDto(websiteId, isActive: activeSlug is null),
        };

        foreach (var p in packages)
            list.Add(ToDto(p));

        // Prefer active package; if none active, Default is marked active above.
        return list;
    }

    public async Task<ThemePackageDto?> GetThemeAsync(int websiteId, string slug, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (IsBuiltin(slug))
        {
            var anyActive = await _context.WebsiteThemes.AsNoTracking()
                .AnyAsync(t => t.WebsiteID == websiteId && t.IsActive, ct);
            return BuiltinDto(websiteId, isActive: !anyActive);
        }

        var entity = await _context.WebsiteThemes.AsNoTracking()
            .FirstOrDefaultAsync(t => t.WebsiteID == websiteId && t.Slug == slug, ct);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<WebsiteTheme?> GetThemeEntityAsync(int themeId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        // AsNoTracking because the context closes with this method: a tracked entity handed back to a
        // caller that then edits it would have nowhere to save.
        return await _context.WebsiteThemes.AsNoTracking()
            .FirstOrDefaultAsync(t => t.WebsiteThemeID == themeId, ct);
    }

    public async Task ActivateThemeAsync(int websiteId, string slug, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        slug = NormalizeSlug(slug);

        var packages = await _context.WebsiteThemes
            .Where(t => t.WebsiteID == websiteId)
            .ToListAsync(ct);

        if (IsBuiltin(slug))
        {
            foreach (var p in packages)
                p.IsActive = false;
            await _context.SaveChangesAsync(ct);
            return;
        }

        var target = packages.FirstOrDefault(t => string.Equals(t.Slug, slug, StringComparison.OrdinalIgnoreCase));
        if (target is null)
            throw new InvalidOperationException($"Theme '{slug}' is not installed for this website.");

        foreach (var p in packages)
            p.IsActive = p.WebsiteThemeID == target.WebsiteThemeID;
        target.IsActive = true;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteThemeAsync(int websiteId, string slug, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        slug = NormalizeSlug(slug);
        if (IsBuiltin(slug))
            throw new InvalidOperationException("The built-in Default theme cannot be deleted.");

        var theme = await _context.WebsiteThemes
            .FirstOrDefaultAsync(t => t.WebsiteID == websiteId && t.Slug == slug, ct);
        if (theme is null) return;
        if (theme.IsActive)
            throw new InvalidOperationException("Deactivate the theme before deleting it.");

        var dir = GetPackageDirectory(websiteId, slug);
        if (Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);

        _context.WebsiteThemes.Remove(theme);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ThemePackageDto> InstallFromZipAsync(
        int websiteId, Stream zipStream, string? originalFileName = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        Directory.CreateDirectory(_themesRoot);

        var tempRoot = Path.Combine(Path.GetTempPath(), "dn-theme-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            ExtractZipSafely(zipStream, tempRoot);

            var packageRoot = FindPackageRoot(tempRoot);
            var manifest = ReadManifest(packageRoot);

            var slug = NormalizeSlug(
                !string.IsNullOrWhiteSpace(manifest.Slug) ? manifest.Slug!
                : !string.IsNullOrWhiteSpace(manifest.Name) ? Slugify(manifest.Name)
                : Path.GetFileNameWithoutExtension(originalFileName) is { Length: > 0 } fromFile
                    ? Slugify(fromFile)
                    : "theme");

            if (IsBuiltin(slug))
                throw new InvalidOperationException("Slug 'Default' is reserved for the built-in theme.");

            if (!SlugRegex.IsMatch(slug))
                throw new InvalidOperationException("Theme slug must be alphanumeric (with - or _), max 64 chars.");

            var name = string.IsNullOrWhiteSpace(manifest.Name) ? slug : manifest.Name.Trim();
            EnsureViewsFolder(packageRoot);

            var dest = GetPackageDirectory(websiteId, slug);
            if (Directory.Exists(dest))
                Directory.Delete(dest, recursive: true);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            CopyDirectory(packageRoot, dest);

            // Persist a normalized theme.json so exports are consistent.
            var normalized = new ThemeManifest
            {
                Name = name,
                Slug = slug,
                Version = manifest.Version,
                Author = manifest.Author,
                Description = manifest.Description,
            };
            await File.WriteAllTextAsync(
                Path.Combine(dest, "theme.json"),
                JsonSerializer.Serialize(normalized, JsonOpts),
                ct);

            var hasScreenshot = DetectScreenshot(dest) is not null;

            var entity = await _context.WebsiteThemes
                .FirstOrDefaultAsync(t => t.WebsiteID == websiteId && t.Slug == slug, ct);

            if (entity is null)
            {
                entity = new WebsiteTheme
                {
                    WebsiteID = websiteId,
                    Slug = slug,
                    Name = name,
                    Version = TrimOrNull(manifest.Version, 50),
                    Author = TrimOrNull(manifest.Author, 100),
                    Description = TrimOrNull(manifest.Description, 500),
                    HasScreenshot = hasScreenshot,
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.WebsiteThemes.Add(entity);
            }
            else
            {
                entity.Name = name;
                entity.Version = TrimOrNull(manifest.Version, 50);
                entity.Author = TrimOrNull(manifest.Author, 100);
                entity.Description = TrimOrNull(manifest.Description, 500);
                entity.HasScreenshot = hasScreenshot;
            }

            await _context.SaveChangesAsync(ct);
            return ToDto(entity);
        }
        finally
        {
            try { if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, recursive: true); }
            catch { /* best effort */ }
        }
    }

    public async Task<(byte[] Bytes, string FileName)> ExportZipAsync(
        int websiteId, string slug, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        slug = NormalizeSlug(slug);
        string sourceDir;
        string fileName;

        if (IsBuiltin(slug))
        {
            sourceDir = Path.Combine(_themesRoot, BuiltinSlug);
            if (!Directory.Exists(sourceDir))
                throw new InvalidOperationException("Built-in Default theme folder was not found.");
            fileName = "Default.zip";
        }
        else
        {
            var exists = await _context.WebsiteThemes.AsNoTracking()
                .AnyAsync(t => t.WebsiteID == websiteId && t.Slug == slug, ct);
            if (!exists)
                throw new InvalidOperationException($"Theme '{slug}' is not installed.");

            sourceDir = GetPackageDirectory(websiteId, slug);
            if (!Directory.Exists(sourceDir))
                throw new InvalidOperationException($"Theme package folder for '{slug}' is missing on disk.");
            fileName = $"{slug}.zip";
        }

        await using var ms = new MemoryStream();
        ZipFile.CreateFromDirectory(sourceDir, ms, CompressionLevel.Optimal, includeBaseDirectory: false);
        return (ms.ToArray(), fileName);
    }

    public async Task<ActiveThemeDto> GetActiveThemeAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var active = await _context.WebsiteThemes.AsNoTracking()
            .FirstOrDefaultAsync(t => t.WebsiteID == websiteId && t.IsActive, ct);

        if (active is null)
        {
            return new ActiveThemeDto
            {
                Slug = BuiltinSlug,
                Name = "Default",
                Version = null,
                ViewRoot = BuiltinSlug,
            };
        }

        return new ActiveThemeDto
        {
            Slug = active.Slug,
            Name = active.Name,
            Version = active.Version,
            ViewRoot = ViewRootFor(websiteId, active.Slug),
        };
    }

    public string? GetScreenshotPath(int websiteId, string slug)
    {
        slug = NormalizeSlug(slug);
        var dir = IsBuiltin(slug)
            ? Path.Combine(_themesRoot, BuiltinSlug)
            : GetPackageDirectory(websiteId, slug);
        return DetectScreenshot(dir);
    }

    // ── helpers ───────────────────────────────────────────────────────────

    public string GetPackageDirectory(int websiteId, string slug) =>
        Path.Combine(_themesRoot, websiteId.ToString(), slug);

    public static string ViewRootFor(int websiteId, string slug) =>
        IsBuiltin(slug) ? BuiltinSlug : $"{websiteId}/{slug}";

    private ThemePackageDto BuiltinDto(int websiteId, bool isActive)
    {
        var dir = Path.Combine(_themesRoot, BuiltinSlug);
        var manifest = Directory.Exists(dir) ? TryReadManifest(dir) : null;
        return new ThemePackageDto
        {
            WebsiteThemeID = null,
            WebsiteID = websiteId,
            Slug = BuiltinSlug,
            Name = manifest?.Name is { Length: > 0 } n ? n : "Default",
            Version = manifest?.Version,
            Author = manifest?.Author ?? "Dotnetable",
            Description = manifest?.Description ?? "Built-in storefront theme.",
            HasScreenshot = DetectScreenshot(dir) is not null,
            IsActive = isActive,
            IsBuiltin = true,
            CreatedAt = null,
            ViewRoot = BuiltinSlug,
        };
    }

    private static ThemePackageDto ToDto(WebsiteTheme t) => new()
    {
        WebsiteThemeID = t.WebsiteThemeID,
        WebsiteID = t.WebsiteID,
        Slug = t.Slug,
        Name = t.Name,
        Version = t.Version,
        Author = t.Author,
        Description = t.Description,
        HasScreenshot = t.HasScreenshot,
        IsActive = t.IsActive,
        IsBuiltin = false,
        CreatedAt = t.CreatedAt,
        ViewRoot = ViewRootFor(t.WebsiteID, t.Slug),
    };

    private static bool IsBuiltin(string slug) =>
        string.Equals(slug, BuiltinSlug, StringComparison.OrdinalIgnoreCase);

    private static string NormalizeSlug(string slug) => (slug ?? string.Empty).Trim();

    private static string Slugify(string value)
    {
        var s = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-");
        s = s.Trim('-');
        return string.IsNullOrEmpty(s) ? "theme" : s[..Math.Min(s.Length, 64)];
    }

    private static string? TrimOrNull(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim();
        return value.Length <= max ? value : value[..max];
    }

    private static string? DetectScreenshot(string dir)
    {
        if (!Directory.Exists(dir)) return null;
        foreach (var name in new[] { "screenshot.png", "screenshot.jpg", "screenshot.jpeg", "Screenshot.png" })
        {
            var path = Path.Combine(dir, name);
            if (File.Exists(path)) return path;
        }
        return null;
    }

    private static void EnsureViewsFolder(string packageRoot)
    {
        var views = Path.Combine(packageRoot, "Views");
        if (!Directory.Exists(views))
            throw new InvalidOperationException("Theme package must contain a Views/ folder.");
    }

    private static ThemeManifest ReadManifest(string packageRoot)
    {
        var path = Path.Combine(packageRoot, "theme.json");
        if (!File.Exists(path))
            return new ThemeManifest { Name = Path.GetFileName(packageRoot) };

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ThemeManifest>(json, JsonOpts) ?? new ThemeManifest();
    }

    private static ThemeManifest? TryReadManifest(string packageRoot)
    {
        try { return ReadManifest(packageRoot); }
        catch { return null; }
    }

    /// <summary>
    /// Zip may contain a single top-level folder (ocean/Views/...) or files at root (theme.json + Views/).
    /// </summary>
    private static string FindPackageRoot(string extractRoot)
    {
        if (File.Exists(Path.Combine(extractRoot, "theme.json")) ||
            Directory.Exists(Path.Combine(extractRoot, "Views")))
            return extractRoot;

        var dirs = Directory.GetDirectories(extractRoot);
        if (dirs.Length == 1 &&
            (File.Exists(Path.Combine(dirs[0], "theme.json")) ||
             Directory.Exists(Path.Combine(dirs[0], "Views"))))
            return dirs[0];

        // Fall back: first directory that has Views
        foreach (var d in dirs)
        {
            if (Directory.Exists(Path.Combine(d, "Views")))
                return d;
        }

        throw new InvalidOperationException(
            "Could not find a theme package root (expected theme.json and/or Views/).");
    }

    private static void ExtractZipSafely(Stream zipStream, string destDir)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
        var destFull = Path.GetFullPath(destDir) + Path.DirectorySeparatorChar;

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name) && entry.FullName.EndsWith('/'))
            {
                // directory entry
                var dirPath = Path.GetFullPath(Path.Combine(destDir, entry.FullName));
                if (!dirPath.StartsWith(destFull, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Zip entry path traversal blocked.");
                Directory.CreateDirectory(dirPath);
                continue;
            }

            var target = Path.GetFullPath(Path.Combine(destDir, entry.FullName));
            if (!target.StartsWith(destFull, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Zip entry path traversal blocked.");

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            entry.ExtractToFile(target, overwrite: true);
        }
    }

    private static void CopyDirectory(string source, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (var file in Directory.GetFiles(source))
            File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), overwrite: true);
        foreach (var dir in Directory.GetDirectories(source))
            CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
    }
}
