// RTL / LTR toolbar buttons for DNCkEditor.
// Click → Blazor wraps bound save HTML as <div dir="…" style="direction:…">…</div>
// Visual dir on the editable is separate (so the editor can stay unwrapped while typing).
window.dnCkEditor = (function () {
    var MAX_TRIES = 80;
    var RETRY_MS = 100;

    function stateOf(host) {
        return host && host._dnCkEditorState;
    }

    function clearTimers(st) {
        if (!st) return;
        if (st.timer) {
            clearTimeout(st.timer);
            st.timer = null;
        }
        if (st.observer) {
            try { st.observer.disconnect(); } catch (e) { /* ignore */ }
            st.observer = null;
        }
    }

    function setPressed(btn, on) {
        if (!btn) return;
        btn.setAttribute('aria-pressed', on ? 'true' : 'false');
        btn.classList.toggle('ck-on', !!on);
        btn.classList.toggle('ck-off', !on);
    }

    function applyActive(host, active) {
        var st = stateOf(host);
        if (!st) return;
        st.active = (active || '').toLowerCase();
        var group = host.querySelector('.dn-ck-dir-group');
        if (!group) return;
        group.querySelectorAll('.dn-ck-dir-btn').forEach(function (btn) {
            setPressed(btn, btn.getAttribute('data-dn-dir') === st.active);
        });
    }

    function getEditor(host) {
        if (!host) return null;
        var editable = host.querySelector('.ck-editor__editable');
        if (editable && editable.ckeditorInstance) return editable.ckeditorInstance;

        var nodes = host.querySelectorAll('.ck-editor__editable, textarea, .ck-source-editing-area, .ck-editor');
        for (var i = 0; i < nodes.length; i++) {
            if (nodes[i].ckeditorInstance) return nodes[i].ckeditorInstance;
        }
        var all = host.querySelectorAll('*');
        for (var j = 0; j < all.length; j++) {
            if (all[j].ckeditorInstance) return all[j].ckeditorInstance;
        }
        return null;
    }

    function setVisualDirection(host, dir) {
        dir = (dir === 'rtl') ? 'rtl' : (dir === 'ltr' ? 'ltr' : '');
        var editor = getEditor(host);
        if (!editor || !dir) return false;
        try {
            var view = editor.editing && editor.editing.view;
            if (!view) return false;
            view.change(function (writer) {
                var root = view.document.getRoot();
                if (!root) return;
                writer.setAttribute('dir', dir, root);
                writer.setStyle('direction', dir, root);
            });
            return true;
        } catch (e) {
            console.warn('[dnCkEditor] applyVisualDirection failed', e);
            return false;
        }
    }

    function makeSeparator() {
        var sep = document.createElement('span');
        sep.className = 'ck ck-toolbar__separator dn-ck-dir-sep';
        return sep;
    }

    function makeButton(dir, label, title, active, onClick) {
        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'ck ck-button ck-button_with-text dn-ck-dir-btn ' + (active === dir ? 'ck-on' : 'ck-off');
        btn.setAttribute('data-dn-dir', dir);
        btn.setAttribute('tabindex', '-1');
        btn.setAttribute('aria-label', title || label);
        btn.setAttribute('data-cke-tooltip-text', title || label);
        btn.setAttribute('aria-pressed', active === dir ? 'true' : 'false');
        btn.title = title || label;

        var text = document.createElement('span');
        text.className = 'ck ck-button__label';
        text.textContent = label;
        btn.appendChild(text);

        var fire = function (e) {
            e.preventDefault();
            e.stopPropagation();
            if (e.stopImmediatePropagation) e.stopImmediatePropagation();
            onClick(dir);
        };
        btn.addEventListener('mousedown', fire, true);
        btn.addEventListener('click', fire, true);
        return btn;
    }

    function applyDirection(host, dir) {
        dir = (dir === 'rtl') ? 'rtl' : 'ltr';
        var st = stateOf(host);
        var editor = getEditor(host);
        var html = '';

        if (editor) {
            try { html = editor.getData() || ''; }
            catch (e) { console.error('[dnCkEditor] getData failed', e); }
            setVisualDirection(host, dir);
        } else {
            console.warn('[dnCkEditor] no editor instance — Blazor will use its buffer');
        }

        applyActive(host, dir);

        if (!st || !st.dotNetRef) {
            console.error('[dnCkEditor] missing DotNet ref — cannot update save HTML');
            return false;
        }

        // Always notify Blazor so the *bound model* gets the wrapper for Save → DB.
        st.dotNetRef.invokeMethodAsync('NotifyDirectionApplied', html, dir)
            .then(function () {
                console.debug('[dnCkEditor] direction applied for save:', dir);
            })
            .catch(function (err) {
                console.error('[dnCkEditor] NotifyDirectionApplied failed', err);
                return st.dotNetRef.invokeMethodAsync('ApplyDirectionFromJs', dir);
            })
            .catch(function (err2) {
                console.error('[dnCkEditor] ApplyDirectionFromJs failed', err2);
            });
        return true;
    }

    function findToolbarItems(host) {
        var items = host.querySelector('.ck-toolbar__items');
        if (items) return items;
        var toolbar = host.querySelector('.ck-toolbar');
        return toolbar ? toolbar.querySelector('.ck-toolbar__items') : null;
    }

    function inject(host) {
        var st = stateOf(host);
        if (!st || st.disposed) return true;

        var items = findToolbarItems(host);
        if (!items) return false;

        if (items.querySelector('.dn-ck-dir-group')) {
            applyActive(host, st.active);
            if (st.active) setVisualDirection(host, st.active);
            return true;
        }

        var labels = st.labels || {};
        var active = (st.active || '').toLowerCase();
        var group = document.createElement('span');
        group.className = 'ck dn-ck-dir-group';
        group.setAttribute('role', 'presentation');

        group.appendChild(makeSeparator());
        group.appendChild(makeButton('rtl', labels.rtl || 'RTL', labels.rtlTitle || 'Right to left', active, function (d) {
            applyDirection(host, d);
        }));
        group.appendChild(makeButton('ltr', labels.ltr || 'LTR', labels.ltrTitle || 'Left to right', active, function (d) {
            applyDirection(host, d);
        }));
        items.appendChild(group);

        if (active) setVisualDirection(host, active);
        return true;
    }

    function schedule(host) {
        var st = stateOf(host);
        if (!st || st.disposed) return;

        var tryInject = function () {
            if (!stateOf(host) || st.disposed) return;
            if (inject(host)) return;
            st.tries = (st.tries || 0) + 1;
            if (st.tries < MAX_TRIES) {
                st.timer = setTimeout(tryInject, RETRY_MS);
            } else {
                console.warn('[dnCkEditor] Toolbar not found after retries.');
            }
        };

        st.tries = 0;
        tryInject();
    }

    return {
        attachDirectionToolbar: function (host, labels, active, dotNetRef) {
            if (!host) return;

            var prev = stateOf(host);
            if (prev && !prev.disposed) {
                prev.labels = labels || prev.labels;
                prev.active = (active || '').toLowerCase();
                if (dotNetRef) prev.dotNetRef = dotNetRef;
                applyActive(host, prev.active);
                if (prev.active) setVisualDirection(host, prev.active);
                if (!host.querySelector('.dn-ck-dir-group')) schedule(host);
                return;
            }

            host._dnCkEditorState = {
                labels: labels || {},
                active: (active || '').toLowerCase(),
                dotNetRef: dotNetRef,
                tries: 0,
                timer: null,
                observer: null,
                disposed: false
            };

            schedule(host);

            var st = host._dnCkEditorState;
            var debounce = null;
            st.observer = new MutationObserver(function () {
                if (st.disposed) return;
                if (host.querySelector('.dn-ck-dir-group')) {
                    // Editor may appear after toolbar — re-apply visual dir.
                    if (st.active) setVisualDirection(host, st.active);
                    return;
                }
                if (!(host.querySelector('.ck-toolbar') || host.querySelector('.ck-editor'))) return;
                if (debounce) clearTimeout(debounce);
                debounce = setTimeout(function () {
                    if (!st.disposed && !host.querySelector('.dn-ck-dir-group')) {
                        schedule(host);
                    }
                }, 200);
            });
            st.observer.observe(host, { childList: true, subtree: true });
        },

        setActiveDirection: function (host, active) {
            applyActive(host, active);
        },

        applyVisualDirection: function (host, dir) {
            return setVisualDirection(host, dir);
        },

        applyDirection: function (host, dir) {
            return applyDirection(host, dir);
        },

        dispose: function (host) {
            if (!host) return;
            var st = stateOf(host);
            if (!st) return;
            st.disposed = true;
            clearTimers(st);
            try {
                host.querySelectorAll('.dn-ck-dir-group').forEach(function (el) { el.remove(); });
            } catch (e) { /* ignore */ }
            host._dnCkEditorState = null;
        }
    };
})();
