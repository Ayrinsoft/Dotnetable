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
