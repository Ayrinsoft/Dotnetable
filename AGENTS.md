# Agent notes — Dotnetable

## DbContext in services (Blazor Server)

**Never inject `AppDbContext` as a field** on an application service. Blazor runs layout + page + child components (several `RecordAttachmentsPanel`s on an order, nav, etc.) in parallel on one circuit scope. A scoped service holding one context throws `A second operation was started on this context instance`.

**Every service now follows this.** The conversion is done — the only remaining holders are `UnitOfWork` and `GenericRepository<T>`, both registered in DI and resolved by nothing.

**Required pattern**, one of two:

```csharp
// Ordinary service: a short-lived context per call.
public async Task<X> GetAsync(int id, CancellationToken ct = default)
{
    await using var _context = await _contextFactory.CreateDbContextAsync(ct);
    return await _context.Xs.FirstOrDefaultAsync(...);
}

// Service that takes part in cross-service transactions (inventory, wallet, ledger, stock docs):
public async Task<X> GetAsync(int id, CancellationToken ct = default)
{
    await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
    var _context = _lease.Context;
    ...
}
```

`DbLease` joins `AmbientDbContext.Current` when a caller has pushed one (so the work lands inside their transaction) and otherwise opens its own, disposing only what it created. The local is named `_context` on purpose: it keeps method bodies uniform and makes the lifetime obvious at the top of each method.

### Private helpers must take the caller's context

```csharp
private static async Task<Row> EnsureRowAsync(AppDbContext _context, int id, CancellationToken ct)
```

A private helper that opens **its own** context and is called from another method is silently broken: whatever it stages is discarded when its context is disposed, and whatever it returns is detached from the caller's change tracker. `EmailTemplateService.EnsureOwnRowAsync` was exactly this — it built a template row that no `SaveChanges` ever wrote. The compiler cannot catch it, so the rule is mechanical: **if a private helper touches the context and is called from anywhere else in the class, it takes `AppDbContext _context` as its first parameter.**

The same applies to any helper returning `IQueryable` — the query is only valid while the context that built it is alive.

Do not add a new service with `private readonly AppDbContext _context`. `DbContextFieldTests.No_New_Service_May_Hold_An_AppDbContext_Field` enforces this, with a baseline that may shrink and never grow (a stale entry fails `Baseline_Contains_No_Stale_Entries`). Adding a name to the baseline to make a new service compile defeats the entire guard.

### What this changed for tests

Services write through their own context, so a fixture that seeds and asserts through its own `_context` will read stale entities out of its identity map. Call `_context.ChangeTracker.Clear()` between the act and the assert, or read back through a fresh context.

## Concurrency on contended counters

Stock reservations (`InventoryService`, `VendorProductService`, `WarehouseService`) and wallet balances (`ClientWalletService`) move with a single atomic `ExecuteUpdateAsync` carrying the check in its `WHERE` clause, e.g. `WHERE QuantityOnHand - QuantityReserved >= qty`, treating the affected-row count as the answer.

**Do not "simplify" these back into read-modify-write.** `IsRowVersion()` is only server-maintained on SQL Server; on MySQL (`longblob`) and PostgreSQL (`bytea`) the column never changes, so EF's `WHERE RowVersion = @old` always matches and the optimistic concurrency silently degrades to last-write-wins — two buyers reserving the same unit, two debits spending the same balance. `ConcurrencyModelExtensions` therefore only declares the token where the engine can maintain it.

`ExecuteUpdate` bypasses the change tracker, so each of those methods calls a `RefreshTrackedAsync` helper afterwards to reload the entity if this context already had it loaded. Keep that when adding a similar path, or a tracked entity will serve the pre-update value and overwrite it on the next `SaveChanges`.

Tests touching these paths must use `RelationalTestDb` (SQLite in memory), not `UseInMemoryDatabase` — the InMemory provider has no SQL and throws on `ExecuteUpdate`.

## Security invariants

Do not weaken these without saying so explicitly; each one closes a hole that was open before launch.

