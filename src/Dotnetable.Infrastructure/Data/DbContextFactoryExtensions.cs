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
}
