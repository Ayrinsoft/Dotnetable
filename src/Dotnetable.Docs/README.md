# Dotnetable Docs

Static documentation site for **Admin** and **API** in one project. Switch with one click (Admin | API). Content is FA/EN via the language toggle.

## Run

```bash
dotnet run --project src/Dotnetable.Docs
```

Open **http://localhost:5188** (see `Properties/launchSettings.json`).

## Layout

| Path | Role |
|------|------|
| `wwwroot/index.html` | Shell + Admin/API switch |
| `wwwroot/css/fonts.css` + `wwwroot/fonts/` | Local fonts (no CDN) |
| `wwwroot/css/docs.css` | Theme |
| `wwwroot/js/app.js` | Router: `#/{lang}/{admin\|api}/{page}` |
| `wwwroot/js/catalog-admin.js` | Admin pages |
| `wwwroot/js/catalog-api.js` | API pages (samples + auth once) |

## Keep in sync

| Change | Update |
|--------|--------|
| Admin menu/screen | `catalog-admin.js` + `NavMenu` / `AdminSearchCatalog` |
| API controller | `catalog-api.js` |
| Shared concept (e.g. stock) | both catalogs + `relatedAdmin` / `relatedApi` |

Auth for the public API is documented **once** on API → Authentication. Endpoint pages only mark the auth level and link back.