| Invariant | Where |
|-----------|-------|
| One-time codes have an attempt budget and are destroyed when it is spent | `WebsiteClientAuthService.CheckCodeAsync` |
| Sign-in lockout after repeated failures, for customers **and** admin members | `WebsiteClientAuthService`, `MemberService.ValidateSignInAsync` |
| Passwords go through `PasswordPolicy` — never a bare length check | `Dotnetable.Application.Security.PasswordPolicy` |
| Every public write endpoint is rate limited per IP | `[EnableRateLimiting]` + `Dotnetable.Hosting.RateLimiting` |
| Secrets fail the boot outside Development rather than falling back to a placeholder | `StartupValidation.ValidateProductionSecrets` |
| Admin-authored HTML is sanitised before rendering | `ContentSanitizer`, called from `ContentShortcodeProcessor` |
| Uploads are checked against a deny-list that overrides any configured allow-list | `FileService.ValidateExtension` |
| Files are served with a MIME derived from the stored extension, not the uploader's header | `FilesController` |
| An order is marked paid only on a server-to-server verify, never on a callback query string | `OnlinePaymentService.CompleteAsync` |
| Page sizes are clamped to `GridQuery.MaxPageSize` | `GridQuery.Take`, `GridQuery.ClampPageSize` |

## Provider frameworks (SMS, payment gateways)

SMS senders and payment gateways follow the same shape as storage backends: an interface with a string `Key`, one class per provider, a registry that resolves by key, and per-website rows (`WebsiteSmsSettings`, `PaymentGateways`) holding a provider key plus a settings JSON blob.

Adding a gateway is **a new class and a DI line** — never a change to checkout, orders or auth. Each also ships a template-driven provider (`GenericHttpSmsProvider`, `GenericRedirectGatewayProvider`) so a panel that needs no bespoke code can be configured from the admin UI alone.

Payment providers must declare `AmountUnit`: the shop prices in the site currency's major unit, most Iranian aggregators bill in Rial, PayPing bills in Toman and Stripe in minor units. Getting this wrong charges the customer ten or a hundred times the order total.

## Environment (current phase)

**There is a live production site.** The owner put it into production; treat the deployed database as
real and shared. This flipped after an incident: an agent kept squashing everything into a single
regenerated `InitialCreate` (the old policy below), the owner deployed a build with a new table, and
`DatabaseUpdateService`'s legacy-baseline logic silently marked the *new* `InitialCreate` as "already
applied" (because core tables already existed) without ever running its `CREATE TABLE` — so the new
table never got created and the admin panel 500'd on first use. Do not repeat this.

| Phase | Status | Agent rules |
|-------|--------|-------------|
| Local test DB | Done | Was used before the first production deploy. |
| Production | **Active now** | Confirm before destructive ops; no casual data wipes; every schema change ships as a real migration (below) that gets applied to the live DB deliberately, not assumed. |

### EF migrations — real incremental migrations from now on (no more squashing)

**Mandatory — every entity / `AppDbContext` schema change ships its own new migration file.**
Never delete or regenerate an already-shipped `InitialCreate` (or any other already-shipped
migration) again — the live database's `__EFMigrationsHistory` already has specific migration IDs
recorded as applied; replacing those files with regenerated ones under new IDs breaks the history
match and the change silently never applies (see the incident above).

| Allowed | Forbidden |
|---------|-----------|
| `dotnet ef migrations add <FeatureName>` — a new file, on top of history | Deleting/regenerating `InitialCreate` or any other already-committed migration |
| Exactly one new migration per schema change, per provider | Editing the `Up`/`Down` of a migration that has already been committed/shipped |
| Keeping all three providers in lockstep (same change, one migration each) | Leaving schema only in `.sql` or only in entities |

Providers (keep all three in lockstep):

- `src/Dotnetable.Migrations.SqlServer/Migrations/`
- `src/Dotnetable.Migrations.MySql/Migrations/`
- `src/Dotnetable.Migrations.PostgreSql/Migrations/`

After **any** entity / `AppDbContext` schema change, **in the same change**, add one migration per
provider (never touch existing migration files):

```bash
dotnet ef migrations add <FeatureName> --project src/Dotnetable.Migrations.SqlServer --startup-project src/Dotnetable.Migrations.SqlServer --output-dir Migrations
dotnet ef migrations add <FeatureName> --project src/Dotnetable.Migrations.MySql --startup-project src/Dotnetable.Migrations.MySql --output-dir Migrations
dotnet ef migrations add <FeatureName> --project src/Dotnetable.Migrations.PostgreSql --startup-project src/Dotnetable.Migrations.PostgreSql --output-dir Migrations
```

