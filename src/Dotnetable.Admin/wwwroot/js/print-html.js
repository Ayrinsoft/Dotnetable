/**
 * Print / PDF from a rendered HTML region (not the full admin chrome via Ctrl+P).
 * Opens a clean document with the element's HTML + copied styles, then triggers print
 * so the user can save as PDF from the browser dialog.
 *
 * Always forces a light print surface (white background, near-black text) so dark mode
 * in the admin does not produce washed-out gray ink on paper / PDF.
 */
window.dotnetablePrint = window.dotnetablePrint || {
  /** Open an external URL (WhatsApp, mailto, …) without leaving the admin circuit. */
  openExternal: function (url) {
    if (!url) return;
    window.open(url, "_blank", "noopener,noreferrer");
  },

  /**
   * @param {string} elementId - id of the root element to print
   * @param {string} [title] - document title
   */
  printElement: function (elementId, title) {
    var el = document.getElementById(elementId);
    if (!el) {
      console.warn("dotnetablePrint: element not found:", elementId);
      return;
    }

    var docTitle = title || document.title || "Print";
    var clone = el.cloneNode(true);
    // Strip controls marked no-print inside the clone
    clone.querySelectorAll(".no-print").forEach(function (n) {
      n.parentNode && n.parentNode.removeChild(n);
    });
    // Drop dark-mode hooks so theme CSS does not keep a dark palette on the clone root
    clone.classList && clone.classList.remove("dark", "mud-theme-dark");
    clone.querySelectorAll(".dark, .mud-theme-dark").forEach(function (n) {
      n.classList.remove("dark", "mud-theme-dark");
    });
    if (clone.removeAttribute) {
      clone.removeAttribute("data-bs-theme");
      clone.removeAttribute("data-theme");
    }

    var styles = "";
    try {
      for (var i = 0; i < document.styleSheets.length; i++) {
        var sheet = document.styleSheets[i];
        try {
          var rules = sheet.cssRules || sheet.rules;
          if (!rules) continue;
          for (var j = 0; j < rules.length; j++) {
            styles += rules[j].cssText + "\n";
          }
        } catch (e) {
          // Cross-origin stylesheets — link them instead
          if (sheet.href) {
            styles += '@import url("' + sheet.href + '");\n';
          }
        }
      }
    } catch (e) {
      /* ignore */
    }

    // Appended LAST so it wins over MudBlazor / Bootstrap dark tokens copied above.
    var forceLightCss =
      "/* Force light print surface — always on top of theme CSS */" +
      "html,body,:root{" +
      "color-scheme:light!important;" +
      "background:#fff!important;" +
      "background-color:#fff!important;" +
      "color:#000!important;" +
      /* Bootstrap */
      "--bs-body-color:#000!important;" +
      "--bs-body-bg:#fff!important;" +
      "--bs-secondary-color:#111!important;" +
      "--bs-tertiary-color:#222!important;" +
      "--bs-emphasis-color:#000!important;" +
      "--bs-border-color:#333!important;" +
      /* MudBlazor palette tokens commonly used for text/surfaces */
      "--mud-palette-black:#000!important;" +
      "--mud-palette-white:#fff!important;" +
      "--mud-palette-text-primary:#000!important;" +
      "--mud-palette-text-secondary:#111!important;" +
      "--mud-palette-text-disabled:#333!important;" +
      "--mud-palette-action-default:#000!important;" +
      "--mud-palette-action-disabled:#444!important;" +
      "--mud-palette-surface:#fff!important;" +
      "--mud-palette-background:#fff!important;" +
      "--mud-palette-background-gray:#f5f5f5!important;" +
      "--mud-palette-drawer-background:#fff!important;" +
      "--mud-palette-appbar-background:#fff!important;" +
      "--mud-palette-lines-default:#444!important;" +
      "--mud-palette-lines-inputs:#444!important;" +
      "--mud-palette-divider:#444!important;" +
      "--mud-palette-table-lines:#444!important;" +
      "--mud-palette-table-striped:#f7f7f7!important;" +
      "--mud-palette-table-hover:#eee!important;" +
      "--mud-palette-gray-default:#111!important;" +
      "--mud-palette-gray-light:#333!important;" +
      "--mud-palette-gray-lighter:#555!important;" +
      "--mud-palette-overlay-dark:transparent!important;" +
      "--mud-palette-overlay-light:transparent!important;" +
      "}" +
      "html,body{" +
      "margin:0!important;" +
      "padding:16px!important;" +
      "font-family:system-ui,-apple-system,'Segoe UI',Roboto,sans-serif!important;" +
      "background:#fff!important;" +
      "color:#000!important;" +
      "}" +
      /* Kill inherited gray/faded ink from dark theme utilities */
      "body,body *{" +
      "color:#000!important;" +
      "caret-color:#000!important;" +
      "-webkit-text-fill-color:#000!important;" +
      "text-shadow:none!important;" +
      "opacity:1!important;" +
      "-webkit-print-color-adjust:exact!important;" +
      "print-color-adjust:exact!important;" +
      "}" +
      "html,body{background:#fff!important;background-color:#fff!important;}" +
      "a,a:visited,a:hover{color:#000!important;text-decoration:underline;}" +
      "table{border-collapse:collapse;width:100%;}" +
      ".text-end{text-align:right!important;}" +
      ".no-print{display:none!important;}" +
      /* Structured customer documents — keep borders, padding, light fills */
      ".print-doc-sheet,.label-sheet{" +
      "background:#fff!important;color:#000!important;" +
      "border:1px solid #111!important;padding:32px 36px!important;" +
      "max-width:820px!important;margin:0 auto!important;border-radius:0!important;" +
      "box-shadow:none!important;" +
      "}" +
      ".label-sheet{border:2px solid #111!important;max-width:440px!important;}" +
      ".print-doc-header{border-bottom:2px solid #111!important;padding-bottom:16px!important;margin-bottom:20px!important;" +
      "display:flex!important;justify-content:space-between!important;gap:12px!important;}" +
      ".print-doc-title{font-size:1.5rem!important;font-weight:700!important;margin:0 0 4px!important;}" +
      ".print-doc-subtitle{font-size:1.05rem!important;font-weight:600!important;margin:0 0 6px!important;}" +
      ".print-doc-meta{font-size:.88rem!important;margin:0!important;}" +
      ".print-doc-badge{border:1px solid #111!important;padding:4px 10px!important;font-size:.75rem!important;font-weight:700!important;}" +
      ".print-doc-parties{display:grid!important;grid-template-columns:1fr 1fr!important;gap:14px!important;margin-bottom:20px!important;}" +
      ".print-doc-party{border:1px solid #222!important;padding:12px 14px!important;}" +
      ".print-doc-party-label{font-size:.72rem!important;font-weight:700!important;text-transform:uppercase!important;" +
      "border-bottom:1px solid #ccc!important;padding-bottom:6px!important;margin:0 0 8px!important;}" +
      ".print-doc-party-name{font-weight:700!important;margin:0 0 8px!important;}" +
      ".print-doc-party table,.print-doc-party td{border:none!important;padding:2px 0!important;}" +
      ".print-doc-lines th{background:#f0f2f4!important;border-top:1px solid #111!important;" +
      "border-bottom:2px solid #111!important;padding:10px!important;font-weight:700!important;}" +
      ".print-doc-lines td{border-bottom:1px solid #ccc!important;padding:10px!important;}" +
      ".print-doc-footer-grid{display:grid!important;grid-template-columns:1fr 280px!important;gap:16px!important;}" +
      ".print-doc-notes{border:1px dashed #888!important;padding:12px!important;font-size:.85rem!important;}" +
      ".print-doc-totals{border:1px solid #111!important;}" +
      ".print-doc-totals td{padding:8px 12px!important;border:none!important;border-bottom:1px solid #e0e0e0!important;}" +
      ".print-doc-totals tr:last-child td{border-bottom:none!important;border-top:2px solid #111!important;font-weight:700!important;}" +
      ".print-doc-stamp{margin-top:24px!important;padding-top:12px!important;border-top:1px solid #ccc!important;" +
      "text-align:center!important;font-size:.78rem!important;}" +
      ".label-order{border-bottom:1px solid #ccc!important;padding-bottom:10px!important;margin-bottom:12px!important;}" +
      ".label-address{border-top:1px dashed #bbb!important;border-bottom:1px dashed #bbb!important;padding:12px 0!important;}" +
      "@media print{" +
      "html,body{padding:0!important;background:#fff!important;color:#000!important;}" +
      "body,body *{color:#000!important;-webkit-text-fill-color:#000!important;opacity:1!important;}" +
      ".print-doc-sheet,.label-sheet{border-color:#000!important;}" +
      "}";

    var html =
      "<!DOCTYPE html><html lang=\"en\" data-bs-theme=\"light\" class=\"\">" +
      "<head><meta charset=\"utf-8\"/>" +
      "<meta name=\"color-scheme\" content=\"light only\"/>" +
      "<meta name=\"supported-color-schemes\" content=\"light\"/>" +
      "<title>" +
      escapeHtml(docTitle) +
      "</title>" +
      "<style>" +
      styles +
      "\n" +
      forceLightCss +
      "</style></head>" +
      "<body class=\"\" data-bs-theme=\"light\" style=\"background:#fff;color:#000;\">" +
      clone.innerHTML +
      "</body></html>";

    var w = window.open("", "_blank");
    if (!w) {
      // Popup blocked — fall back to in-page print of marked region only
      window.print();
      return;
    }
    w.document.open();
    w.document.write(html);
    w.document.close();
    // Ensure the popup document itself is not treated as dark by the browser
    try {
      w.document.documentElement.classList.remove("dark", "mud-theme-dark");
      w.document.documentElement.setAttribute("data-bs-theme", "light");
      w.document.documentElement.style.colorScheme = "light";
      if (w.document.body) {
        w.document.body.classList.remove("dark", "mud-theme-dark");
        w.document.body.setAttribute("data-bs-theme", "light");
        w.document.body.style.background = "#fff";
        w.document.body.style.color = "#000";
      }
    } catch (e) {
      /* ignore */
    }
    w.focus();
    // Wait for styles/images
    setTimeout(function () {
      try {
        w.print();
      } catch (e) {
        console.warn(e);
      }
    }, 300);
  },
};

function escapeHtml(s) {
  return String(s)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}
