# Agent notes — Dotnetable

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
