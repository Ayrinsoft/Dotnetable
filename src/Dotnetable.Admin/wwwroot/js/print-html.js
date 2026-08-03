/**
 * Print / PDF from a rendered HTML region (not the full admin chrome via Ctrl+P).
 * Opens a clean document with the element's HTML + copied styles, then triggers print
 * so the user can save as PDF from the browser dialog.
 */
window.dotnetablePrint = window.dotnetablePrint || {
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

    var html =
      "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/>" +
      "<title>" +
      escapeHtml(docTitle) +
      "</title>" +
      "<style>" +
      "html,body{margin:0;padding:16px;background:#fff;color:#111;}" +
      "body{font-family:system-ui,-apple-system,'Segoe UI',Roboto,sans-serif;}" +
      "table{border-collapse:collapse;width:100%;}" +
      "th,td{border-bottom:1px solid #ddd;padding:6px 8px;text-align:left;}" +
      "th{font-weight:600;}" +
      ".text-end{text-align:right;}" +
      ".no-print{display:none!important;}" +
      "@media print{body{padding:0;}}" +
      styles +
      "</style></head><body>" +
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
