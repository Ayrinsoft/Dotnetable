namespace Dotnetable.Infrastructure.Data;

/// <summary>
/// Optional ambient <see cref="AppDbContext"/> for multi-service unit-of-work operations.
/// <para>
/// <see cref="AppDbContext"/> is registered as Transient (see <c>BLAZOR_DBCONTEXT.md</c>), so each
/// scoped service receives its own instance at construction. That prevents Blazor parallel-init
/// races, but breaks cross-service transactions: if <c>OrderService</c> begins a transaction on
/// context A and then calls <c>VendorCreditService</c> (context B), B cannot see A’s uncommitted
/// inserts and will block on row locks until <see cref="Microsoft.Data.SqlClient.SqlException"/>
/// command timeout (often at the first <c>Orders</c> read inside settlement).
/// </para>
/// <para>
/// Call <see cref="Use"/> around a transactional block so nested services join the same connection
/// and transaction. Services resolve via <c>AmbientDbContext.Current ?? injectedContext</c>.
/// </para>
/// </summary>
public static class AmbientDbContext
{
    private static readonly AsyncLocal<AppDbContext?> CurrentLocal = new();

    public static AppDbContext? Current => CurrentLocal.Value;

    /// <summary>Push <paramref name="context"/> as ambient for the current async flow until disposed.</summary>
    public static IDisposable Use(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var previous = CurrentLocal.Value;
        CurrentLocal.Value = context;
        return new Pop(previous);
    }

    private sealed class Pop(AppDbContext? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            CurrentLocal.Value = previous;
        }
    }
}
