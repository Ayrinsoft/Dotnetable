(() => {
  "use strict";

  const STORAGE_LANG = "dotnetable.docs.lang";
  const STORAGE_BOOK = "dotnetable.docs.book";

  const ui = {
    en: {
      brand: "Dotnetable Docs",
      "brand.sub.admin": "Admin guide",
      "brand.sub.api": "API guide",
      "book.admin": "Admin",
      "book.api": "API",
      search: "Search",
      "search.placeholder": "Search…",
      loading: "Loading…",
      "footer.hint": "When Admin or API behavior changes, update the matching page here.",
      purpose: "In plain words",
      howto: "Step by step",
      tips: "Common mistakes & tips",
      related: "Related pages",
      example: "Real example",
      columns: "What each column means",
      endpoints: "Endpoints",
      request: "Request",
      response: "Response",
      headers: "Headers",
      query: "Query parameters",
      body: "Body",
      auth: "Auth",
      "auth.see": "See Authentication",
      "path.admin": "Admin path",
      "path.api": "API path",
      "home.admin.title": "Admin documentation",
      "home.admin.lead": "What each Admin screen does, when you need it, and how to use it.",
      "home.api.title": "API documentation",
      "home.api.lead": "Public REST API for storefronts and apps — headers, auth once, then each endpoint with sample request/response.",
      browse: "Browse by section",
      noResults: "No matching pages.",
      notFound: "Page not found",
      notFoundLead: "That page does not exist. Pick another topic from the sidebar.",
      role: "Access",
      cross: "Same concept in",
      "cross.admin": "Admin",
      "cross.api": "API",
      "sample.status": "HTTP",
    },
    fa: {
      brand: "مستندات Dotnetable",
      "brand.sub.admin": "راهنمای ادمین",
      "brand.sub.api": "راهنمای API",
      "book.admin": "ادمین",
      "book.api": "API",
      search: "جستجو",
      "search.placeholder": "جستجو…",
      loading: "در حال بارگذاری…",
      "footer.hint": "با تغییر رفتار ادمین یا API، همان صفحه را اینجا هم به‌روز کنید.",
      purpose: "به زبان ساده",
      howto: "قدم‌به‌قدم",
      tips: "اشتباهات رایج و نکات",
      related: "صفحات مرتبط",
      example: "مثال واقعی",
      columns: "معنی هر ستون",
      endpoints: "اندپوینت‌ها",
      request: "درخواست",
      response: "پاسخ",
      headers: "هدرها",
      query: "پارامترهای کوئری",
      body: "بدنه",
      auth: "احراز هویت",
      "auth.see": "صفحه احراز هویت",
      "path.admin": "مسیر ادمین",
      "path.api": "مسیر API",
      "home.admin.title": "مستندات پنل ادمین",
      "home.admin.lead": "هر صفحه ادمین چه می‌کند، کی لازم است، و چطور با آن کار کنید.",
      "home.api.title": "مستندات API",
      "home.api.lead": "API عمومی برای استورفرانت و اپ‌ها — هدرها، یک‌بار احراز هویت، بعد هر اندپوینت با نمونه request/response.",
      browse: "مرور بر اساس بخش",
      noResults: "صفحه‌ای پیدا نشد.",
      notFound: "صفحه پیدا نشد",
      notFoundLead: "این صفحه وجود ندارد. موضوع دیگری را از منوی کناری انتخاب کنید.",
      role: "دسترسی",
      cross: "همین مفهوم در",
      "cross.admin": "ادمین",
      "cross.api": "API",
      "sample.status": "HTTP",
    },
  };

  const els = {
    html: document.documentElement,
    nav: document.getElementById("nav"),
    content: document.getElementById("content"),
    crumbs: document.getElementById("crumbs"),
    searchInput: document.getElementById("searchInput"),
    langFa: document.getElementById("langFa"),
    langEn: document.getElementById("langEn"),
    bookAdmin: document.getElementById("bookAdmin"),
    bookApi: document.getElementById("bookApi"),
    brandSub: document.getElementById("brandSub"),
    menuToggle: document.getElementById("menuToggle"),
    backdrop: document.getElementById("backdrop"),
    pathBadge: document.getElementById("pathBadge"),
    pathBadgeLabel: document.getElementById("pathBadgeLabel"),
    pathBadgeCode: document.getElementById("pathBadgeCode"),
  };

  let lang = "fa";
  let book = "admin";
  let query = "";

  function bookCatalog(b) {
    if (b === "api") return window.DOCS_API;
    return window.DOCS_ADMIN;
  }

  function parseHash() {
    const raw = (location.hash || "").replace(/^#\/?/, "");
    const parts = raw.split("/").filter(Boolean);
    let maybeLang = null;
    let maybeBook = null;
    if (parts[0] === "en" || parts[0] === "fa") maybeLang = parts.shift();
    if (parts[0] === "admin" || parts[0] === "api") maybeBook = parts.shift();
    // Legacy: #/fa/inventory-stock (no book) → admin
    const pageId = parts.join("/") || "home";
    return { lang: maybeLang, book: maybeBook, pageId };
  }

  function loadState() {
    const parsed = parseHash();
    if (parsed.lang === "fa" || parsed.lang === "en") lang = parsed.lang;
    else {
      const saved = localStorage.getItem(STORAGE_LANG);
      lang = saved === "en" || saved === "fa" ? saved : "fa";
    }
    if (parsed.book === "admin" || parsed.book === "api") book = parsed.book;
    else {
      const saved = localStorage.getItem(STORAGE_BOOK);
      book = saved === "api" || saved === "admin" ? saved : "admin";
    }
  }

  function setHash(pageId, replace) {
    const next = `#/${lang}/${book}/${pageId || "home"}`;
    if (replace) history.replaceState(null, "", next);
    else location.hash = next;
  }

  function t(key) {
    return (ui[lang] && ui[lang][key]) || ui.en[key] || key;
  }

  function txt(value) {
    if (!value) return "";
    if (typeof value === "string") return value;
    return value[lang] || value.en || value.fa || "";
  }

  function applyChrome() {
    els.html.lang = lang;
    els.html.dir = lang === "fa" ? "rtl" : "ltr";
    document.title = book === "api"
      ? (lang === "fa" ? "مستندات API · Dotnetable" : "API Docs · Dotnetable")
      : (lang === "fa" ? "مستندات ادمین · Dotnetable" : "Admin Docs · Dotnetable");

    document.querySelectorAll("[data-i18n]").forEach((node) => {
      const key = node.getAttribute("data-i18n");
      if (key === "brand.sub") {
        node.textContent = t(book === "api" ? "brand.sub.api" : "brand.sub.admin");
        return;
      }
      node.textContent = t(key);
    });
    document.querySelectorAll("[data-i18n-placeholder]").forEach((node) => {
      node.setAttribute("placeholder", t(node.getAttribute("data-i18n-placeholder")));
    });

    els.langFa.classList.toggle("active", lang === "fa");
    els.langEn.classList.toggle("active", lang === "en");
    els.bookAdmin.classList.toggle("active", book === "admin");
    els.bookApi.classList.toggle("active", book === "api");
    if (els.brandSub) els.brandSub.textContent = t(book === "api" ? "brand.sub.api" : "brand.sub.admin");

    localStorage.setItem(STORAGE_LANG, lang);
    localStorage.setItem(STORAGE_BOOK, book);
  }

  function allPages(catalog) {
    const list = [];
    for (const section of catalog.sections || []) {
      for (const page of section.pages || []) list.push({ section, page });
    }
    return list;
  }

  function findPage(catalog, pageId) {
    for (const section of catalog.sections || []) {
      const page = (section.pages || []).find((p) => p.id === pageId);
      if (page) return { section, page };
    }
    return null;
  }

  function matchesQuery(page, section) {
    if (!query) return true;
    const q = query.toLowerCase();
    const hay = [
      page.id,
      page.adminPath || "",
      page.path || "",
      page.method || "",
      txt(page.title),
      txt(page.summary),
      txt(section.title),
      ...(page.keywords || []).map(txt),
      ...(page.endpoints || []).flatMap((ep) => [ep.method || "", ep.path || "", txt(ep.summary || "")]),
    ]
      .join(" ")
      .toLowerCase();
    return hay.includes(q);
  }

  function escapeHtml(str) {
    return String(str)
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;");
  }

  function formatRich(value) {
    const raw = txt(value);
    if (!raw) return "";
    let html = escapeHtml(raw);
    html = html.replace(/\*\*(.+?)\*\*/g, "<strong>$1</strong>");
    html = html.replace(/`([^`]+)`/g, "<code>$1</code>");
    html = html.replace(/\n/g, "<br />");
    return html;
  }

  function renderParagraphs(value) {
    if (!value) return "";
    if (Array.isArray(value)) return value.map((p) => `<p>${formatRich(p)}</p>`).join("");
    return `<p>${formatRich(value)}</p>`;
  }

  function renderDefinitions(defs) {
    if (!defs || !defs.length) return "";
    return `<dl class="def-list">${defs
      .map(
        (d) => `
      <div class="def-row">
        <dt>${formatRich(d.term)}</dt>
        <dd>${formatRich(d.def)}</dd>
      </div>`
      )
      .join("")}</dl>`;
  }

  function prettyJson(value) {
    if (value == null) return "";
    if (typeof value === "string") return value;
    try {
      return JSON.stringify(value, null, 2);
    } catch {
      return String(value);
    }
  }

  function methodClass(method) {
    const m = (method || "GET").toUpperCase();
    if (m === "POST") return "method-post";
    if (m === "PUT") return "method-put";
    if (m === "DELETE") return "method-delete";
    if (m === "PATCH") return "method-patch";
    return "method-get";
  }

  function authLabel(auth) {
    if (!auth || auth === "none") return lang === "fa" ? "بدون توکن مشتری" : "No customer token";
    if (auth === "website") return lang === "fa" ? "فقط کلید وب‌سایت" : "Website key only";
    if (auth === "jwt") return lang === "fa" ? "JWT مشتری" : "Customer JWT";
    if (auth === "optional-jwt") return lang === "fa" ? "مهمان یا JWT" : "Guest or JWT";
    return txt(auth);
  }

  function renderSample(kind, sample) {
    if (!sample) return "";
    const label = kind === "request" ? t("request") : t("response");
    const status = sample.status ? `${t("sample.status")} ${sample.status}` : "";
    const body = prettyJson(sample.body !== undefined ? sample.body : sample);
    const note = sample.note ? `<div class="callout" style="margin:0.75rem">${formatRich(sample.note)}</div>` : "";
    return `
      <div class="sample-block">
        <header><span>${escapeHtml(label)}</span><span>${escapeHtml(status)}</span></header>
        ${note}
        <pre>${escapeHtml(body)}</pre>
      </div>`;
  }

  function renderEndpoint(ep, index) {
    const headers = (ep.headers || [])
      .map((h) => `<tr><th><code>${escapeHtml(txt(h.name) || h.name)}</code></th><td>${formatRich(h.desc || h)}</td></tr>`)
      .join("");
    const queryParams = (ep.query || [])
      .map((h) => `<tr><th><code>${escapeHtml(txt(h.name) || h.name)}</code></th><td>${formatRich(h.desc || h)}</td></tr>`)
      .join("");
    const pathParams = (ep.pathParams || [])
      .map((h) => `<tr><th><code>${escapeHtml(txt(h.name) || h.name)}</code></th><td>${formatRich(h.desc || h)}</td></tr>`)
      .join("");

    return `
      <section class="card" id="ep-${index}">
        <h2>${formatRich(ep.title || ep.path)}</h2>
        <div class="endpoint-line">
          <span class="method-pill ${methodClass(ep.method)}">${escapeHtml((ep.method || "GET").toUpperCase())}</span>
          <code class="path">${escapeHtml(ep.path || "")}</code>
          <span class="auth-badge">
            ${escapeHtml(t("auth"))}: ${escapeHtml(authLabel(ep.auth))}
            · <a href="#/${lang}/api/auth">${escapeHtml(t("auth.see"))}</a>
          </span>
        </div>
        ${ep.summary ? `<p style="margin-top:0.85rem">${formatRich(ep.summary)}</p>` : ""}
        ${ep.purpose ? renderParagraphs(ep.purpose) : ""}

        ${
          headers
            ? `<h3 style="margin-top:1rem">${escapeHtml(t("headers"))}</h3>
               <table class="kv-table"><tbody>${headers}</tbody></table>`
            : ""
        }
        ${
          pathParams
            ? `<h3 style="margin-top:1rem">Path</h3>
               <table class="kv-table"><tbody>${pathParams}</tbody></table>`
            : ""
        }
        ${
          queryParams
            ? `<h3 style="margin-top:1rem">${escapeHtml(t("query"))}</h3>
               <table class="kv-table"><tbody>${queryParams}</tbody></table>`
            : ""
        }

        <div class="sample-grid" style="margin-top:1rem">
          ${renderSample("request", ep.request)}
          ${renderSample("response", ep.response)}
        </div>

        ${
          ep.notes
            ? `<ul style="margin-top:0.9rem">${ep.notes.map((n) => `<li>${formatRich(n)}</li>`).join("")}</ul>`
            : ""
        }
      </section>`;
  }

  function renderCrossLinks(page) {
    const links = [];
    for (const id of page.relatedAdmin || []) {
      const hit = findPage(window.DOCS_ADMIN, id);
      if (!hit) continue;
      links.push({ book: "admin", id, title: hit.page.title });
    }
    for (const id of page.relatedApi || []) {
      const hit = findPage(window.DOCS_API, id);
      if (!hit) continue;
      links.push({ book: "api", id, title: hit.page.title });
    }
    if (!links.length) return "";
    return `
      <section class="card">
        <h2>${escapeHtml(t("cross"))}</h2>
        <div class="cross-links">
          ${links
            .map(
              (l) => `
            <a class="cross-link" href="#/${lang}/${l.book}/${l.id}">
              <span class="tag">${escapeHtml(t(l.book === "api" ? "cross.api" : "cross.admin"))}</span>
              <span>${escapeHtml(txt(l.title))}</span>
            </a>`
            )
            .join("")}
        </div>
      </section>`;
  }

  function renderNav(activeId) {
    const catalog = bookCatalog(book);
    const frag = document.createDocumentFragment();

    for (const section of catalog.sections || []) {
      const visiblePages = (section.pages || []).filter((p) => matchesQuery(p, section));
      if (query && visiblePages.length === 0) continue;

      const wrap = document.createElement("div");
      wrap.className = "nav-section";
      if (section.collapsedByDefault && !query && !(section.pages || []).some((p) => p.id === activeId)) {
        wrap.classList.add("collapsed");
      }

      const titleBtn = document.createElement("button");
      titleBtn.type = "button";
      titleBtn.className = "nav-section-title";
      titleBtn.innerHTML = `<span>${escapeHtml(txt(section.title))}</span><span class="chev">▾</span>`;
      titleBtn.addEventListener("click", () => wrap.classList.toggle("collapsed"));
      wrap.appendChild(titleBtn);

      const links = document.createElement("div");
      links.className = "nav-links";
      for (const page of section.pages || []) {
        const a = document.createElement("a");
        a.className = "nav-link";
        a.href = `#/${lang}/${book}/${page.id}`;
        a.textContent = txt(page.title);
        if (page.id === activeId) a.classList.add("active");
        if (query && !matchesQuery(page, section)) a.classList.add("hidden");
        links.appendChild(a);
      }
      wrap.appendChild(links);
      frag.appendChild(wrap);
    }

    if (!frag.childNodes.length) {
      const empty = document.createElement("div");
      empty.className = "callout";
      empty.textContent = t("noResults");
      frag.appendChild(empty);
    }

    els.nav.replaceChildren(frag);
  }

  function renderMaintenanceNote(catalog) {
    const note = catalog.meta && catalog.meta.maintenance;
    if (!note) return "";
    return `
      <section class="card">
        <h2>${escapeHtml(txt(note.title))}</h2>
        <p>${escapeHtml(txt(note.body))}</p>
        <ul>
          ${(note.items || []).map((item) => `<li>${escapeHtml(txt(item))}</li>`).join("")}
        </ul>
      </section>`;
  }

  function renderHome() {
    const catalog = bookCatalog(book);
    const cards = (catalog.sections || [])
      .map((section) => {
        const first = (section.pages || [])[0];
        if (!first) return "";
        return `
          <a class="feature-card" href="#/${lang}/${book}/${first.id}">
            <strong>${escapeHtml(txt(section.title))}</strong>
            <span>${escapeHtml(txt(section.summary || first.summary))}</span>
            <span class="path">${(section.pages || []).length} ${lang === "fa" ? "صفحه" : "pages"}</span>
          </a>`;
      })
      .join("");

    const titleKey = book === "api" ? "home.api.title" : "home.admin.title";
    const leadKey = book === "api" ? "home.api.lead" : "home.admin.lead";

    els.content.innerHTML = `
      <section class="hero">
        <h1>${escapeHtml(t(titleKey))}</h1>
        <p>${escapeHtml(t(leadKey))}</p>
      </section>
      <section class="card">
        <h2>${escapeHtml(t("browse"))}</h2>
        <div class="grid-cards">${cards}</div>
      </section>
      ${renderMaintenanceNote(catalog)}
    `;
    els.crumbs.innerHTML = `<strong>${escapeHtml(t(titleKey))}</strong>`;
    els.pathBadge.hidden = true;
  }

  function renderPage(section, page) {
    const steps = (page.howTo || []).map((s) => `<li>${formatRich(s)}</li>`).join("");
    const tips = (page.tips || []).map((s) => `<li>${formatRich(s)}</li>`).join("");
    const related = (page.related || [])
      .map((id) => {
        const hit = findPage(bookCatalog(book), id);
        if (!hit) return "";
        return `<a class="feature-card" href="#/${lang}/${book}/${id}">
          <strong>${escapeHtml(txt(hit.page.title))}</strong>
          <span>${escapeHtml(txt(hit.page.summary))}</span>
        </a>`;
      })
      .filter(Boolean)
      .join("");

    const purposeHtml = Array.isArray(page.purpose)
      ? page.purpose.map((p) => `<p class="lead-line">${formatRich(p)}</p>`).join("")
      : renderParagraphs(page.purpose || page.summary);

    const columnsHtml = page.columns
      ? `<section class="card"><h2>${escapeHtml(t("columns"))}</h2>${renderDefinitions(page.columns)}</section>`
      : "";

    const exampleHtml = page.example
      ? `<section class="card">
          <h2>${escapeHtml(t("example"))}</h2>
          ${renderParagraphs(page.example.intro)}
          ${page.example.steps ? `<ol class="steps">${page.example.steps.map((s) => `<li>${formatRich(s)}</li>`).join("")}</ol>` : ""}
          ${page.example.result ? `<div class="callout ok">${formatRich(page.example.result)}</div>` : ""}
        </section>`
      : "";

    const extra = (page.sections || [])
      .map(
        (s) => `
        <section class="card">
          <h2>${formatRich(s.title)}</h2>
          ${renderParagraphs(s.body)}
          ${s.definitions ? renderDefinitions(s.definitions) : ""}
          ${s.items ? `<ul>${s.items.map((i) => `<li>${formatRich(i)}</li>`).join("")}</ul>` : ""}
          ${s.callout ? `<div class="callout ${s.callout.tone || ""}">${formatRich(s.callout.text)}</div>` : ""}
        </section>`
      )
      .join("");

    const endpointsHtml = (page.endpoints || []).map((ep, i) => renderEndpoint(ep, i)).join("");

    // Single-endpoint pages (optional shortcut fields on page itself)
    const singleEndpoint =
      page.method && page.path
        ? renderEndpoint(
            {
              title: page.title,
              method: page.method,
              path: page.path,
              auth: page.auth,
              summary: page.endpointSummary || page.summary,
              headers: page.headers,
              query: page.query,
              pathParams: page.pathParams,
              request: page.request,
              response: page.response,
              notes: page.endpointNotes,
            },
            0
          )
        : "";

    els.content.innerHTML = `
      <section class="hero">
        <h1>${escapeHtml(txt(page.title))}</h1>
        <p>${formatRich(page.summary)}</p>
        <div class="meta-row">
          ${page.adminPath ? `<span class="chip">${escapeHtml(t("path.admin"))}: <code>${escapeHtml(page.adminPath)}</code></span>` : ""}
          ${page.path && !page.method ? `<span class="chip">${escapeHtml(t("path.api"))}: <code>${escapeHtml(page.path)}</code></span>` : ""}
          ${page.access ? `<span class="chip">${escapeHtml(t("role"))}: ${escapeHtml(txt(page.access))}</span>` : ""}
        </div>
      </section>

      <section class="card">
        <h2>${escapeHtml(t("purpose"))}</h2>
        ${purposeHtml}
      </section>

      ${columnsHtml}
      ${exampleHtml}
      ${
        steps
          ? `<section class="card"><h2>${escapeHtml(t("howto"))}</h2><ol class="steps">${steps}</ol></section>`
          : ""
      }
      ${extra}
      ${singleEndpoint}
      ${endpointsHtml}
      ${
        tips
          ? `<section class="card"><h2>${escapeHtml(t("tips"))}</h2><ul>${tips}</ul></section>`
          : ""
      }
      ${renderCrossLinks(page)}
      ${
        related
          ? `<section class="card"><h2>${escapeHtml(t("related"))}</h2><div class="grid-cards">${related}</div></section>`
          : ""
      }
    `;

    els.crumbs.innerHTML = `${escapeHtml(txt(section.title))} / <strong>${escapeHtml(txt(page.title))}</strong>`;

    const badgePath = page.adminPath || page.path || (page.endpoints && page.endpoints[0] && page.endpoints[0].path);
    if (badgePath) {
      els.pathBadge.hidden = false;
      els.pathBadgeLabel.textContent = t(page.adminPath ? "path.admin" : "path.api");
      els.pathBadgeCode.textContent = badgePath;
      els.pathBadge.title = badgePath;
    } else {
      els.pathBadge.hidden = true;
    }
  }

  function renderNotFound() {
    els.content.innerHTML = `
      <section class="not-found hero">
        <h1>${escapeHtml(t("notFound"))}</h1>
        <p>${escapeHtml(t("notFoundLead"))}</p>
      </section>`;
    els.crumbs.innerHTML = `<strong>${escapeHtml(t("notFound"))}</strong>`;
    els.pathBadge.hidden = true;
  }

  function closeSidebar() {
    document.body.classList.remove("sidebar-open");
    els.backdrop.hidden = true;
  }
  function openSidebar() {
    document.body.classList.add("sidebar-open");
    els.backdrop.hidden = false;
  }

  function route() {
    loadState();
    applyChrome();

    const parsed = parseHash();
    const pageId = parsed.pageId || "home";

    // Normalize legacy hashes and incomplete hashes
    if (!location.hash || location.hash === "#" || location.hash === "#/") {
      setHash("home", true);
    } else if (!parsed.book || !parsed.lang) {
      setHash(pageId, true);
    }

    const catalog = bookCatalog(book);
    if (!catalog || !Array.isArray(catalog.sections)) {
      els.content.innerHTML = `<div class="callout warn">Catalog failed to load for “${escapeHtml(book)}”.</div>`;
      return;
    }

    if (pageId === "home") {
      renderNav("home");
      renderHome();
      closeSidebar();
      return;
    }

    const hit = findPage(catalog, pageId);
    renderNav(pageId);
    if (!hit) renderNotFound();
    else renderPage(hit.section, hit.page);

    closeSidebar();
    els.content.focus({ preventScroll: true });
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  els.langFa.addEventListener("click", () => {
    lang = "fa";
    setHash(parseHash().pageId || "home");
    route();
  });
  els.langEn.addEventListener("click", () => {
    lang = "en";
    setHash(parseHash().pageId || "home");
    route();
  });
  els.bookAdmin.addEventListener("click", () => {
    book = "admin";
    setHash("home");
    route();
  });
  els.bookApi.addEventListener("click", () => {
    book = "api";
    setHash("home");
    route();
  });

  els.searchInput.addEventListener("input", (e) => {
    query = e.target.value.trim();
    renderNav(parseHash().pageId || "home");
  });

  els.menuToggle.addEventListener("click", () => {
    if (document.body.classList.contains("sidebar-open")) closeSidebar();
    else openSidebar();
  });
  els.backdrop.addEventListener("click", closeSidebar);
  window.addEventListener("hashchange", route);

  if (!window.DOCS_ADMIN || !window.DOCS_API) {
    els.content.innerHTML = `<div class="callout warn">Documentation catalogs failed to load.</div>`;
    return;
  }

  route();
})();
