/* =====================================================================
   "My Addresses" page — add/edit/delete/set-default via the Account
   controller (which proxies to the API using the customer's JWT cookie).
   ===================================================================== */
(function () {
    "use strict";

    var modalEl = document.getElementById("addressModal");
    if (!modalEl) return;
    var modal = new bootstrap.Modal(modalEl);

    function val(id) {
        var el = document.getElementById(id);
        return el ? el.value.trim() : "";
    }

    function setFeedback(message) {
        var el = document.getElementById("addressFeedback");
        if (el) el.textContent = message || "";
    }

    async function postJson(url, body) {
        try {
            var res = await fetch(url, {
                method: "POST",
                headers: window.dnAntiforgeryHeaders({ "Content-Type": "application/json" }),
                body: JSON.stringify(body || {})
            });
            var data = await res.json().catch(function () { return {}; });
            return { ok: res.ok, status: res.status, data: data };
        } catch (_) {
            return { ok: false, status: 0, data: { message: "Network error. Please try again." } };
        }
    }

    async function loadCities(countryId, selectedCityId) {
        var citySelect = document.getElementById("addrCity");
        citySelect.innerHTML = '<option value="">—</option>';
        if (!countryId) {
            citySelect.disabled = true;
            return;
        }
        citySelect.disabled = false;
        try {
            var res = await fetch("/Account/Cities?countryId=" + encodeURIComponent(countryId));
            var cities = await res.json();
            cities.forEach(function (c) {
                var opt = document.createElement("option");
                opt.value = c.id;
                opt.textContent = c.title;
                if (selectedCityId && String(c.id) === String(selectedCityId)) opt.selected = true;
                citySelect.appendChild(opt);
            });
        } catch (_) { /* leave city list empty on failure */ }
    }

    document.getElementById("addrCountry").addEventListener("change", function () {
        loadCities(this.value, null);
    });

    function resetForm() {
        document.getElementById("addressForm").reset();
        document.getElementById("addrId").value = "0";
        document.getElementById("addrCity").innerHTML = '<option value="">—</option>';
        document.getElementById("addrCity").disabled = true;
        setFeedback("");
    }

    function openAddModal() {
        resetForm();
        document.getElementById("addressModalTitle").textContent = "Add address";
        modal.show();
    }

    async function openEditModal(btn) {
        resetForm();
        document.getElementById("addressModalTitle").textContent = "Edit address";
        document.getElementById("addrId").value = btn.dataset.id;
        document.getElementById("addrTitle").value = btn.dataset.title || "";
        document.getElementById("addrReceiver").value = btn.dataset.receiver || "";
        document.getElementById("addrLine").value = btn.dataset.line || "";
        document.getElementById("addrPostal").value = btn.dataset.postal || "";
        document.getElementById("addrPhone").value = btn.dataset.phone || "";
        document.getElementById("addrDefault").checked = btn.dataset.default === "true";
        var countryId = btn.dataset.country && btn.dataset.country !== "" ? btn.dataset.country : "";
        document.getElementById("addrCountry").value = countryId;
        if (countryId) await loadCities(countryId, btn.dataset.city);
        modal.show();
    }

    document.getElementById("addAddressBtn")?.addEventListener("click", openAddModal);

    document.getElementById("addressList").addEventListener("click", function (e) {
        var editBtn = e.target.closest(".address-edit");
        if (editBtn) { openEditModal(editBtn); return; }

        var deleteBtn = e.target.closest(".address-delete");
        if (deleteBtn) { deleteAddress(deleteBtn.dataset.id); return; }

        var defaultBtn = e.target.closest(".address-set-default");
        if (defaultBtn) { setDefault(defaultBtn.dataset.id); return; }
    });

    document.getElementById("addressForm").addEventListener("submit", async function (e) {
        e.preventDefault();
        setFeedback("");

        var line = val("addrLine");
        if (!line) { setFeedback("Address is required."); return; }

        var id = parseInt(document.getElementById("addrId").value, 10) || 0;
        var payload = {
            title: val("addrTitle") || null,
            receiverName: val("addrReceiver") || null,
            countryId: val("addrCountry") ? parseInt(val("addrCountry"), 10) : null,
            cityId: val("addrCity") ? parseInt(val("addrCity"), 10) : null,
            addressLine: line,
            postalCode: val("addrPostal") || null,
            phone: val("addrPhone") || null,
            isDefault: document.getElementById("addrDefault").checked
        };

        var btn = document.getElementById("addressSubmit");
        btn.disabled = true;
        try {
            var url = id > 0 ? "/Account/UpdateAddress?id=" + id : "/Account/AddAddress";
            var r = await postJson(url, payload);
            if (r.ok) { window.location.reload(); return; }
            setFeedback(r.data.message || "Could not save the address.");
        } finally {
            btn.disabled = false;
        }
    });

    async function deleteAddress(id) {
        if (!window.confirm("Delete this address?")) return;
        var r = await postJson("/Account/DeleteAddress?id=" + id);
        if (r.ok) window.location.reload();
        else window.alert(r.data.message || "Could not delete the address.");
    }

    async function setDefault(id) {
        var r = await postJson("/Account/SetDefaultAddress?id=" + id);
        if (r.ok) window.location.reload();
        else window.alert(r.data.message || "Could not update the address.");
    }
})();
