// Injects RTL / LTR buttons into the CKEditor 5 toolbar and wraps full HTML in
// <div dir="…" style="direction:…">…</div> via the live editor instance.
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

        // Capture phase so CK toolbar does not swallow the event.
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

    var OUTER_DIR_RE = /^\s*<div\b(?=[^>]*\b(?:style\s*=\s*["'][^"']*\bdirection\s*:\s*(?:rtl|ltr)|dir\s*=\s*["'](?:rtl|ltr)["']))[^>]*>\s*([\s\S]*)\s*<\/div>\s*$/i;

    function unwrapDirection(html) {
        if (!html) return '';
        var m = String(html).match(OUTER_DIR_RE);
        return m ? m[1] : html;
    }

    function wrapDirection(html, dir) {
        var inner = unwrapDirection(html || '');
        var textOnly = String(inner).replace(/<[^>]*>/g, '').replace(/&nbsp;/gi, ' ').trim();
        if (!textOnly && !/<(img|table|figure|iframe|video|ul|ol)\b/i.test(inner)) {
            inner = '<p>&nbsp;</p>';
        }
        // Both dir= and style= — some sanitizers keep one or the other.
        return '<div dir="' + dir + '" style="direction:' + dir + '">' + inner + '</div>';
    }

    function notifyBlazor(st, html, dir) {
        if (!st || !st.dotNetRef) return;
        st.dotNetRef.invokeMethodAsync('NotifyDirectionApplied', html, dir)
            .catch(function (err) {
                console.error('[dnCkEditor] NotifyDirectionApplied failed', err);
                return st.dotNetRef.invokeMethodAsync('ApplyDirectionFromJs', dir);
            })
            .catch(function (err2) {
                console.error('[dnCkEditor] ApplyDirectionFromJs failed', err2);
            });
    }

    function applyDirection(host, dir) {
        dir = (dir === 'rtl') ? 'rtl' : 'ltr';
        var st = stateOf(host);
        var editor = getEditor(host);

        if (!editor) {
            console.warn('[dnCkEditor] ckeditorInstance not found — falling back to Blazor path.');
            if (st && st.dotNetRef) {
                st.dotNetRef.invokeMethodAsync('ApplyDirectionFromJs', dir)
                    .catch(function (err) { console.error('[dnCkEditor] fallback failed', err); });
            }
            return false;
        }

        var current = '';
        try { current = editor.getData() || ''; }
        catch (e) { console.error('[dnCkEditor] getData failed', e); }

        var wrapped = wrapDirection(current, dir);

        try {
            editor.setData(wrapped);
        } catch (e) {
            console.error('[dnCkEditor] setData failed', e);
            return false;
        }

        // Immediate visual on the editing root (independent of data pipeline).
        try {
            var view = editor.editing && editor.editing.view;
            if (view) {
                view.change(function (writer) {
                    var root = view.document.getRoot();
                    if (!root) return;
                    writer.setAttribute('dir', dir, root);
                    writer.setStyle('direction', dir, root);
                });
            }
        } catch (e) { /* non-fatal */ }

        // If the schema kept the wrapper, change:data will sync Blazor — do not push a
        // second HTML string (that races Value→SetData and jumps the caret while typing).
        // Only force-notify when the wrapper was stripped but we still want it in the model.
        applyActive(host, dir);
        try {
            var saved = editor.getData() || wrapped;
            var kept =
                /direction\s*:\s*(rtl|ltr)/i.test(saved) ||
                /\bdir\s*=\s*["'](rtl|ltr)["']/i.test(saved);
            if (!kept) {
                console.warn('[dnCkEditor] Schema dropped direction wrapper; pushing wrapped HTML to Blazor model only.');
                notifyBlazor(st, wrapped, dir);
            }
            // when kept: rely on change:data only
        } catch (e) {
            notifyBlazor(st, wrapped, dir);
        }
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
        /**
         * Safe to call on every Blazor render — does NOT tear down existing buttons.
         */
        attachDirectionToolbar: function (host, labels, active, dotNetRef) {
            if (!host) return;

            var prev = stateOf(host);
            if (prev && !prev.disposed) {
                // Keep the same lifecycle; only refresh callback + labels + pressed state.
                prev.labels = labels || prev.labels;
                prev.active = (active || '').toLowerCase();
                if (dotNetRef) prev.dotNetRef = dotNetRef;
                applyActive(host, prev.active);
                if (!host.querySelector('.dn-ck-dir-group')) {
                    schedule(host);
                }
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

            // Debounced: subtree mutations fire on every keystroke; only re-inject if buttons gone.
            var st = host._dnCkEditorState;
            var debounce = null;
            st.observer = new MutationObserver(function () {
                if (st.disposed) return;
                if (host.querySelector('.dn-ck-dir-group')) return;
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
