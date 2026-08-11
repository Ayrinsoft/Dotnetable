# Blazor Server + EF Core concurrency

## The bug

```
System.InvalidOperationException: A second operation was started on this context instance
before a previous operation completed.
```

Blazor Server runs `OnInitializedAsync` of **layout, nav, and page** in parallel inside **one DI scope** (the circuit). If every service shares one scoped `AppDbContext`, two awaits hit the same instance → EF throws.

## What we do now

| Registration | Lifetime | Why |
|---|---|---|
| `IDbContextFactory<AppDbContext>` | Scoped options | Live connection string from `localsettings` |
| `AppDbContext` | **Transient** | Each consumer (scoped service) gets its **own** context at construction — parallel services no longer share |

## Preferred pattern for **new** services

```csharp
public class MyService(IDbContextFactory<AppDbContext> factory) : IMyService
{
    public async Task<Item?> GetAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Items.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}

// or:
await factory.UseAsync(async (db, ct) =>
    await db.Items.ToListAsync(ct), ct);
```

Even with Transient injection, **two concurrent queries inside the same service method** on the same field still race. Short-lived factory contexts fix that too.

## Detection

- Unit tests under `tests/Dotnetable.Tests/Architecture/DbContextConcurrencyTests.cs`
  - Asserts `AppDbContext` is Transient (not Scoped)
  - Caps how many services may still hold a long-lived `AppDbContext` field
  - Documents factory-only “canonical” services

## Residual risk

- `Task.WhenAll` of two methods on the **same** service instance that both use `_context`
- Fire-and-forget (`_ = DoDbWorkAsync()`) while another call uses `_context`

Mitigation: use `IDbContextFactory` per operation, not a field.

## Multi-service transactions (critical)

Because each service owns a **different** Transient context, this pattern deadlocks / times out on SQL Server:

```csharp
// OrderService._context  (connection A)
await using var tx = await _context.Database.BeginTransactionAsync(ct);
_context.Orders.Add(order);
await _context.SaveChangesAsync(ct); // exclusive locks on uncommitted Order row

// VendorCreditService._context  (connection B) — waits for A's locks until command timeout
await _vendorCredit.SettleHostOrderAsync(order.OrderID, ct);
```

Symptom:

```
Microsoft.Data.SqlClient.SqlException: Execution Timeout Expired.
```

often on the first `Orders` query inside `VendorCreditService.SettleHostOrderAsync`.

### Fix: ambient unit-of-work context

```csharp
await using var tx = await _context.Database.BeginTransactionAsync(ct);
using var ambient = AmbientDbContext.Use(_context); // nested services join connection A

await _vendorCredit.SettleHostOrderAsync(order.OrderID, ct);
await tx.CommitAsync(ct);
```

Order-path services resolve context as:

```csharp
private readonly AppDbContext _fallback;
private AppDbContext _context => AmbientDbContext.Current ?? _fallback;
```

See `AmbientDbContext.cs`. Apply `AmbientDbContext.Use` around any block that opens a transaction and then calls other DbContext-backed services.
