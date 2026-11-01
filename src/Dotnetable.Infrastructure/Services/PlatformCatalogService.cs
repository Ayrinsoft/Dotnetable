using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Catalog;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class PlatformCatalogService : IPlatformCatalogService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public PlatformCatalogService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<bool> IsPlatformAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await PlatformCatalog.IsPlatformAsync(context, ct);
    }

    public async Task<int> CatalogWebsiteIdAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await PlatformCatalog.CatalogWebsiteIdAsync(context, websiteId, ct);
    }
}
