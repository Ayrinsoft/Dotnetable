# Agent notes — Dotnetable

## Environment (current phase)

**There is no pre-installed production or customer site yet.** Work is still in the **local/test** phase.

| Phase | Status | Agent rules |
|-------|--------|-------------|
| Local test DB | **Active now** | Safe to run migrations, Schema Compare, seed, destructive schema experiments, and full app against the shared **test** database (`Dotnetable` on local SQL). Prefer reversible changes when practical, but do **not** block on “production safety” — this is not a live customer store. |
| First real test release | **Not yet** | Owner will say when the first semi-real online test site exists. Only then tighten caution for that environment. |
| Semi-real online test → real production | **Later** | After owner confirms: treat as shared/live; confirm before destructive ops, careful migrations, no casual data wipes. |

Until the owner announces otherwise: **assume only the local test database**; apply EF migrations, SSDT/Schema Compare, and feature work directly on it.

### EF migrations vs Schema Compare

- Schema is often applied with **Schema Compare** from `src/Dotnetable.Database` onto SQL Server.
- EF still needs a **current model snapshot** so `dotnet ef database update` / Admin startup `MigrateAsync` do not throw `PendingModelChangesWarning`.
- After a batch of entity changes applied via Schema Compare (or raw-SQL migrations without Designer), add a **snapshot-sync** migration if needed (`Up` may be no-op when DB already matches).
- Runtime already ignores `PendingModelChangesWarning` in `ConfigureProvider`; design-time `dotnet ef` does **not** — keep the snapshot honest.
- Prefer `dotnet-ef` tools version aligned with package runtime (currently EF Core **10.0.11**).

```bash
# Update tools when version mismatch warning appears
dotnet tool update --global dotnet-ef --version 10.0.11

# SqlServer migrations (connection via DOTNETABLE_MIGRATIONS_CONNECTION or Admin localsettings)
dotnet ef database update --project src/Dotnetable.Migrations.SqlServer --startup-project src/Dotnetable.Migrations.SqlServer
```

## Database schema (SSDT `.sql` must stay in sync)

Canonical table scripts live in `src/Dotnetable.Database/*.sql` (SSDT project `Dotnetable.Database.sqlproj`). Developers often apply schema via **Schema Compare** from this project onto SQL Server.

**Mandatory rule — after any model / schema change, update the matching `.sql` files first (same shape as the EF model), then continue with other work.**

Apply this whenever you change any of:

- `src/Dotnetable.Domain/Entities/*`
- `src/Dotnetable.Infrastructure/Data/AppDbContext.cs` (mappings, indexes, FKs, defaults)
- EF migrations under `src/Dotnetable.Migrations.*`

**Do this in order for the same change (do not leave `.sql` for later):**

1. Update entities / `AppDbContext` / generate EF migrations as needed.
2. **Immediately** update the corresponding table scripts under `src/Dotnetable.Database/`:
   - Columns, nullability, types, defaults (`CONSTRAINT … DEFAULT`)
   - Primary keys, unique constraints, indexes, foreign keys
   - New tables: add `TableName.sql` **and** include it in `Dotnetable.Database.sqlproj` (`Build Include=…`)
   - Dropped objects: remove from both the `.sql` file and the `.sqlproj` entry
3. Only after SSDT scripts match the model: services, API, Admin UI, docs, tests, etc.

**Why:** If `.sql` lags behind EF, Schema Compare treats the live DB as “extra” and **drops** columns/tables the app still uses → runtime `Invalid column name` / failed migrations. Stale SSDT also makes publish/compare unsafe.

**Shape checklist (mirror EF, not a subset):**

- Site-currency + USD dual money columns when both exist on the entity (e.g. `AvailableCredit` **and** `AvailableCreditUsd`)
- Indexes and FKs with the same names EF/migrations use when practical
- Defaults aligned with `HasDefaultValue` / migration defaults
- Comments/`sp_addextendedproperty` optional; structure is required

Do not reintroduce a second source of truth. Runtime upgrades still go through EF migrations on Admin startup; SSDT `.sql` must remain a full structural mirror so Schema Compare never deletes real model columns.

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