Confirm `dotnet ef migrations has-pending-model-changes` is clean for all three providers you
touched, and that the generated `Up()` contains only the intended delta (review it — it should not
try to re-create tables that already exist).

**Applying to the live database:** the Admin panel has a built-in page for this —
`/system/updates` (`DatabaseUpdates.razor`, `SuperAdminOnly`) lists pending migrations and applies
them with one click via `IDatabaseUpdateService.ApplyUpdatesAsync()` (`context.Database.MigrateAsync`).
This is the intended way to roll out a schema change to production after deploying a new build —
do not assume a deploy alone applies pending migrations; someone (owner or automated pipeline) must
still trigger apply. Never suggest recreating/dropping the production database.

Prefer `dotnet-ef` tools version aligned with package runtime (currently EF Core **10.0.11**).

```bash
dotnet tool update --global dotnet-ef --version 10.0.11
```

## Database schema (SSDT `.sql` must stay in sync — same change)

Canonical table scripts live in `src/Dotnetable.Database/*.sql` (SSDT project `Dotnetable.Database.sqlproj`). Developers often apply schema via **Schema Compare** from this project onto SQL Server.

**Mandatory — whenever the EF model changes, update the matching `.sql` files in the same turn.** Do not finish the task with only entities / only init / only `.sql` updated.

Apply this whenever you change any of:

- `src/Dotnetable.Domain/Entities/*`
- `src/Dotnetable.Infrastructure/Data/AppDbContext.cs` (mappings, indexes, FKs, defaults)
- `InitialCreate` under `src/Dotnetable.Migrations.*`

**Do this in order for the same change:**

1. Update entities / `AppDbContext`.
2. **Immediately** update the corresponding table scripts under `src/Dotnetable.Database/`:
   - Columns, nullability, types, defaults (`CONSTRAINT … DEFAULT`)
   - Primary keys, unique constraints, indexes, foreign keys
   - New tables: add `TableName.sql` **and** include it in `Dotnetable.Database.sqlproj` (`Build Include=…`)
   - Dropped objects: remove from both the `.sql` file and the `.sqlproj` entry
3. **Immediately** add a new migration (+ snapshot update) on all three providers — see "EF migrations" above. Never regenerate/delete an already-shipped migration.
4. Only after `.sql` **and** the new migration match the model: services, API, Admin UI, docs, tests, etc.

**Why:** If `.sql` lags, Schema Compare treats the live DB as “extra” and **drops** columns the app still uses. If the migration lags or is missing, the live database never gets the new column/table (`Invalid column name` / `Invalid object name`) even though the model and SSDT look right — this is exactly what caused the `/website/contact-info` 500 in production.

**Shape checklist (mirror EF, not a subset):**

- Site-currency + USD dual money columns when both exist on the entity (e.g. `AvailableCredit` **and** `AvailableCreditUsd`)
- Indexes and FKs with the same names EF/`InitialCreate` use when practical
- Defaults aligned with `HasDefaultValue` / init defaults
- Comments/`sp_addextendedproperty` optional; structure is required

Do not reintroduce a second source of truth. Entities + `AppDbContext` are the model; `.sql` and `InitialCreate` are two full mirrors of that same shape.

## Documentation site

Unified static docs (Admin + API):

- Project: `src/Dotnetable.Docs`
- Admin content: `wwwroot/js/catalog-admin.js`
- API content: `wwwroot/js/catalog-api.js`
- Switch in UI: Admin | API (`#/{lang}/{admin|api}/{pageId}`)

**Rule:** When Admin UI/behavior or API endpoints change, update the matching catalog page in the same change. Cross-link with `relatedAdmin` / `relatedApi` when the concept exists in both.

Auth for the storefront API is only detailed on the API page `auth`. Do not re-explain full auth on every endpoint.

```bash
dotnet run --project src/Dotnetable.Docs
```

Do not reintroduce Swagger UI on `Dotnetable.API`; use this docs site instead.
