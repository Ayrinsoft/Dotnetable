# Dotnetable Admin Docs

Static bilingual (**فارسی / English**) operator guide for every **Dotnetable Admin** area: what each screen is for and how to use it.

## Run

```bash
dotnet run --project src/Dotnetable.Admin.Docs
```

Then open **http://localhost:5188** (see `Properties/launchSettings.json`).

You can also open `wwwroot/index.html` directly, but serving via Kestrel is recommended so deep links and static assets behave consistently.

## Layout

| Path | Role |
|------|------|
| `wwwroot/index.html` | Shell (sidebar, language switch, content host) |
| `wwwroot/css/fonts.css` | Local `@font-face` for Inter + Vazirmatn (no CDN) |
| `wwwroot/fonts/*.woff2` | Font binary files |
| `wwwroot/css/docs.css` | Theme (RTL/LTR) |
| `wwwroot/js/app.js` | Router, search, i18n chrome |
| `wwwroot/js/catalog.js` | **All page content** (FA + EN), structured like Admin menus |

Fonts are fully offline. Do not reintroduce Google Fonts CDN links.

## Keep docs in sync with Admin

Whenever you change Admin behavior or navigation:

1. Update `NavMenu.razor` and `AdminSearchCatalog.cs` (existing project rule).
2. Update the matching entry in `wwwroot/js/catalog.js` (purpose, how-to, tips, `adminPath`).
3. If you add a new Admin page, add a new docs page under the same section and link `related` where useful.

Do not ship Admin feature changes without the matching docs update when the change affects how operators use the panel.

## Language

- Default language is **Persian (`fa`)** with RTL layout.
- Toggle **FA / EN** in the sidebar; preference is stored in `localStorage`.
- Hash routes look like `#/fa/inventory-stock` or `#/en/inventory-stock`.
