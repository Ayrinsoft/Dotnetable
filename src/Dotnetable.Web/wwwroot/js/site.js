/* =====================================================================
   Theme interactions — generic vanilla JS (no proprietary library).
   Handles the auth popup (sign in / register / OTP / password reset)
   and the customer session calls.
   ===================================================================== */
(function () {
    "use strict";

    var authModalEl = document.getElementById("authModal");
    var authModal = authModalEl ? new bootstrap.Modal(authModalEl) : null;

    // The email/mobile awaiting OTP verification or password reset.
    var pendingIdentifier = "";

    // --- View switching ------------------------------------------------
    function showAuthView(name) {
        clearAuthFeedback();
        document.querySelectorAll(".auth-view").forEach(function (el) {
            el.classList.toggle("d-none", el.id !== "view-" + name);
        });
        // The top tabs only make sense for the login/register views.
        var tabs = document.getElementById("authTabs");
        if (tabs) tabs.classList.toggle("invisible", name !== "login" && name !== "register");
        setActive("loginTabBtn", name === "login");
        setActive("registerTabBtn", name === "register");
    }
    window.showAuthView = showAuthView;

    function setActive(id, on) {
        var el = document.getElementById(id);
        if (el) el.classList.toggle("active", on);
    }

    // Open the popup on a given tab ("login" | "register").
    window.openAuth = function (tab) {
        if (!authModal) return;
        showAuthView(tab === "register" ? "register" : "login");
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

    function val(id) {
        var el = document.getElementById(id);
        return el ? el.value.trim() : "";
    }

    // POST JSON, returning { ok, status, data }.
    async function postJson(url, body) {
        try {
            var res = await fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(body)
            });
            var data = await res.json().catch(function () { return {}; });
            return { ok: res.ok, status: res.status, data: data };
        } catch (_) {
            return { ok: false, status: 0, data: { message: "Network error. Please try again." } };
        }
    }

    function bindSubmit(formId, btnId, handler) {
        var form = document.getElementById(formId);
        if (!form) return;
        form.addEventListener("submit", async function (e) {
            e.preventDefault();
            var btn = document.getElementById(btnId);
            if (btn) btn.disabled = true;
            try { await handler(); }
            finally { if (btn) btn.disabled = false; }
        });
    }

    // --- Sign in -------------------------------------------------------
    bindSubmit("loginForm", "loginSubmit", async function () {
        setFeedback("loginFeedback", "");
        var r = await postJson("/Account/Login", {
            identifier: val("loginIdentifier"),
            password: val("loginPassword")
        });
        if (r.ok) { window.location.reload(); return; }
        if (r.status === 403 && r.data.status === "NotActivated") {
            pendingIdentifier = r.data.identifier || val("loginIdentifier");
            showVerify(pendingIdentifier);
            setFeedback("verifyFeedback", r.data.message || "Please verify your account.");
            return;
        }
        setFeedback("loginFeedback", r.data.message || "Sign in failed.");
    });

    // --- Register ------------------------------------------------------
    bindSubmit("registerForm", "registerSubmit", async function () {
        setFeedback("registerFeedback", "");
        var email = val("regEmail");
        var cellphone = val("regCellphone");
        if (!email && !cellphone) {
            setFeedback("registerFeedback", "Enter an email or a mobile number.");
            return;
        }
        if (val("regPassword") !== val("regPasswordConfirm")) {
            setFeedback("registerFeedback", "Passwords do not match.");
            return;
        }
        var r = await postJson("/Account/Register", {
            givenName: val("regGivenName"),
            surname: val("regSurname"),
            email: email,
            countryCode: val("regCountryCode"),
            cellphone: cellphone,
            password: val("regPassword")
        });
        if (r.ok) {
            pendingIdentifier = r.data.identifier || email || cellphone;
            showVerify(pendingIdentifier);
            setFeedback("verifyFeedback", r.data.message || "We sent you a verification code.", true);
            return;
        }
        setFeedback("registerFeedback", r.data.message || "Registration failed.");
    });

    function showVerify(target) {
        showAuthView("verify");
        var el = document.getElementById("verifyTarget");
        if (el) el.textContent = target;
    }

    // --- Verify OTP ----------------------------------------------------
    bindSubmit("verifyForm", "verifySubmit", async function () {
        setFeedback("verifyFeedback", "");
        var r = await postJson("/Account/VerifyOtp", {
            identifier: pendingIdentifier,
            code: val("verifyCode")
        });
        if (r.ok && !r.data.message) { window.location.reload(); return; }
        if (r.ok) { // already active — no token
            showAuthView("login");
            setFeedback("loginFeedback", r.data.message || "Account activated. Please sign in.", true);
            return;
        }
        setFeedback("verifyFeedback", r.data.message || "The code is invalid or has expired.");
    });

    window.resendOtp = async function () {
        setFeedback("verifyFeedback", "");
        var r = await postJson("/Account/ResendOtp", { identifier: pendingIdentifier });
        setFeedback("verifyFeedback", r.data.message || (r.ok ? "A new code has been sent." : "Could not resend."), r.ok);
    };

    // --- Forgot password ----------------------------------------------
    bindSubmit("forgotForm", "forgotSubmit", async function () {
        setFeedback("forgotFeedback", "");
        var identifier = val("forgotIdentifier");
        var r = await postJson("/Account/ForgotPassword", { identifier: identifier });
        if (r.ok) {
            pendingIdentifier = identifier;
            showAuthView("reset");
            var el = document.getElementById("resetTarget");
            if (el) el.textContent = identifier;
            setFeedback("resetFeedback", r.data.message || "", true);
            return;
        }
        setFeedback("forgotFeedback", r.data.message || "Could not send a reset code.");
    });

    // --- Reset password ------------------------------------------------
    bindSubmit("resetForm", "resetSubmit", async function () {
        setFeedback("resetFeedback", "");
        if (val("resetPassword") !== val("resetPasswordConfirm")) {
            setFeedback("resetFeedback", "Passwords do not match.");
            return;
        }
        var r = await postJson("/Account/ResetPassword", {
            identifier: pendingIdentifier,
            code: val("resetCode"),
            newPassword: val("resetPassword")
        });
        if (r.ok) {
            showAuthView("login");
            setFeedback("loginFeedback", r.data.message || "Password reset. Please sign in.", true);
            return;
        }
        setFeedback("resetFeedback", r.data.message || "The code is invalid or has expired.");
    });

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
