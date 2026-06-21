using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class DatabaseUpdateService : IDatabaseUpdateService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public DatabaseUpdateService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<string>> GetAppliedUpdatesAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return (await context.Database.GetAppliedMigrationsAsync(ct)).ToList();
    }

    public async Task<IReadOnlyList<string>> GetPendingUpdatesAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return (await context.Database.GetPendingMigrationsAsync(ct)).ToList();
    }

    public async Task ApplyUpdatesAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await context.Database.MigrateAsync(ct);
    }
}
