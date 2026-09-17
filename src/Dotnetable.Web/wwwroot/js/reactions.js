/* =====================================================================
   Like / favorite / bookmark buttons (Views/Shared/_ReactionBar.cshtml).
   Guests get the sign-in popup; signed-in customers toggle through
   /Reactions/Toggle and the bar is redrawn from the returned state.
   ===================================================================== */
(function () {
    "use strict";

    function render(bar, state) {
        bar.querySelectorAll(".reaction-btn").forEach(function (btn) {
            var on = btn.dataset.kind === "like" ? state.liked : state.bookmarked;
            btn.classList.toggle("active", on);
            btn.setAttribute("aria-pressed", on ? "true" : "false");
            var icon = btn.querySelector("i");
            if (icon) icon.className = "bi bi-" + btn.dataset.icon + (on ? "-fill" : "");
        });
        var count = bar.querySelector(".reaction-count");
        if (count) count.textContent = state.likeCount;
    }

    function toggle(bar, btn) {
        var feedback = bar.querySelector(".reaction-feedback");
        if (feedback) feedback.textContent = "";

        if (bar.dataset.loggedIn !== "true") {
            if (window.openAuth) window.openAuth("login");
            return;
        }

        var on = !btn.classList.contains("active");
        var body = new URLSearchParams({
            targetType: bar.dataset.targetType,
            targetId: bar.dataset.targetId,
            kind: btn.dataset.kind,
            on: on ? "true" : "false"
        });

        btn.disabled = true;
        fetch("/Reactions/Toggle", {
            method: "POST",
            headers: window.dnAntiforgeryHeaders({ "Content-Type": "application/x-www-form-urlencoded", "Accept": "application/json" }),
            body: body.toString()
        }).then(function (res) {
            if (res.status === 401) {
                bar.dataset.loggedIn = "false";
                if (window.openAuth) window.openAuth("login");
                return null;
            }
            if (!res.ok) throw new Error("failed");
            return res.json();
        }).then(function (state) {
            if (state) render(bar, state);
        }).catch(function () {
            if (feedback) feedback.textContent = "Could not save. Please try again.";
        }).finally(function () {
            btn.disabled = false;
        });
    }

    document.addEventListener("click", function (e) {
        var btn = e.target.closest(".reaction-bar .reaction-btn");
        if (!btn) return;
        e.preventDefault();
        toggle(btn.closest(".reaction-bar"), btn);
    });
})();
