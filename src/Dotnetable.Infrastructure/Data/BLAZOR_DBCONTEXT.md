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
