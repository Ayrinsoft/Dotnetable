using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

/// <summary>Worker address loaded for one website. <see cref="SocialProxyLease.Open"/> applies it.</summary>
public readonly record struct SocialProxySnapshot(string? BaseUrl, string? Key);

/// <summary>Points the social HTTP clients at this website's worker until disposed.</summary>
public sealed class SocialProxyLease : IDisposable
{
    private readonly SocialProxyAmbient.State? _previous;
    private bool _disposed;

    private SocialProxyLease(SocialProxyAmbient.State? previous) => _previous = previous;

    public static async Task<SocialProxySnapshot> ReadAsync(AppDbContext context, int websiteId, CancellationToken ct = default)
    {
        var row = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => new { w.SocialProxyUrl, w.SocialProxyKey })
            .FirstOrDefaultAsync(ct);
        return new SocialProxySnapshot(row?.SocialProxyUrl, row?.SocialProxyKey);
    }

    /// <summary>
    /// Call this in the same method that sends the HTTP request, after the await that loaded the snapshot.
    /// A value set inside an async method does not flow back to the caller, so the handler would skip the worker.
    /// </summary>
    public static SocialProxyLease Open(SocialProxySnapshot snapshot)
    {
        var previous = SocialProxyAmbient.Current;
        var baseUrl = SocialProxyAddress.NormalizeBase(snapshot.BaseUrl);
        SocialProxyAmbient.Set(baseUrl is null
            ? null
            : new SocialProxyAmbient.State(baseUrl, string.IsNullOrWhiteSpace(snapshot.Key) ? null : snapshot.Key));
        return new SocialProxyLease(previous);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        SocialProxyAmbient.Set(_previous);
    }
}
