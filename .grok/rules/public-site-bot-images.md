# Public site images for crawlers

Every new public site must ship with the same crawler image rule as `src/Dotnetable.Web`. A new website row, theme, or storefront is not done until a crawler’s HTML contains no storage-CDN file address.

## Required behavior

- Classify the caller with `BotDetector` in `src/Dotnetable.Web/Services/BotDetector.cs`. Treat disguised crawlers as crawlers: `GoogleOther`, `Google-InspectionTool`, `Google-Read-Aloud`, `GoogleProducer`, `APIs-Google`, `DuplexWeb-Google`, `Google-Agent`, `meta-externalagent`. An empty User-Agent is a crawler.
- A person still gets the images. That includes a normal browser and the WhatsApp in-app browser (`Mozilla/` + `Chrome/` + `WhatsApp`, and not `compatible; WhatsApp`).
- On a crawler GET/HEAD of HTML, run `MediaHtml.StripForBots` through `BotMediaFilterMiddleware` before the body is sent. Remove uploaded-file URLs from `img`, `data-src`, `data-full`, `data-bg`, inline style, `<style>`, `<script>`, and image preload. An uploaded file is a 32-hex GUID name (optional `t_` or `_thumb`) or a path under `/api/files/`.
- Do not leave the URL in the response. Replacing it with the 1×1 `MediaHtml.Placeholder` is fine. An empty `src` is not.
- Keep the favicon. For a link-preview bot only (`WhatsApp/2.x`, Telegram, Facebook, `meta-externalfetcher`, and the other tokens in `LinkPreviewPattern`), keep the single `og:image`.
- New theme views load uploads with `MediaHtml.Placeholder` plus `data-src` / `data-bg`, not a raw CDN `src`. `wwwroot/js/media.js` swaps the real URL in for people.

## Where it lives

- A theme installed on `Dotnetable.Web` inherits the middleware in `Program.cs`. Do not add a second copy inside the theme.
- A standalone public site (its own host, Razor, or React app) must include the same detection, the same strip, and the middleware (or an equivalent response rewrite) registered for HTML GET/HEAD. Do not ship that site with CDN URLs in the HTML crawlers receive.
- This is host behavior, not a per-website database flag. Do not add a setting that can turn it off for a new site.
