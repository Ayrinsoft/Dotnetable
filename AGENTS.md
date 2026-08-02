# Agent notes — Dotnetable

## Admin operator documentation

There is a **static bilingual (FA/EN) docs site** for Admin features:

- Project: `src/Dotnetable.Admin.Docs`
- Content source: `src/Dotnetable.Admin.Docs/wwwroot/js/catalog.js`

**Rule:** When you add, remove, rename, or change the meaning of an Admin screen or workflow, update the matching docs page in `catalog.js` in the **same change**. Also keep `NavMenu.razor` and `AdminSearchCatalog.cs` aligned.

Run docs locally:

```bash
dotnet run --project src/Dotnetable.Admin.Docs
```
