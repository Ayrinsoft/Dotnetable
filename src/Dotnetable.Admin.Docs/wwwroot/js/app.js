(() => {
  "use strict";

  const STORAGE_KEY = "dotnetable.admin.docs.lang";
  const ui = {
    en: {
      brand: "Dotnetable Docs",
      "brand.sub": "Admin guide",
      search: "Search",
      "search.placeholder": "Search pages…",
      "open.admin": "Admin route",
      loading: "Loading…",
      "footer.hint": "When Admin features change, update the matching page here.",
      purpose: "In plain words",
      howto: "Step by step",
      tips: "Common mistakes & tips",
      related: "Related pages",
      example: "Real example",
      columns: "What each column means",
      homeTitle: "Admin documentation",
      homeLead: "Plain-language guide for every Dotnetable Admin screen — what it does, when you need it, and exactly how to use it.",
      browse: "Browse by section",
      noResults: "No matching pages.",
      notFound: "Page not found",
      notFoundLead: "That documentation page does not exist. Pick another topic from the sidebar.",
      role: "Role / access",
      path: "Admin path",
    },
    fa: {
      brand: "مستندات Dotnetable",
      "brand.sub": "راهنمای ادمین",
      search: "جستجو",
      "search.placeholder": "جستجوی صفحات…",
      "open.admin": "مسیر ادمین",
      loading: "در حال بارگذاری…",
      "footer.hint": "هر وقت قابلیت ادمین عوض شد، همان بخش را اینجا هم به‌روز کنید.",
      purpose: "به زبان ساده",
      howto: "قدم‌به‌قدم",
      tips: "اشتباهات رایج و نکات",
      related: "صفحات مرتبط",
      example: "مثال واقعی",
      columns: "معنی هر ستون",
      homeTitle: "مستندات پنل ادمین",
      homeLead: "راهنمای ساده برای هر صفحه ادمین Dotnetable — این صفحه چیست، کی لازم است، و دقیقاً چطور با آن کار کنید.",
      browse: "مرور بر اساس بخش",
      noResults: "صفحه‌ای پیدا نشد.",
      notFound: "صفحه پیدا نشد",
      notFoundLead: "این صفحه در مستندات وجود ندارد. موضوع دیگری را از منوی کناری انتخاب کنید.",
      role: "نقش / دسترسی",
      path: "مسیر ادمین",
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
    menuToggle: document.getElementById("menuToggle"),
    backdrop: document.getElementById("backdrop"),
    adminPathLink: document.getElementById("adminPathLink"),
    adminPathCode: document.getElementById("adminPathCode"),
  };

  let lang = loadLang();
  let query = "";

  function loadLang() {
    const fromHash = parseHash().lang;
    if (fromHash === "fa" || fromHash === "en") return fromHash;
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved === "fa" || saved === "en") return saved;
    return "fa";
  }

  function parseHash() {
    const raw = (location.hash || "#/fa/").replace(/^#\/?/, "");
    const parts = raw.split("/").filter(Boolean);
    const maybeLang = parts[0] === "en" || parts[0] === "fa" ? parts.shift() : null;
    const pageId = parts.join("/") || "home";
    return { lang: maybeLang, pageId };
  }

  function setHash(pageId, replace) {
    const next = `#/${lang}/${pageId}`;
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
    document.title = lang === "fa" ? "مستندات ادمین Dotnetable" : "Dotnetable Admin Docs";

    document.querySelectorAll("[data-i18n]").forEach((node) => {
      node.textContent = t(node.getAttribute("data-i18n"));
    });
    document.querySelectorAll("[data-i18n-placeholder]").forEach((node) => {
      node.setAttribute("placeholder", t(node.getAttribute("data-i18n-placeholder")));
    });

    els.langFa.classList.toggle("active", lang === "fa");
    els.langEn.classList.toggle("active", lang === "en");
    localStorage.setItem(STORAGE_KEY, lang);
  }

  function allPages() {
    const list = [];
    for (const section of window.DOCS.sections) {
      for (const page of section.pages) {
        list.push({ section, page });
      }
    }
    return list;
  }

  function findPage(pageId) {
    for (const section of window.DOCS.sections) {
      const page = section.pages.find((p) => p.id === pageId);
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
      txt(page.title),
      txt(page.summary),
      txt(section.title),
      ...(page.keywords || []).map(txt),
    ]
      .join(" ")
      .toLowerCase();
    return hay.includes(q);
  }

  function renderNav(activeId) {
    const frag = document.createDocumentFragment();

    for (const section of window.DOCS.sections) {
      const visiblePages = section.pages.filter((p) => matchesQuery(p, section));
      if (query && visiblePages.length === 0) continue;

      const wrap = document.createElement("div");
      wrap.className = "nav-section";
      if (section.collapsedByDefault && !query && !section.pages.some((p) => p.id === activeId)) {
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
      for (const page of section.pages) {
        const a = document.createElement("a");
        a.className = "nav-link";
        a.href = `#/${lang}/${page.id}`;
        a.textContent = txt(page.title);
        a.dataset.pageId = page.id;
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

  function renderHome() {
    const cards = window.DOCS.sections
      .map((section) => {
        const first = section.pages[0];
        if (!first) return "";
        return `
          <a class="feature-card" href="#/${lang}/${first.id}">
            <strong>${escapeHtml(txt(section.title))}</strong>
            <span>${escapeHtml(txt(section.summary || first.summary))}</span>
            <span class="path">${section.pages.length} ${lang === "fa" ? "صفحه" : "pages"}</span>
          </a>`;
      })
      .join("");

    els.content.innerHTML = `
      <section class="hero">
        <h1>${escapeHtml(t("homeTitle"))}</h1>
        <p>${escapeHtml(t("homeLead"))}</p>
      </section>
      <section class="card">
        <h2>${escapeHtml(t("browse"))}</h2>
        <div class="grid-cards">${cards}</div>
      </section>
      ${renderMaintenanceNote()}
    `;
    els.crumbs.innerHTML = `<strong>${escapeHtml(t("homeTitle"))}</strong>`;
    els.adminPathLink.hidden = true;
  }

  function renderMaintenanceNote() {
    const note = window.DOCS.meta && window.DOCS.meta.maintenance;
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

  function formatRich(value) {
    const raw = txt(value);
    if (!raw) return "";
    // Escape first, then allow a tiny safe subset: **bold**, `code`, and newlines.
    let html = escapeHtml(raw);
    html = html.replace(/\*\*(.+?)\*\*/g, "<strong>$1</strong>");
    html = html.replace(/`([^`]+)`/g, "<code>$1</code>");
    html = html.replace(/\n/g, "<br />");
    return html;
  }

  function renderParagraphs(value) {
    if (!value) return "";
    if (Array.isArray(value)) {
      return value.map((p) => `<p>${formatRich(p)}</p>`).join("");
    }
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

  function renderPage(section, page) {
    const steps = (page.howTo || []).map((s) => `<li>${formatRich(s)}</li>`).join("");
    const tips = (page.tips || []).map((s) => `<li>${formatRich(s)}</li>`).join("");
    const related = (page.related || [])
      .map((id) => {
        const hit = findPage(id);
        if (!hit) return "";
        return `<a class="feature-card" href="#/${lang}/${id}">
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
      ? `<section class="card">
          <h2>${escapeHtml(t("columns"))}</h2>
          ${renderDefinitions(page.columns)}
        </section>`
      : "";

    const exampleHtml = page.example
      ? `<section class="card">
          <h2>${escapeHtml(t("example"))}</h2>
          ${renderParagraphs(page.example.intro)}
          ${page.example.steps ? `<ol class="steps">${page.example.steps.map((s) => `<li>${formatRich(s)}</li>`).join("")}</ol>` : ""}
          ${page.example.result ? `<div class="callout ok">${formatRich(page.example.result)}</div>` : ""}
        </section>`
      : "";

    const extra = page.sections
      ? page.sections
          .map(
            (s) => `
        <section class="card">
          <h2>${formatRich(s.title)}</h2>
          ${renderParagraphs(s.body)}
          ${s.definitions ? renderDefinitions(s.definitions) : ""}
          ${
            s.items
              ? `<ul>${s.items.map((i) => `<li>${formatRich(i)}</li>`).join("")}</ul>`
              : ""
          }
          ${s.callout ? `<div class="callout ${s.callout.tone || ""}">${formatRich(s.callout.text)}</div>` : ""}
        </section>`
          )
          .join("")
      : "";

    els.content.innerHTML = `
      <section class="hero">
        <h1>${escapeHtml(txt(page.title))}</h1>
        <p>${formatRich(page.summary)}</p>
        <div class="meta-row">
          ${page.adminPath ? `<span class="chip">${escapeHtml(t("path"))}: <code>${escapeHtml(page.adminPath)}</code></span>` : ""}
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
          ? `<section class="card">
              <h2>${escapeHtml(t("howto"))}</h2>
              <ol class="steps">${steps}</ol>
            </section>`
          : ""
      }

      ${extra}

      ${
        tips
          ? `<section class="card">
              <h2>${escapeHtml(t("tips"))}</h2>
              <ul>${tips}</ul>
            </section>`
          : ""
      }

      ${
        related
          ? `<section class="card">
              <h2>${escapeHtml(t("related"))}</h2>
              <div class="grid-cards">${related}</div>
            </section>`
          : ""
      }
    `;

    els.crumbs.innerHTML = `${escapeHtml(txt(section.title))} / <strong>${escapeHtml(txt(page.title))}</strong>`;

    if (page.adminPath) {
      els.adminPathLink.hidden = false;
      els.adminPathCode.textContent = page.adminPath;
      els.adminPathLink.title = page.adminPath;
    } else {
      els.adminPathLink.hidden = true;
    }
  }

  function renderNotFound() {
    els.content.innerHTML = `
      <section class="not-found hero">
        <h1>${escapeHtml(t("notFound"))}</h1>
        <p>${escapeHtml(t("notFoundLead"))}</p>
      </section>`;
    els.crumbs.innerHTML = `<strong>${escapeHtml(t("notFound"))}</strong>`;
    els.adminPathLink.hidden = true;
  }

  function route() {
    const parsed = parseHash();
    if (parsed.lang && parsed.lang !== lang) {
      lang = parsed.lang;
    }
    applyChrome();

    const pageId = parsed.pageId || "home";
    if (!location.hash || location.hash === "#" || location.hash === "#/") {
      setHash("home", true);
    }

    if (pageId === "home") {
      renderNav("home");
      renderHome();
      closeSidebar();
      return;
    }

    const hit = findPage(pageId);
    renderNav(pageId);
    if (!hit) {
      renderNotFound();
    } else {
      renderPage(hit.section, hit.page);
    }
    closeSidebar();
    els.content.focus({ preventScroll: true });
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function escapeHtml(str) {
    return String(str)
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;");
  }

  function closeSidebar() {
    document.body.classList.remove("sidebar-open");
    els.backdrop.hidden = true;
  }

  function openSidebar() {
    document.body.classList.add("sidebar-open");
    els.backdrop.hidden = false;
  }

  els.langFa.addEventListener("click", () => {
    lang = "fa";
    const pageId = parseHash().pageId || "home";
    setHash(pageId);
    route();
  });
  els.langEn.addEventListener("click", () => {
    lang = "en";
    const pageId = parseHash().pageId || "home";
    setHash(pageId);
    route();
  });

  els.searchInput.addEventListener("input", (e) => {
    query = e.target.value.trim();
    const pageId = parseHash().pageId || "home";
    renderNav(pageId);
  });

  els.menuToggle.addEventListener("click", () => {
    if (document.body.classList.contains("sidebar-open")) closeSidebar();
    else openSidebar();
  });
  els.backdrop.addEventListener("click", closeSidebar);
  window.addEventListener("hashchange", route);

  if (!window.DOCS || !Array.isArray(window.DOCS.sections)) {
    els.content.innerHTML = `<div class="callout warn">Documentation catalog failed to load (catalog.js).</div>`;
    return;
  }

  route();
})();
