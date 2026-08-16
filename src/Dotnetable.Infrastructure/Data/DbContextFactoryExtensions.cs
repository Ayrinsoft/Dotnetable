using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Data;

/// <summary>
/// Helpers for short-lived <see cref="AppDbContext"/> instances.
/// Prefer this (or injecting <see cref="IDbContextFactory{TContext}"/>) in any code path that may
/// run in parallel with other Blazor component initializations — never share one long-lived
/// context across concurrent awaits.
/// </summary>
public static class DbContextFactoryExtensions
{
    public static async Task<T> UseAsync<T>(
        this IDbContextFactory<AppDbContext> factory,
        Func<AppDbContext, CancellationToken, Task<T>> action,
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await action(db, ct);
    }

    public static async Task UseAsync(
        this IDbContextFactory<AppDbContext> factory,
        Func<AppDbContext, CancellationToken, Task> action,
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        await action(db, ct);
    }

    /// <summary>
    /// Join an ambient UoW context when one is set (cross-service transaction);
    /// otherwise open a short-lived context. Safe for parallel Blazor init.
    /// </summary>
    public static async Task<T> UseAmbientOrCreateAsync<T>(
        this IDbContextFactory<AppDbContext> factory,
        Func<AppDbContext, CancellationToken, Task<T>> action,
        CancellationToken ct = default)
    {
        if (AmbientDbContext.Current is { } ambient)
            return await action(ambient, ct);
        await using var db = await factory.CreateDbContextAsync(ct);
        return await action(db, ct);
    }

    public static async Task UseAmbientOrCreateAsync(
        this IDbContextFactory<AppDbContext> factory,
        Func<AppDbContext, CancellationToken, Task> action,
        CancellationToken ct = default)
    {
        if (AmbientDbContext.Current is { } ambient)
        {
            await action(ambient, ct);
            return;
        }
        await using var db = await factory.CreateDbContextAsync(ct);
        await action(db, ct);
    }
}
