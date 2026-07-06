# Dotnetable React (serverless front-end)

A static React SPA that mirrors the Dotnetable.Web public site but talks to `Dotnetable.API`
directly from the browser — no server-side rendering, so the `dist/` build output can be hosted on
any static host or CDN ("serverless" mode).

## Features

- Layout driven entirely by the API: brand/logo/contact/socials (`api/siteinfo`), header & footer
  menus (`api/menu/...`).
- **Runtime theming**: on boot the app fetches `api/theme/active` (managed in Admin → Themes) and
  applies the design tokens as CSS custom properties — re-theme the deployed site from the admin
  without a rebuild.
- Home (slideshow placement `home_top`, CMS `home` page, latest posts), Blog list + post detail,
  CMS pages by slug, Shop list + product detail, Contact form.
- **Dynamic forms & surveys** (`/form/:slug`) built in Admin → Forms & Surveys, with validation,
  success message and public survey results.

## Run

```bash
cp .env.example .env      # fill in VITE_API_URL and VITE_WEBSITE_KEY (the website's AuthCode)
npm install
npm run dev               # local dev
npm run build             # static production build in dist/
```

> CORS: the API must allow the SPA's origin.
