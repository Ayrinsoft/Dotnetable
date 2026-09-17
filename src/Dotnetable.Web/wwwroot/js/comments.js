/* Comment thread on blog posts and CMS pages (Views/Shared/_Comments.cshtml):
   "load more", reply targeting, captcha for guests and the AJAX submit. */
(function () {
    var root = document.getElementById('comments');
    if (!root) return;

    var target = root.dataset.target;
    var targetId = root.dataset.targetId;
    var loggedIn = root.dataset.loggedIn === 'true';
    var page = parseInt(root.dataset.page, 10) || 1;
    var pageSize = parseInt(root.dataset.pageSize, 10) || 20;
    var total = parseInt(root.dataset.total, 10) || 0;

    var list = document.getElementById('comments-list');
    var moreBtn = document.getElementById('comments-more');
    var form = document.getElementById('comment-form');
    var alertBox = document.getElementById('comment-alert');
    var captchaHost = document.getElementById('comment-captcha');
    var submitBtn = document.getElementById('comment-submit');
    var replyBanner = document.getElementById('comment-reply-banner');
    var replyName = document.getElementById('comment-reply-name');
    var parentInput = form.querySelector('input[name="parentCommentId"]');

    var challenge = null;
    var turnstileWidgetId = null;
    var turnstileScriptPromise = null;

    function escapeHtml(s) {
        var div = document.createElement('div');
        div.textContent = s == null ? '' : String(s);
        return div.innerHTML;
    }

    // escapeHtml leaves quotes alone, which is fine for text but not inside an attribute value.
    function escapeAttr(s) {
        return escapeHtml(s).replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }

    function showAlert(message, kind) {
        alertBox.innerHTML = message
            ? '<div class="alert alert-' + (kind || 'danger') + ' mb-3">' + escapeHtml(message) + '</div>'
            : '';
    }

    // ── Rendering (mirrors _CommentItem.cshtml) ─────────────────────

    function formatDate(iso) {
        var d = new Date(iso);
        return isNaN(d) ? '' : d.toLocaleDateString(undefined, { year: 'numeric', month: 'long', day: 'numeric' });
    }

    function renderComment(c, depth) {
        var initial = c.authorName ? c.authorName.charAt(0).toUpperCase() : '?';
        var replies = (c.replies || []).map(function (r) { return renderComment(r, depth + 1); }).join('');
        return '' +
            '<div class="comment d-flex gap-3 mt-3" id="comment-' + c.commentID + '">' +
            (c.authorAvatarUrl
                ? '  <img src="' + escapeAttr(c.authorAvatarUrl) + '" alt="' + escapeAttr(c.authorName) + '" class="comment-avatar comment-avatar-staff flex-shrink-0" loading="lazy" />'
                : '  <div class="comment-avatar flex-shrink-0' + (c.isStaff ? ' comment-avatar-staff' : '') + '">' + escapeHtml(initial) + '</div>') +
            '  <div class="flex-grow-1" style="min-width:0">' +
            '    <div class="d-flex flex-wrap gap-2 align-items-center">' +
            '      <strong>' + escapeHtml(c.authorName) + '</strong>' +
            (c.isStaff ? '<span class="badge bg-primary">Staff</span>' : '') +
            '      <small class="text-muted">' + escapeHtml(formatDate(c.createdAt)) + '</small>' +
            '    </div>' +
            '    <p class="comment-body mb-1">' + escapeHtml(c.body) + '</p>' +
            '    <button type="button" class="btn btn-link btn-sm p-0 text-decoration-none comment-reply"' +
            '            data-comment-id="' + c.commentID + '" data-author="' + escapeAttr(c.authorName) + '">' +
            '      <i class="bi bi-reply me-1"></i>Reply</button>' +
            (replies ? '<div class="comment-replies' + (depth < 3 ? ' comment-replies-indent' : '') + '">' + replies + '</div>' : '') +
            '  </div>' +
            '</div>';
    }

    if (moreBtn) {
        moreBtn.addEventListener('click', function () {
            moreBtn.disabled = true;
            var url = '/Comments/List?target=' + encodeURIComponent(target) +
                '&targetId=' + encodeURIComponent(targetId) + '&page=' + (page + 1);
            fetch(url, { headers: { 'Accept': 'application/json' } })
                .then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
                .then(function (result) {
                    page += 1;
                    var items = result.items || [];
                    list.insertAdjacentHTML('beforeend', items.map(function (c) { return renderComment(c, 0); }).join(''));
                    moreBtn.disabled = false;
                    if (items.length === 0 || page * pageSize >= total) moreBtn.classList.add('d-none');
                })
                .catch(function () { moreBtn.disabled = false; });
        });
    }

    // ── Reply targeting ─────────────────────────────────────────────

    function clearReply() {
        parentInput.value = '';
        replyBanner.classList.add('d-none');
    }

    list.addEventListener('click', function (event) {
        var btn = event.target.closest('.comment-reply');
        if (!btn) return;
        parentInput.value = btn.dataset.commentId;
        replyName.textContent = btn.dataset.author || '';
        replyBanner.classList.remove('d-none');
        document.getElementById('comment-form-wrap').scrollIntoView({ behavior: 'smooth', block: 'start' });
        document.getElementById('comment-body').focus({ preventScroll: true });
    });

    document.getElementById('comment-reply-cancel').addEventListener('click', clearReply);

    // ── Captcha (guests only; same challenge endpoint as the contact form) ──

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
            captchaHost.appendChild(box);
            loadTurnstileScript().then(function () {
                turnstileWidgetId = window.turnstile.render(box, { sitekey: c.siteKey });
            }).catch(fetchCaptcha);
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
        if (!captchaHost) return;
        fetch('/Home/ContactCaptcha', { headers: { 'Accept': 'application/json' } })
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
        if (!captchaHost) return;
        if (challenge && challenge.provider === 'turnstile' && window.turnstile && turnstileWidgetId != null) {
            window.turnstile.reset(turnstileWidgetId);
        } else {
            fetchCaptcha();
        }
    }

    // ── Submit ──────────────────────────────────────────────────────

    form.addEventListener('submit', function (event) {
        event.preventDefault();
        showAlert(null);

        var data = new FormData(form);
        var body = (data.get('body') || '').trim();
        var name = (data.get('authorName') || '').trim();
        if (!body || (!loggedIn && !name)) {
            showAlert(loggedIn ? 'Please write a comment.' : 'Please enter your name and a comment.');
            return;
        }

        var payload = {
            body: body,
            parentCommentId: parentInput.value ? parseInt(parentInput.value, 10) : null,
            authorName: name || null,
            authorEmail: (data.get('authorEmail') || '').trim() || null,
            website: data.get('website') || '',
            captchaToken: captchaToken(),
            captchaAnswer: data.get('captchaAnswer') || '',
        };

        var url = '/Comments/Submit?target=' + encodeURIComponent(target) + '&targetId=' + encodeURIComponent(targetId);
        submitBtn.disabled = true;
        fetch(url, {
            method: 'POST',
            headers: window.dnAntiforgeryHeaders({ 'Content-Type': 'application/json' }),
            body: JSON.stringify(payload),
        })
            .then(function (r) { return r.json().catch(function () { return {}; }).then(function (b) { return { ok: r.ok, status: r.status, body: b }; }); })
            .then(function (result) {
                submitBtn.disabled = false;
                if (!result.ok) {
                    var msg = result.body.message || 'Could not post your comment. Please try again.';
                    if (loggedIn && (result.status === 401 || result.status === 403)) msg += ' Please sign in again.';
                    showAlert(msg);
                    resetCaptcha();
                    return;
                }

                showAlert(result.body.message || 'Thanks! Your comment will appear after it has been reviewed.', 'success');
                form.reset();
                clearReply();
                resetCaptcha();
            })
            .catch(function () {
                submitBtn.disabled = false;
                showAlert('Service is unavailable. Please try again later.');
            });
    });

    fetchCaptcha();
})();
