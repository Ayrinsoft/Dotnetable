// Shared light/dark theme persistence for both the Blazor admin and the
// Razor-Pages auth screens. Stored in localStorage (fast read) and a cookie
// (so server-rendered auth pages can pick it up before first paint).
window.dotnetableTheme = {
    get: function () {
        try { return localStorage.getItem('dn-theme') || ''; } catch { return ''; }
    },
    set: function (value) {
        try { localStorage.setItem('dn-theme', value); } catch (e) { }
        document.cookie = 'dn-theme=' + value + ';path=/;max-age=31536000;samesite=lax';
    },
    // Used by the auth pages' inline toggle button.
    toggle: function () {
        var next = (document.documentElement.classList.contains('dark')) ? 'light' : 'dark';
        document.documentElement.classList.toggle('dark', next === 'dark');
        window.dotnetableTheme.set(next);
        return next;
    }
};

// Admin panel color skin (MudBlazor palette preset). Browser-local only.
window.dotnetableAdminSkin = {
    get: function () {
        try { return localStorage.getItem('dn-admin-skin') || ''; } catch { return ''; }
    },
    set: function (value) {
        try { localStorage.setItem('dn-admin-skin', value || ''); } catch (e) { }
    }
};

// Active UI language for both the Blazor admin and the Razor-Pages auth screens. Same
// localStorage+cookie pattern as dotnetableTheme so pre-render (auth pages, PageLocalizer)
// can read it server-side while the client keeps a fast local copy.
window.dotnetableLang = {
    get: function () {
        try { return localStorage.getItem('dn-lang') || ''; } catch { return ''; }
    },
    set: function (value) {
        try { localStorage.setItem('dn-lang', value); } catch (e) { }
        document.cookie = 'dn-lang=' + value + ';path=/;max-age=31536000;samesite=lax';
    },
    // Sets dir/lang on the real <html> element so RTL reaches document.body-portaled content too
    // (MudBlazor dialogs/menus/snackbars render outside the Blazor component's own DOM subtree).
    // Called once the Blazor circuit has resolved the language from the DB (see MainLayout).
    setDir: function (rtl, lang) {
        document.documentElement.setAttribute('dir', rtl ? 'rtl' : 'ltr');
        if (lang) document.documentElement.setAttribute('lang', lang);
    }
};

// Session-local override of the admin UI mode (Basic/General/Advanced) — display-only, never
// synced back to the member's stored AdminUIMode. Persisted in localStorage so it survives a
// reload of the same browser but is otherwise independent per device.
window.dotnetableUiMode = {
    get: function () {
        try { return localStorage.getItem('dn-uimode') || ''; } catch { return ''; }
    },
    set: function (value) {
        try { localStorage.setItem('dn-uimode', value); } catch (e) { }
    }
};

// Dismissible admin info/hint alerts. Key is namespaced under dn-dismiss: so product-edit.media
// becomes localStorage['dn-dismiss:product-edit.media'] = '1'.
window.dotnetableDismiss = {
    isDismissed: function (key) {
        if (!key) return false;
        try { return localStorage.getItem('dn-dismiss:' + key) === '1'; } catch { return false; }
    },
    dismiss: function (key) {
        if (!key) return;
        try { localStorage.setItem('dn-dismiss:' + key, '1'); } catch (e) { }
    }
};

// Triggers a browser download of base64 content produced server-side
// (used by the Translations page to export a CSV file).
window.dotnetableFile = {
    download: function (fileName, base64, mime) {
        var bytes = atob(base64);
        var buffer = new Uint8Array(bytes.length);
        for (var i = 0; i < bytes.length; i++) buffer[i] = bytes.charCodeAt(i);
        var blob = new Blob([buffer], { type: mime || 'application/octet-stream' });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },

    /**
     * Trigger a browser download for a same-origin (or CORS-enabled) URL.
     * Synchronous entry point for Blazor interop — never throws; work runs async.
     * Prefer Admin's /media/download/{id} proxy so CDN CORS is not required.
     */
    downloadUrl: function (url, fileName) {
        var name = fileName || 'download';
        // Same-origin paths (e.g. /media/download/123) work best with credentials + <a download>.
        try {
            var a = document.createElement('a');
            a.href = url;
            a.download = name;
            a.rel = 'noopener noreferrer';
            // Keep the user on the page; Content-Disposition: attachment does the rest.
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
        } catch (e) {
            // Last resort: navigate the top window (still usually downloads with attachment header).
            try { window.location.assign(url); } catch (_) { /* ignore */ }
        }
    }
};
