# Dotnetable — agent notes

## Production is live — database schema changes need a real migration

See `AGENTS.md` ("EF migrations — real incremental migrations from now on") for the full policy
and the incident that made it mandatory: a squashed/regenerated `InitialCreate` was deployed after
production already had an older `InitialCreate` recorded in `__EFMigrationsHistory`; the ID
mismatch made `DatabaseUpdateService`'s legacy-baseline logic silently mark the new migration as
"already applied" without ever creating the new table, so the feature 500'd in production. **Every
entity / `AppDbContext` change ships as a brand-new `dotnet ef migrations add <Name>` file on top of
history, for all three providers — never delete or regenerate an already-shipped migration.**
Applying to the live DB is a deliberate step: `/system/updates` in the Admin panel (`SuperAdminOnly`).

## Admin permissions: mandatory checklist for every new admin feature/service

The Admin panel (`src/Dotnetable.Admin`) is role-key based. **Every time a new admin-panel
page, CRUD screen, or service is implemented, its permission wiring must be completed in full —
never leave a feature reachable only by master/superadmin.** A feature with no dedicated,
grantable permission is a feature site owners and staff can never be given access to, no matter
what an admin tries to check in the Policy editor.

Do all of the following, every time:

1. **Add the permission key(s)** in `src/Dotnetable.Application/Authorization/RoleKeys.cs`.
   - Convention: `"area.action"` (e.g. `"forms.edit"`). The part before the first dot becomes the
     visual group in the Policy editor and the search palette; keep it to exactly one dot unless
     you also special-case the group/label lookups (see `PolicyEdit.razor`'s `GroupTitle`/
     `RoleActionLabel` — a 3-segment key like `website.social.edit` breaks the default label
     parsing, which assumes `area.action`).
   - If a feature is safe to delegate to a non-master site owner separately from an existing
     broad key (e.g. don't bundle a low-risk content feature into a key that also grants IP
     whitelist / script injection), give it its **own** key rather than reusing a broad one.
2. **Register it** in `src/Dotnetable.Application/Authorization/RoleCatalog.cs` (`RoleCatalog.All`)
   with a clear, non-technical description. This is what makes the key seedable, selectable in the
   Policy editor, and turned into an ASP.NET Core authorization policy.
3. **Guard the page**: `@attribute [Authorize(Policy = RoleKeys.YourKey)]` on the Razor page. If a
   page must be reachable through more than one key (a legacy broad key AND a new narrow one, so
   existing grants aren't broken by the split), add a composite policy in
   `src/Dotnetable.Admin/Auth/AdminPolicies.cs` (see `WebsiteSocialAccess`/`WebsiteContactAccess`
   for the pattern) and use that policy name instead.
4. **Add the nav link** in `src/Dotnetable.Admin/Components/Layout/NavMenu.razor`, wrapped in
   `<AuthorizeView Policy="@RoleKeys.YourKey">` (or the composite policy) so it only renders when
   granted. If it sits in a group gated by an outer `@if (_canX || ...)`, add the new key to that
   condition too or the whole group can disappear even when the user holds the new key.
5. **Add a search-palette entry** in `src/Dotnetable.Admin/Navigation/AdminSearchCatalog.cs` (kept
   in sync with NavMenu per the comment at the top of that file) — use `AltRoleKey` if the page is
   reachable through two keys.
6. **Update the public guide/catalog** if it exists: `src/Dotnetable.Docs/wwwroot/js/catalog-admin.js`
   (and `catalog-api.js` cross-links when the same concept is exposed publicly).
7. Rebuild (`dotnet build Dotnetable.sln`) and confirm no errors.

### Why permissions can silently go missing even when the code looks right

- `PolicyService.GetAssignableRolesAsync` only lets a **non-master** admin grant roles **they
  themselves already hold** (master/superadmin bypasses this and can grant everything). If the
  person granting access doesn't have a key, it won't even appear as a checkbox for them to give
  to someone else — it's not "missing from the system", it's missing from the granter's own
  policy.
- `SetupService.SyncRoleCatalogAsync` runs on app startup and additively inserts any `RoleCatalog`
  key missing from the DB `Roles` table, and auto-grants every catalog key to policies literally
  titled `DefaultPolicies.Administrators` and to the seeded staff templates
  (`DefaultPolicies.StaffTemplates`). **It does NOT touch custom-named policies** an admin created
  by hand (e.g. a one-off "Full Post Access" policy) — those must be manually re-opened and the
  new checkboxes checked after a new permission key ships.
- The Policy editor (`PolicyEdit.razor`) groups checkboxes by the key's area segment and sorts
  groups alphabetically by default. A conceptually-related permission in a differently-named area
  (e.g. Category/Tag/PostType management lives under `taxonomy.*`, not `posts.*`) is easy for an
  admin to miss when they only check the obviously-named group. When adding a key that's part of
  an existing feature area but must stay a separate key, special-case its group label/sort order
  in `PolicyEdit.razor` (`GroupTitle`/`GroupSortKey`) so it renders next to the feature it belongs
  to instead of wherever it falls alphabetically.
