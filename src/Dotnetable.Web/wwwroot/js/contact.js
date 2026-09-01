(function () {
    var PENDING_KEY = 'dotnetable:contact:pending';
    var PENDING_MS = 24 * 60 * 60 * 1000;

    var form = document.getElementById('contact-form');
    if (!form) return;
    var pendingBox = document.getElementById('contact-pending');
    var alertBox = document.getElementById('contact-alert');
    var captchaHost = document.getElementById('contact-captcha');
    var submitBtn = document.getElementById('contact-submit');

    var challenge = null;      // last fetched captcha challenge
    var turnstileWidgetId = null;
    var turnstileScriptPromise = null;

    function showAlert(message, kind) {
        alertBox.innerHTML = message
            ? '<div class="alert alert-' + (kind || 'danger') + ' mb-3">' + escapeHtml(message) + '</div>'
            : '';
    }

    function escapeHtml(s) {
        var div = document.createElement('div');
        div.textContent = s;
        return div.innerHTML;
    }

    function readPending() {
        try {
            var raw = localStorage.getItem(PENDING_KEY);
            if (!raw) return null;
            var data = JSON.parse(raw);
            if (!data || !data.submittedAt || Date.now() - data.submittedAt > PENDING_MS) {
                localStorage.removeItem(PENDING_KEY);
                return null;
            }
            return data;
        } catch (e) { return null; }
    }

    function renderPending(data) {
        document.getElementById('contact-pending-subject').textContent = data.fields.messageSubject || '(no subject)';
        document.getElementById('contact-pending-meta').textContent =
            data.fields.senderName + ' — ' + new Date(data.submittedAt).toLocaleString();
        document.getElementById('contact-pending-message').textContent = data.fields.messageBody;
        form.classList.add('d-none');
        pendingBox.classList.remove('d-none');
    }

    function loadTurnstileScript() {
        if (window.turnstile) return Promise.resolve();
        if (turnstileScriptPromise) return turnstileScriptPromise;
        turnstileScriptPromise = new Promise(function (resolve, reject) {
            var s = document.createElement('script');
            s.src = 'https://challenges.cloudflare.com/turnstile/v0/api.js';
            s.async = true;
            s.defer = true;
            s.onload = resolve;
            s.onerror = reject;
            document.head.appendChild(s);
        });
        return turnstileScriptPromise;
    }

    function renderCaptcha(c) {
        challenge = c;
        captchaHost.innerHTML = '';

        if (c.provider === 'turnstile' && c.siteKey) {
            var box = document.createElement('div');
            box.id = 'turnstile-box';
            captchaHost.appendChild(box);

            loadTurnstileScript().then(function () {
                turnstileWidgetId = window.turnstile.render(box, { sitekey: c.siteKey });
            }).catch(function () {
                // Cloudflare unreachable — ask the server for a fresh (math) challenge instead.
                fetchCaptcha();
            });
            return;
        }

        var wrap = document.createElement('div');
        wrap.innerHTML =
            '<label class="form-label">Solve the captcha</label>' +
            '<div class="d-flex align-items-center gap-2">' +
            '  <span>' + c.svg + '</span>' +
            '  <input type="text" class="form-control" style="max-width:100px" name="captchaAnswer" inputmode="numeric" autocomplete="off" placeholder="?" />' +
            '</div>';
        captchaHost.appendChild(wrap);
    }

    function fetchCaptcha() {
        return fetch('/Home/ContactCaptcha', { headers: { 'Accept': 'application/json' } })
            .then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
            .then(renderCaptcha)
            .catch(function () {
                captchaHost.innerHTML = '<div class="text-danger small">Could not load the verification challenge. Please reload the page.</div>';
            });
    }

    function captchaToken() {
        if (challenge && challenge.provider === 'turnstile') {
            return window.turnstile && turnstileWidgetId != null ? window.turnstile.getResponse(turnstileWidgetId) : null;
        }
        return challenge ? challenge.token : null;
    }

    function resetCaptcha() {
        if (challenge && challenge.provider === 'turnstile' && window.turnstile && turnstileWidgetId != null) {
            window.turnstile.reset(turnstileWidgetId);
        } else {
            fetchCaptcha();
        }
    }

    form.addEventListener('submit', function (event) {
        event.preventDefault();
        showAlert(null);

        var data = new FormData(form);
        var fields = {
            senderName: (data.get('senderName') || '').trim(),
            emailAddress: (data.get('emailAddress') || '').trim(),
            cellphoneNumber: (data.get('cellphoneNumber') || '').trim(),
            messageSubject: (data.get('messageSubject') || '').trim(),
            messageBody: (data.get('messageBody') || '').trim(),
        };

        var payload = Object.assign({}, fields, {
            website: data.get('website') || '',
            captchaToken: captchaToken(),
            captchaAnswer: data.get('captchaAnswer') || '',
        });

        submitBtn.disabled = true;
        fetch('/Home/ContactSubmit', {
            method: 'POST',
            headers: window.dnAntiforgeryHeaders({ 'Content-Type': 'application/json' }),
            body: JSON.stringify(payload),
        })
            .then(function (r) { return r.json().catch(function () { return {}; }).then(function (body) { return { ok: r.ok, body: body }; }); })
            .then(function (result) {
                submitBtn.disabled = false;
                if (!result.ok) {
                    showAlert(result.body.message || 'Could not send your message. Please try again.');
                    resetCaptcha();
                    return;
                }

                try {
                    localStorage.setItem(PENDING_KEY, JSON.stringify({ fields: fields, submittedAt: Date.now() }));
                } catch (e) { /* storage unavailable — the success message still shows this once */ }
                renderPending({ fields: fields, submittedAt: Date.now() });
            })
            .catch(function () {
                submitBtn.disabled = false;
                showAlert('Service is unavailable. Please try again later.');
            });
    });

    var pending = readPending();
    if (pending) {
        renderPending(pending);
    } else {
        fetchCaptcha();
    }
})();
