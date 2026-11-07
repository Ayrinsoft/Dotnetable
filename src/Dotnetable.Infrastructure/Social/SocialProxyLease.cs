using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

/// <summary>Points the social HTTP clients at this website's worker until disposed.</summary>
public sealed class SocialProxyLease : IDisposable
{
    private readonly SocialProxyAmbient.State? _previous;
    private bool _disposed;

    private SocialProxyLease(SocialProxyAmbient.State? previous) => _previous = previous;

    public static async Task<SocialProxyLease> OpenAsync(AppDbContext context, int websiteId, CancellationToken ct = default)
    {
        var row = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => new { w.SocialProxyUrl, w.SocialProxyKey })
            .FirstOrDefaultAsync(ct);
        var previous = SocialProxyAmbient.Current;
        var baseUrl = SocialProxyAddress.NormalizeBase(row?.SocialProxyUrl);
        SocialProxyAmbient.Set(baseUrl is null
            ? null
            : new SocialProxyAmbient.State(baseUrl, string.IsNullOrWhiteSpace(row?.SocialProxyKey) ? null : row!.SocialProxyKey));
        return new SocialProxyLease(previous);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        SocialProxyAmbient.Set(_previous);
    }
}
