/* =====================================================================
   Theme interactions — generic vanilla JS (no proprietary library).
   Handles the login / register popup and the customer session calls.
   ===================================================================== */
(function () {
    "use strict";

    // --- Auth popup (Bootstrap modal) ----------------------------------
    var authModalEl = document.getElementById("authModal");
    var authModal = authModalEl ? new bootstrap.Modal(authModalEl) : null;

    // Open the popup on a given tab ("login" | "register").
    window.openAuth = function (tab) {
        if (!authModal) return;
        clearAuthFeedback();
        var selector = tab === "register" ? "#registerTabBtn" : "#loginTabBtn";
        var trigger = document.querySelector(selector);
        if (trigger) bootstrap.Tab.getOrCreateInstance(trigger).show();
        authModal.show();
    };

    function clearAuthFeedback() {
        document.querySelectorAll(".auth-feedback").forEach(function (el) {
            el.textContent = "";
            el.className = "auth-feedback text-danger";
        });
    }

    function setFeedback(id, message, ok) {
        var el = document.getElementById(id);
        if (!el) return;
        el.textContent = message || "";
        el.className = "auth-feedback " + (ok ? "text-success" : "text-danger");
    }

    // --- Login ---------------------------------------------------------
    var loginForm = document.getElementById("loginForm");
    if (loginForm) {
        loginForm.addEventListener("submit", async function (e) {
            e.preventDefault();
            var btn = document.getElementById("loginSubmit");
            setFeedback("loginFeedback", "");
            btn.disabled = true;
            try {
                var res = await fetch("/Account/Login", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        username: document.getElementById("loginUsername").value,
                        password: document.getElementById("loginPassword").value
                    })
                });
                if (res.ok) {
                    // Refresh so the page reflects the signed-in state.
                    window.location.reload();
                    return;
                }
                var data = await res.json().catch(function () { return {}; });
                setFeedback("loginFeedback", data.message || "Sign in failed.");
            } catch (_) {
                setFeedback("loginFeedback", "Network error. Please try again.");
            } finally {
                btn.disabled = false;
            }
        });
    }

    // --- Register ------------------------------------------------------
    var registerForm = document.getElementById("registerForm");
    if (registerForm) {
        registerForm.addEventListener("submit", async function (e) {
            e.preventDefault();
            var btn = document.getElementById("registerSubmit");
            setFeedback("registerFeedback", "");

            var pwd = document.getElementById("regPassword").value;
            var confirm = document.getElementById("regPasswordConfirm").value;
            if (pwd !== confirm) {
                setFeedback("registerFeedback", "Passwords do not match.");
                return;
            }

            btn.disabled = true;
            try {
                var res = await fetch("/Account/Register", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        givenName: document.getElementById("regGivenName").value,
                        surname: document.getElementById("regSurname").value,
                        email: document.getElementById("regEmail").value,
                        username: document.getElementById("regUsername").value,
                        password: pwd
                    })
                });
                var data = await res.json().catch(function () { return {}; });
                if (res.ok) {
                    setFeedback("registerFeedback", data.message || "Account created. You can sign in now.", true);
                    registerForm.reset();
                } else {
                    setFeedback("registerFeedback", data.message || "Registration failed.");
                }
            } catch (_) {
                setFeedback("registerFeedback", "Network error. Please try again.");
            } finally {
                btn.disabled = false;
            }
        });
    }

    // --- Logout --------------------------------------------------------
    window.doLogout = async function () {
        try { await fetch("/Account/Logout", { method: "POST" }); } catch (_) {}
        window.location.reload();
    };

    // --- Navbar shadow on scroll --------------------------------------
    var nav = document.querySelector(".site-navbar");
    if (nav) {
        var onScroll = function () {
            nav.style.boxShadow = window.scrollY > 10
                ? "0 6px 24px rgba(20,26,38,.10)"
                : "0 2px 14px rgba(20,26,38,.06)";
        };
        window.addEventListener("scroll", onScroll);
        onScroll();
    }
})();
