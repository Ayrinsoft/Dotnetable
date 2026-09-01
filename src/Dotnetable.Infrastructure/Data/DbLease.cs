using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Data;

/// <summary>
/// A borrowed <see cref="AppDbContext"/> for one operation, used by the services that take part in
/// cross-service transactions.
///
/// <para>Those services need two things at once. When <c>OrderService</c> has pushed an ambient
/// context for a checkout transaction, they must use <em>that</em> context, or their writes land on a
/// different connection and block on the uncommitted rows until the command times out. When there is
/// no ambient context — the ordinary case, and every Blazor page load — they must use a context of
/// their own, because a field-held one is shared by every component on the circuit and throws
/// "A second operation was started on this context instance" the moment two of them initialise in
/// parallel.</para>
///
/// <para>This resolves the first case without disposing what it did not create, and the second case
/// with a short-lived context it does dispose. Use it as:</para>
/// <code>
/// await using var lease = await DbLease.OpenAsync(_contextFactory, ct);
/// var _context = lease.Context;
/// </code>
/// </summary>
public readonly struct DbLease : IAsyncDisposable
{
    private readonly bool _owned;

    private DbLease(AppDbContext context, bool owned)
    {
        Context = context;
        _owned = owned;
    }

    /// <summary>The context to use for this operation. Never null.</summary>
    public AppDbContext Context { get; }

    /// <summary>
    /// Joins the ambient transaction context when one is in flight, otherwise opens a short-lived one.
    /// </summary>
    public static async Task<DbLease> OpenAsync(IDbContextFactory<AppDbContext> factory, CancellationToken ct = default)
    {
        if (AmbientDbContext.Current is { } ambient)
            return new DbLease(ambient, owned: false);

        return new DbLease(await factory.CreateDbContextAsync(ct), owned: true);
    }

    /// <summary>Disposes the context only when this lease created it; an ambient one outlives us.</summary>
    public ValueTask DisposeAsync() => _owned ? Context.DisposeAsync() : ValueTask.CompletedTask;
}
