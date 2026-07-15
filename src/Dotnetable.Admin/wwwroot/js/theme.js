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
    }
};
