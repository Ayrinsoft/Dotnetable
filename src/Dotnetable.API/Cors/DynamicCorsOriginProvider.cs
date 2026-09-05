using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.API.Cors;

/// <summary>
/// The allowed CORS origin list, sourced from <c>Websites.WebsiteAddress</c> instead of a static
/// <c>Cors:AllowedOrigins</c> config entry — every storefront already registers its address there, so
/// there is nothing to duplicate (and forget to update) in config. Config-listed origins are still
/// honored on top, for callers that are not a Website row (e.g. a marketing site, a separate SPA host).
///
/// <para>Held as a plain in-memory snapshot swapped by <see cref="RefreshAsync"/>: the built-in CORS
/// middleware's <c>SetIsOriginAllowed</c> predicate is synchronous, so the check itself can never hit
/// the database — <see cref="CorsOriginRefreshService"/> keeps the snapshot warm instead.</para>
/// </summary>
public class DynamicCorsOriginProvider
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DynamicCorsOriginProvider> _logger;
    private readonly HashSet<string> _staticOrigins;

    private volatile HashSet<string> _websiteOrigins = new(StringComparer.OrdinalIgnoreCase);

    public DynamicCorsOriginProvider(
        IDbContextFactory<AppDbContext> contextFactory,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<DynamicCorsOriginProvider> logger)
    {
        _contextFactory = contextFactory;
        _environment = environment;
        _logger = logger;
        _staticOrigins = new HashSet<string>(
            configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [],
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>True once at least one refresh has completed, so callers can tell "genuinely no
    /// websites configured yet" apart from "hasn't loaded yet".</summary>
    public bool HasLoaded { get; private set; }

    public bool IsAllowed(string origin)
    {
        // Storefront pages are typically opened straight from the file system or a dev server during
        // local development; do not force every developer to seed a Website row first.
        if (_environment.IsDevelopment()) return true;

        return _staticOrigins.Contains(origin) || _websiteOrigins.Contains(origin);
    }

    /// <summary>Re-reads active websites' addresses from the database and swaps them into the live
    /// snapshot. Never throws — a failed refresh just keeps serving the previous snapshot.</summary>
    public async Task RefreshAsync(CancellationToken ct = default)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            var addresses = await context.Websites
                .Where(w => w.Active)
                .Select(w => w.WebsiteAddress)
                .ToListAsync(ct);

            var origins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var address in addresses)
            {
                var origin = NormalizeOrigin(address);
                if (origin is not null) origins.Add(origin);
            }

            _websiteOrigins = origins;
            HasLoaded = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not refresh allowed CORS origins from the database; keeping the previous list.");
        }
    }

    /// <summary>An Origin header is scheme + host + port only ("https://shop.example.com"), never a
    /// path — reduce a stored WebsiteAddress (which may include one) down to that.</summary>
    private static string? NormalizeOrigin(string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;

        var candidate = address.Trim();
        if (!candidate.Contains("://", StringComparison.Ordinal))
            candidate = "https://" + candidate;

        return Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
            ? uri.GetLeftPart(UriPartial.Authority)
            : null;
    }
}
