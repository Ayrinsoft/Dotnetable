# Agent notes — Dotnetable

## DbContext in services (Blazor Server)

**Never inject `AppDbContext` as a field** on an application service. Blazor runs layout + page + child components (several `RecordAttachmentsPanel`s on an order, nav, etc.) in parallel on one circuit scope. A scoped service holding one context throws `A second operation was started on this context instance`.

**Required pattern:** inject `IDbContextFactory<AppDbContext>` and open a short-lived context per call (`CreateDbContextAsync` / `DbContextFactoryExtensions.UseAsync`). For multi-service transactions, use `UseAmbientOrCreateAsync` so nested work joins `AmbientDbContext` when OrderService (etc.) has pushed one.

Do not add a new service with `private readonly AppDbContext _context`. `DbContextFieldTests.No_New_Service_May_Hold_An_AppDbContext_Field` enforces this.

That test carries a **baseline** of the ~70 services written before the rule. The list may shrink, never grow: convert a service to the factory and delete its name (a stale entry fails `Baseline_Contains_No_Stale_Entries`). Adding a name to the baseline to make a new service compile defeats the entire guard.

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

**There is no pre-installed production or customer site yet.** Work is still in the **local/test** phase.

| Phase | Status | Agent rules |
|-------|--------|-------------|
| Local test DB | **Active now** | Safe to run migrations, Schema Compare, seed, destructive schema experiments, and full app against the shared **test** database (`Dotnetable` on local SQL). Prefer reversible changes when practical, but do **not** block on “production safety” — this is not a live customer store. |
| First real test release | **Not yet** | Owner will say when the first semi-real online test site exists. Only then tighten caution for that environment. |
| Semi-real online test → real production | **Later** | After owner confirms: treat as shared/live; confirm before destructive ops, careful migrations, no casual data wipes. |

Until the owner announces otherwise: **assume only the local test database**; apply EF migrations, SSDT/Schema Compare, and feature work directly on it.

### EF migrations — only `InitialCreate` (keep it current)

This phase has **no customer/production upgrade path**. A new database must be born complete from **one** migration.

**Mandatory — do not add incremental / follow-up migrations.**

| Allowed | Forbidden |
|---------|-----------|
| Exactly one migration per provider: `InitialCreate` | `dotnet ef migrations add SomeFeature` as a second file |
| Regenerating `InitialCreate` after a model change | Snapshot-sync / no-op / raw-SQL delta migrations |
| Editing `InitialCreate` + `AppDbContextModelSnapshot` so they match the model | Leaving schema only in `.sql` or only in entities |

Providers (keep all three in lockstep):

- `src/Dotnetable.Migrations.SqlServer/Migrations/`
- `src/Dotnetable.Migrations.MySql/Migrations/`
- `src/Dotnetable.Migrations.PostgreSql/Migrations/`

After **any** entity / `AppDbContext` schema change, **in the same change**, regenerate init:

```bash
# Delete the current InitialCreate + Designer + AppDbContextModelSnapshot in that project, then:
dotnet ef migrations add InitialCreate --project src/Dotnetable.Migrations.SqlServer --startup-project src/Dotnetable.Migrations.SqlServer --output-dir Migrations
dotnet ef migrations add InitialCreate --project src/Dotnetable.Migrations.MySql --startup-project src/Dotnetable.Migrations.MySql --output-dir Migrations
dotnet ef migrations add InitialCreate --project src/Dotnetable.Migrations.PostgreSql --startup-project src/Dotnetable.Migrations.PostgreSql --output-dir Migrations
```

Confirm `dotnet ef migrations has-pending-model-changes` is clean for SqlServer (and the other providers when you touched them). The new `InitialCreate` must include every new column/table/index/FK (e.g. settlement currency) — never “add it in a later migration”.

Local test DBs: owner typically **recreates** the database via Admin Setup after an init squash. If tables already exist, `DatabaseUpdateService` only records the new `InitialCreate` in `__EFMigrationsHistory` (does not rebuild tables). Do not rely on leftover incremental history.

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
3. **Immediately** regenerate `InitialCreate` (+ snapshot) on all three providers so a brand-new DB from Admin Setup matches the model and the `.sql` files.
4. Only after `.sql` **and** `InitialCreate` match the model: services, API, Admin UI, docs, tests, etc.

**Why:** If `.sql` lags, Schema Compare treats the live DB as “extra” and **drops** columns the app still uses. If `InitialCreate` lags, a freshly created database is missing columns (`Invalid column name`) even though the model and SSDT look right.

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
