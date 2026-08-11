# Agent notes — Dotnetable

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
