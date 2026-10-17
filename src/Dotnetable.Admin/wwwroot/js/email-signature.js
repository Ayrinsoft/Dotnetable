(function (root) {
    var activeFont = "Tahoma,Arial,sans-serif";
    var SOCIALS = [
        { key: "instagram", label: "اینستاگرام" },
        { key: "telegram", label: "تلگرام" },
        { key: "whatsapp", label: "واتساپ" },
        { key: "linkedin", label: "لینکدین" },
        { key: "x", label: "ایکس" },
        { key: "youtube", label: "یوتیوب" },
        { key: "aparat", label: "آپارات" }
    ];
    var COLORS = ["#5F41FB", "#1976d2", "#0f766e", "#047857", "#b45309", "#be123c", "#1e293b", "#7c3aed"];
    var FONTS = [
        { id: "tahoma", label: "Tahoma", group: "سیستمی", family: "Tahoma,Arial,sans-serif", google: "" },
        { id: "arial", label: "Arial", group: "سیستمی", family: "Arial,Helvetica,sans-serif", google: "" },
        { id: "vazirmatn", label: "وزیرمتن", group: "گوگل", family: "'Vazirmatn',Tahoma,Arial,sans-serif", google: "Vazirmatn:wght@400;700" },
        { id: "noto-sans", label: "نوتو سنس", group: "گوگل", family: "'Noto Sans Arabic',Tahoma,Arial,sans-serif", google: "Noto+Sans+Arabic:wght@400;700" },
        { id: "noto-naskh", label: "نوتو نسخ", group: "گوگل", family: "'Noto Naskh Arabic',Tahoma,serif", google: "Noto+Naskh+Arabic:wght@400;700" },
        { id: "cairo", label: "کایرو", group: "گوگل", family: "'Cairo',Tahoma,Arial,sans-serif", google: "Cairo:wght@400;700" },
        { id: "rubik", label: "روبیک", group: "گوگل", family: "'Rubik',Tahoma,Arial,sans-serif", google: "Rubik:wght@400;700" },
        { id: "ibm", label: "آی‌بی‌ام پلکس", group: "گوگل", family: "'IBM Plex Sans Arabic',Tahoma,Arial,sans-serif", google: "IBM+Plex+Sans+Arabic:wght@400;700" },
        { id: "readex", label: "ریدکس پرو", group: "گوگل", family: "'Readex Pro',Tahoma,Arial,sans-serif", google: "Readex+Pro:wght@400;700" },
        { id: "el-messiri", label: "المصیری", group: "گوگل", family: "'El Messiri',Tahoma,sans-serif", google: "El+Messiri:wght@400;700" },
        { id: "tajawal", label: "تجوال", group: "گوگل", family: "'Tajawal',Tahoma,Arial,sans-serif", google: "Tajawal:wght@400;700" },
        { id: "almarai", label: "المرای", group: "گوگل", family: "'Almarai',Tahoma,Arial,sans-serif", google: "Almarai:wght@400;700" },
        { id: "amiri", label: "امیری", group: "گوگل", family: "'Amiri',Tahoma,serif", google: "Amiri:wght@400;700" },
        { id: "markazi", label: "مرکزی", group: "گوگل", family: "'Markazi Text',Tahoma,serif", google: "Markazi+Text:wght@400;700" },
        { id: "scheherazade", label: "شهرزاد", group: "گوگل", family: "'Scheherazade New',Tahoma,serif", google: "Scheherazade+New:wght@400;700" },
        { id: "lalezar", label: "لاله‌زار", group: "گوگل", family: "'Lalezar',Tahoma,cursive", google: "Lalezar" },
        { id: "lateef", label: "لطیف", group: "گوگل", family: "'Lateef',Tahoma,serif", google: "Lateef:wght@400;700" },
        { id: "mirza", label: "میرزا", group: "گوگل", family: "'Mirza',Tahoma,serif", google: "Mirza:wght@400;700" }
    ];
    var TEMPLATES = {
        classic: tplClassic,
        modern: tplModern,
        bold: tplBold,
        elegant: tplElegant,
        compact: tplCompact,
        minimal: tplMinimal,
        professional: tplProfessional,
        social: tplSocial,
        executive: tplExecutive,
        stacked: tplStacked,
        headline: tplHeadline,
        sidebar: tplSidebar
    };

    function esc(value) {
        return String(value == null ? "" : value).replace(/[&<>"']/g, function (ch) {
            return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[ch];
        });
    }

    function rgb(hex) {
        var h = hex.slice(1);
        if (h.length === 3) h = h.split("").map(function (c) { return c + c; }).join("");
        var n = parseInt(h, 16);
        return [(n >> 16) & 255, (n >> 8) & 255, n & 255];
    }

    function isLight(hex) {
        var c = rgb(hex);
        return (c[0] * 299 + c[1] * 587 + c[2] * 114) / 1000 > 170;
    }

    function inkOn(hex) {
        return isLight(hex) ? "#1a1d24" : "#ffffff";
    }

    function safeColor(color) {
        var c = String(color || "").trim();
        return /^#([0-9a-f]{6}|[0-9a-f]{3})$/i.test(c) ? c : "#5F41FB";
    }

    function safePhoto(src) {
        var s = String(src || "").trim();
        if (/^https?:\/\//i.test(s)) return s;
        if (/^data:image\/(jpeg|jpg|png|webp);base64,[a-z0-9+/=\s]+$/i.test(s)) return s.replace(/\s/g, "");
        return "";
    }

    function webHref(raw) {
        var s = String(raw || "").trim();
        if (!s || /^(javascript|data|vbscript):/i.test(s)) return "";
        if (/^https?:\/\//i.test(s)) return s;
        if (/^mailto:/i.test(s)) return s;
        return "https://" + s.replace(/^\/+/, "");
    }

    function telHref(raw) {
        var s = String(raw || "").replace(/[^\d+]/g, "");
        return s ? "tel:" + s : "";
    }

    function socialHref(key, raw) {
        var s = String(raw || "").trim();
        if (!s) return "";
        if (/^https?:\/\//i.test(s)) return s;
        if (/^(javascript|data|vbscript):/i.test(s)) return "";
        var handle = s.replace(/^@/, "");
        if (key === "instagram") return "https://instagram.com/" + encodeURIComponent(handle);
        if (key === "telegram") return "https://t.me/" + encodeURIComponent(handle);
        if (key === "x") return "https://x.com/" + encodeURIComponent(handle);
        if (key === "aparat") return "https://www.aparat.com/" + encodeURIComponent(handle);
        if (key === "linkedin") return "https://www.linkedin.com/in/" + encodeURIComponent(handle);
        if (key === "youtube") return "https://www.youtube.com/@" + encodeURIComponent(handle.replace(/^@/, ""));
        if (key === "whatsapp") {
            var digits = s.replace(/\D/g, "");
            if (digits.indexOf("0") === 0) digits = "98" + digits.slice(1);
            return digits ? "https://wa.me/" + digits : "";
        }
        return webHref(s);
    }

    function fontSpec(id) {
        for (var i = 0; i < FONTS.length; i++) {
            if (FONTS[i].id === id) return FONTS[i];
        }
        return FONTS[0];
    }

    function fieldOn(show, key) {
        return !show || show[key] !== false;
    }

    function normalize(state) {
        var src = state || {};
        var show = src.show || {};
        var font = fontSpec(src.font);
        var socials = {};
        SOCIALS.forEach(function (item) {
            var value = String((src.socials && src.socials[item.key]) || "").trim();
            socials[item.key] = fieldOn(show, "social:" + item.key) ? value : "";
        });
        var extras = (src.extras || []).slice(0, 4).map(function (item, index) {
            if (!fieldOn(show, "extra:" + index)) return null;
            return {
                label: String((item && item.label) || "").trim().slice(0, 40),
                url: String((item && item.url) || "").trim().slice(0, 250)
            };
        }).filter(Boolean);
        return {
            template: TEMPLATES[src.template] ? src.template : "classic",
            color: safeColor(src.color),
            font: font.id,
            fontFamily: font.family,
            googleHref: font.google ? "https://fonts.googleapis.com/css2?family=" + font.google + "&display=swap" : "",
            fullName: fieldOn(show, "fullName") ? String(src.fullName || "").trim() : "",
            jobTitle: fieldOn(show, "jobTitle") ? String(src.jobTitle || "").trim() : "",
            brand: fieldOn(show, "brand") ? String(src.brand || "").trim() : "",
            slogan: fieldOn(show, "slogan") ? String(src.slogan || "").trim() : "",
            mobile: fieldOn(show, "mobile") ? String(src.mobile || "").trim() : "",
            phone: fieldOn(show, "phone") ? String(src.phone || "").trim() : "",
            phone2: fieldOn(show, "phone2") ? String(src.phone2 || "").trim() : "",
            phone2Title: String(src.phone2Title || "").trim() || "شماره",
            phone3: fieldOn(show, "phone3") ? String(src.phone3 || "").trim() : "",
            phone3Title: String(src.phone3Title || "").trim() || "شماره",
            email: fieldOn(show, "email") ? String(src.email || "").trim() : "",
            website: fieldOn(show, "website") ? String(src.website || "").trim() : "",
            address: fieldOn(show, "address") ? String(src.address || "").trim() : "",
            photo: safePhoto(src.photo),
            showPhoto: fieldOn(show, "photo") && src.showPhoto !== false,
            socials: socials,
            extras: extras,
            showCta: fieldOn(show, "cta") && !!src.showCta,
            ctaLabel: String(src.ctaLabel || "").trim(),
            ctaUrl: String(src.ctaUrl || "").trim(),
            showDisclaimer: !!src.showDisclaimer,
            disclaimer: String(src.disclaimer || "").trim()
        };
    }

    function contactItems(s) {
        var items = [];
        if (s.mobile) items.push({ label: "موبایل", text: s.mobile, href: telHref(s.mobile) });
        if (s.phone) items.push({ label: "تلفن", text: s.phone, href: telHref(s.phone) });
        if (s.phone2) items.push({ label: s.phone2Title || "شماره", text: s.phone2, href: telHref(s.phone2) });
        if (s.phone3) items.push({ label: s.phone3Title || "شماره", text: s.phone3, href: telHref(s.phone3) });
        if (s.email && s.email.indexOf("\n") < 0) items.push({ label: "ایمیل", text: s.email, href: "mailto:" + s.email });
        if (s.website) {
            var href = webHref(s.website);
            if (href) items.push({ label: "وب", text: s.website.replace(/^https?:\/\//i, "").replace(/\/$/, ""), href: href });
        }
        if (s.address) items.push({ label: "آدرس", text: s.address, href: "" });
        return items;
    }

    function socialItems(s) {
        var items = [];
        SOCIALS.forEach(function (def) {
            var href = socialHref(def.key, s.socials[def.key]);
            if (href) items.push({ label: def.label, href: href });
        });
        s.extras.forEach(function (extra) {
            var href = webHref(extra.url);
            if (href && extra.label) items.push({ label: extra.label, href: href });
        });
        return items;
    }

    function line(html, size, color, weight, pad) {
        if (!html) return "";
        return '<div style="font-family:' + activeFont + ';font-size:' + size + 'px;line-height:' + (size + 6) + 'px;color:' + color + ';font-weight:' + (weight || "normal") + ';margin:0;padding:' + (pad || 0) + 'px 0 0 0;">' + html + '</div>';
    }

    function identity(s, opt) {
        opt = opt || {};
        var parts = [];
        if (s.fullName) parts.push(line(esc(s.fullName), opt.nameSize || 18, opt.nameColor || "#1a1d24", "bold", 0));
        if (s.jobTitle) parts.push(line(esc(s.jobTitle), 13, opt.jobColor || opt.color, "normal", 2));
        if (s.brand) parts.push(line(esc(s.brand), 13, opt.brandColor || "#5c6570", "normal", 1));
        if (s.slogan && opt.slogan !== false) parts.push(line(esc(s.slogan), 12, opt.sloganColor || "#6b7280", "normal", 4));
        if (!parts.length) return "";
        return '<div style="text-align:' + (opt.align || "right") + ';">' + parts.join("") + '</div>';
    }

    function valueHtml(item, color, size) {
        var inner = item.href
            ? '<a href="' + esc(item.href) + '" style="color:' + color + ';text-decoration:none;font-family:' + activeFont + ';font-size:' + size + 'px;">' + esc(item.text) + '</a>'
            : '<span style="color:#3d4450;font-family:' + activeFont + ';font-size:' + size + 'px;">' + esc(item.text) + '</span>';
        if (item.href) inner = '<span dir="ltr" style="direction:ltr;unicode-bidi:embed;">' + inner + '</span>';
        return inner;
    }

    function contactTable(items, color, align) {
        if (!items.length) return "";
        var rows = items.map(function (item) {
            return '<tr><td style="font-family:' + activeFont + ';font-size:13px;line-height:20px;color:#6b7280;padding:1px 0;text-align:' + (align || "right") + ';">' + esc(item.label) + ': ' + valueHtml(item, color, 13) + '</td></tr>';
        }).join("");
        return '<table cellpadding="0" cellspacing="0" border="0" role="presentation" dir="rtl" style="border-collapse:collapse;margin-top:8px;">' + rows + '</table>';
    }

    function inlineContacts(items, color, align) {
        if (!items.length) return "";
        var html = items.map(function (item, index) {
            return (index ? '<span style="color:#c5cad3;"> &nbsp;|&nbsp; </span>' : "") + valueHtml(item, color, 12);
        }).join("");
        return '<div style="font-family:' + activeFont + ';font-size:12px;line-height:18px;padding-top:4px;text-align:' + (align || "right") + ';">' + html + '</div>';
    }

    function pills(items, color) {
        if (!items.length) return "";
        var fg = inkOn(color);
        var cells = items.map(function (item) {
            return '<td style="padding:8px 0 0 6px;"><a href="' + esc(item.href) + '" style="display:inline-block;background:' + color + ';color:' + fg + ';font-family:' + activeFont + ';font-size:11px;line-height:16px;font-weight:bold;text-decoration:none;padding:4px 8px;border-radius:4px;">' + esc(item.label) + '</a></td>';
        }).join("");
        return '<table cellpadding="0" cellspacing="0" border="0" role="presentation" dir="rtl" style="border-collapse:collapse;"><tr>' + cells + '</tr></table>';
    }

    function socialBar(items, color) {
        if (!items.length) return "";
        var fg = inkOn(color);
        var html = items.map(function (item, index) {
            return (index ? '<span style="color:' + fg + ';">&nbsp;|&nbsp;</span>' : "") + '<a href="' + esc(item.href) + '" style="color:' + fg + ';font-family:' + activeFont + ';font-size:12px;line-height:18px;font-weight:bold;text-decoration:none;">' + esc(item.label) + '</a>';
        }).join("");
        return '<table cellpadding="0" cellspacing="0" border="0" role="presentation" width="100%" dir="rtl" style="border-collapse:collapse;margin-top:10px;"><tr><td bgcolor="' + color + '" style="background:' + color + ';padding:8px 12px;text-align:right;">' + html + '</td></tr></table>';
    }

    function textSocials(items, color, align) {
        if (!items.length) return "";
        var html = items.map(function (item, index) {
            return (index ? ' <span style="color:#c5cad3;">|</span> ' : "") + '<a href="' + esc(item.href) + '" style="color:' + color + ';font-family:' + activeFont + ';font-size:12px;text-decoration:none;">' + esc(item.label) + '</a>';
        }).join("");
        return '<div style="font-family:' + activeFont + ';font-size:12px;line-height:18px;padding-top:8px;text-align:' + (align || "right") + ';">' + html + '</div>';
    }

    function cta(s, color, top) {
        if (!s.showCta) return "";
        var href = webHref(s.ctaUrl);
        if (!s.ctaLabel || !href) return "";
        var fg = inkOn(color);
        return '<table cellpadding="0" cellspacing="0" border="0" role="presentation" dir="rtl" style="border-collapse:collapse;margin-top:' + (top || 10) + 'px;"><tr><td bgcolor="' + color + '" style="background:' + color + ';border-radius:6px;padding:8px 14px;"><a href="' + esc(href) + '" style="color:' + fg + ';font-family:' + activeFont + ';font-size:13px;line-height:18px;font-weight:bold;text-decoration:none;">' + esc(s.ctaLabel) + '</a></td></tr></table>';
    }

    function disclaimer(s) {
        if (!s.showDisclaimer || !s.disclaimer) return "";
        return '<div style="font-family:' + activeFont + ';font-size:11px;line-height:17px;color:#8b919c;padding-top:10px;">' + esc(s.disclaimer) + '</div>';
    }

    function photoTag(src, size) {
        if (!src) return "";
        var inner = Math.max(24, size - 12);
        return '<table cellpadding="0" cellspacing="0" border="0" role="presentation"><tr><td width="' + size + '" height="' + size + '" align="center" valign="middle" bgcolor="#ffffff" style="width:' + size + 'px;height:' + size + 'px;background:#ffffff;border-radius:8px;text-align:center;vertical-align:middle;">'
            + '<img src="' + esc(src) + '" width="' + inner + '" alt="" style="display:block;margin:0 auto;width:' + inner + 'px;height:auto;max-height:' + inner + 'px;border:0;outline:none;" />'
            + '</td></tr></table>';
    }

    function initialBadge(name, bg, fg) {
        var ch = (name || "؟").trim().charAt(0) || "؟";
        return '<table cellpadding="0" cellspacing="0" border="0" role="presentation"><tr><td width="64" height="64" align="center" valign="middle" bgcolor="' + bg + '" style="width:64px;height:64px;background:' + bg + ';color:' + fg + ';font-family:' + activeFont + ';font-size:26px;line-height:64px;font-weight:bold;text-align:center;">' + esc(ch) + '</td></tr></table>';
    }

    function tableShell(inner, width) {
        var w = width || 520;
        return '<table cellpadding="0" cellspacing="0" border="0" role="presentation" width="' + w + '" dir="rtl" style="direction:rtl;width:' + w + 'px;max-width:' + w + 'px;border-collapse:collapse;mso-table-lspace:0;mso-table-rspace:0;"><tr><td style="font-family:' + activeFont + ';text-align:right;">' + inner + '</td></tr></table>';
    }

    function shownPhoto(s) {
        return s.showPhoto ? s.photo : "";
    }

    function tplClassic(s) {
        var photo = shownPhoto(s);
        var img = photoTag(photo, 84, 8);
        var body = identity(s, { color: s.color }) + contactTable(contactItems(s), s.color) + pills(socialItems(s), s.color) + cta(s, s.color) + disclaimer(s);
        var photoCell = img ? '<td width="96" valign="top" style="width:96px;padding-left:14px;vertical-align:top;">' + img + '</td>' : '';
        return tableOpen(520) + photoCell + '<td valign="top" style="vertical-align:top;text-align:right;font-family:' + activeFont + ';">' + body + '</td></tr></table>';
    }

    function tplModern(s) {
        var photo = photoTag(shownPhoto(s), 72, 36);
        var photoCell = photo ? '<td width="84" valign="top" style="width:84px;padding-left:12px;vertical-align:top;">' + photo + '</td>' : '';
        var inner = '<table cellpadding="0" cellspacing="0" border="0" role="presentation" dir="rtl" style="border-collapse:collapse;"><tr>' + photoCell + '<td valign="top" style="vertical-align:top;">' + identity(s, { color: s.color }) + '</td></tr></table>'
            + contactTable(contactItems(s), s.color) + pills(socialItems(s), s.color) + cta(s, s.color) + disclaimer(s);
        return tableOpen(520) + '<td style="font-family:' + activeFont + ';text-align:right;">'
            + '<table cellpadding="0" cellspacing="0" border="0" role="presentation" width="100%" style="border-collapse:collapse;margin-bottom:10px;"><tr><td height="4" bgcolor="' + s.color + '" style="height:4px;background:' + s.color + ';font-size:0;line-height:0;">&nbsp;</td></tr></table>'
            + inner + '</td></tr></table>';
    }

    function tplBold(s) {
        var fg = inkOn(s.color);
        var photo = photoTag(shownPhoto(s), 64, 6);
        var photoCell = photo ? '<td width="80" valign="middle" style="width:80px;padding-left:12px;vertical-align:middle;">' + photo + '</td>' : '';
        var head = identity(s, { color: s.color, nameColor: fg, jobColor: fg, brandColor: fg, slogan: false, nameSize: 20 });
        var body = (s.slogan ? line(esc(s.slogan), 12, "#6b7280", "normal", 0) : "") + contactTable(contactItems(s), s.color) + pills(socialItems(s), s.color) + cta(s, s.color) + disclaimer(s);
        return tableOpen(520) + '<td style="font-family:' + activeFont + ';">'
            + '<table cellpadding="0" cellspacing="0" border="0" role="presentation" width="100%" dir="rtl" style="border-collapse:collapse;"><tr><td bgcolor="' + s.color + '" style="background:' + s.color + ';padding:14px 16px;"><table cellpadding="0" cellspacing="0" border="0" role="presentation" dir="rtl"><tr>' + photoCell + '<td valign="middle" style="vertical-align:middle;">' + head + '</td></tr></table></td></tr></table>'
            + '<div style="padding-top:10px;">' + body + '</div></td></tr></table>';
    }

    function tplElegant(s) {
        var photo = photoTag(shownPhoto(s), 72, 36);
        var photoCell = photo ? '<td width="86" valign="top" style="width:86px;padding-left:12px;vertical-align:top;">' + photo + '</td>' : '';
        var body = identity(s, { color: s.color, nameColor: s.color, nameSize: 20 })
            + '<div style="height:1px;background:#e6e8ee;margin-top:8px;font-size:0;line-height:0;">&nbsp;</div>'
            + contactTable(contactItems(s), s.color) + textSocials(socialItems(s), s.color) + cta(s, s.color) + disclaimer(s);
        return tableOpen(520) + '<td style="font-family:' + activeFont + ';border-top:2px solid ' + s.color + ';border-bottom:1px solid #e6e8ee;padding:12px 0;"><table cellpadding="0" cellspacing="0" border="0" role="presentation" dir="rtl"><tr>' + photoCell + '<td valign="top" style="vertical-align:top;">' + body + '</td></tr></table></td></tr></table>';
    }

    function tplCompact(s) {
        var photo = photoTag(shownPhoto(s), 48, 6);
        var photoCell = photo ? '<td width="58" valign="middle" style="width:58px;padding-left:10px;vertical-align:middle;">' + photo + '</td>' : '';
        var title = '<div style="font-family:' + activeFont + ';font-size:14px;line-height:20px;color:#1a1d24;">'
            + (s.fullName ? '<strong>' + esc(s.fullName) + '</strong>' : '')
            + (s.jobTitle ? ' <span style="color:' + s.color + ';">' + esc(s.jobTitle) + '</span>' : '')
            + (s.brand ? ' <span style="color:#6b7280;">· ' + esc(s.brand) + '</span>' : '')
            + '</div>' + inlineContacts(contactItems(s), s.color) + textSocials(socialItems(s), s.color) + cta(s, s.color);
        return tableOpen(520) + photoCell + '<td valign="middle" style="vertical-align:middle;font-family:' + activeFont + ';">' + title + disclaimer(s) + '</td></tr></table>';
    }

    function tplMinimal(s) {
        var photo = photoTag(shownPhoto(s), 40, 4);
        var body = (photo ? '<div style="padding-bottom:8px;">' + photo + '</div>' : '')
            + identity(s, { color: "#6b7280", nameColor: "#1a1d24", jobColor: "#6b7280", nameSize: 16, slogan: false })
            + (s.slogan ? line(esc(s.slogan), 12, "#6b7280", "normal", 4) : "")
            + inlineContacts(contactItems(s), "#374151")
            + textSocials(socialItems(s), s.color)
            + (s.showCta && s.ctaLabel && webHref(s.ctaUrl) ? '<div style="padding-top:8px;"><a href="' + esc(webHref(s.ctaUrl)) + '" style="color:' + s.color + ';font-family:' + activeFont + ';font-size:13px;text-decoration:none;">' + esc(s.ctaLabel) + '</a></div>' : "")
            + disclaimer(s);
        return tableShell(body, 480);
    }

    function tplProfessional(s) {
        var photo = photoTag(shownPhoto(s), 78, 4);
        var photoCell = photo ? '<td width="90" valign="top" style="width:90px;padding-left:12px;vertical-align:top;">' + photo + '</td>' : '';
        var body = identity(s, { color: s.color }) + contactTable(contactItems(s), s.color) + textSocials(socialItems(s), s.color) + cta(s, s.color) + disclaimer(s);
        return tableOpen(520) + photoCell
            + '<td width="4" bgcolor="' + s.color + '" style="width:4px;background:' + s.color + ';font-size:0;line-height:0;">&nbsp;</td>'
            + '<td valign="top" style="vertical-align:top;padding-right:14px;font-family:' + activeFont + ';">' + body + '</td></tr></table>';
    }

    function tplSocial(s) {
        var body = identity(s, { color: s.color }) + contactTable(contactItems(s), s.color) + socialBar(socialItems(s), s.color) + cta(s, s.color) + disclaimer(s);
        var photo = photoTag(shownPhoto(s), 64, 32);
        var photoCell = photo ? '<td width="76" valign="top" style="width:76px;padding-left:12px;vertical-align:top;">' + photo + '</td>' : '';
        return tableOpen(520) + photoCell + '<td valign="top" style="vertical-align:top;font-family:' + activeFont + ';">' + body + '</td></tr></table>';
    }

    function tplExecutive(s) {
        var photo = photoTag(shownPhoto(s), 96, 8);
        var photoCell = photo ? '<td width="110" valign="top" style="width:110px;padding-right:14px;vertical-align:top;">' + photo + '</td>' : '';
        var body = identity(s, { color: s.color, nameSize: 22 }) + cta(s, s.color, 12) + contactTable(contactItems(s), s.color) + pills(socialItems(s), s.color) + disclaimer(s);
        return tableOpen(520) + '<td valign="top" style="vertical-align:top;font-family:' + activeFont + ';text-align:right;">' + body + '</td>' + photoCell + '</tr></table>';
    }

    function tplStacked(s) {
        var photo = shownPhoto(s);
        var face = photo ? photoTag(photo, 80, 40) : ((s.fullName || s.brand) ? initialBadge(s.fullName || s.brand, "#f3f0ff", s.color) : "");
        var body = '<div style="text-align:center;">' + face + '</div>'
            + identity(s, { color: s.color, align: "center", nameSize: 18 })
            + contactTable(contactItems(s), s.color, "center")
            + '<div style="text-align:center;">' + textSocials(socialItems(s), s.color, "center") + '</div>'
            + '<table cellpadding="0" cellspacing="0" border="0" role="presentation" align="center" style="border-collapse:collapse;margin:0 auto;">' + ctaRow(s) + '</table>'
            + '<div style="text-align:center;">' + disclaimer(s) + '</div>';
        return tableShell(body, 420);
    }

    function ctaRow(s) {
        var block = cta(s, s.color, 10);
        if (!block) return "";
        return '<tr><td align="center">' + block + '</td></tr>';
    }

    function tplHeadline(s) {
        var rule = '<table cellpadding="0" cellspacing="0" border="0" role="presentation" width="72" style="border-collapse:collapse;margin-top:8px;"><tr><td height="3" bgcolor="' + s.color + '" style="height:3px;background:' + s.color + ';font-size:0;line-height:0;">&nbsp;</td></tr></table>';
        var photo = photoTag(shownPhoto(s), 56, 6);
        var body = (photo ? '<div style="padding-bottom:8px;">' + photo + '</div>' : '')
            + (s.fullName ? line(esc(s.fullName), 26, s.color, "bold", 0) : "")
            + (s.jobTitle || s.brand ? line(esc([s.jobTitle, s.brand].filter(Boolean).join(" · ")), 13, "#5c6570", "normal", 2) : "")
            + (s.slogan ? line(esc(s.slogan), 12, "#6b7280", "normal", 4) : "")
            + rule
            + inlineContacts(contactItems(s), s.color)
            + textSocials(socialItems(s), s.color)
            + cta(s, s.color)
            + disclaimer(s);
        return tableShell(body, 520);
    }

    function tplSidebar(s) {
        var fg = inkOn(s.color);
        var photo = shownPhoto(s);
        var face = photo ? photoTag(photo, 72, 8) : ((s.fullName || s.brand) ? initialBadge(s.fullName || s.brand, fg, s.color) : "");
        var side = '<td width="148" valign="top" bgcolor="' + s.color + '" style="width:148px;background:' + s.color + ';padding:16px 12px;vertical-align:top;text-align:center;">'
            + '<div style="text-align:center;">' + face + '</div>'
            + identity(s, { color: s.color, nameColor: fg, jobColor: fg, brandColor: fg, slogan: false, align: "center", nameSize: 16 })
            + '</td>';
        var body = (s.slogan ? line(esc(s.slogan), 12, "#6b7280", "normal", 0) : "")
            + contactTable(contactItems(s), s.color) + pills(socialItems(s), s.color) + cta(s, s.color) + disclaimer(s);
        return tableOpen(520) + side + '<td valign="top" style="vertical-align:top;padding:12px 14px;font-family:' + activeFont + ';background:#ffffff;">' + body + '</td></tr></table>';
    }

    function tableOpen(width) {
        return '<table cellpadding="0" cellspacing="0" border="0" role="presentation" width="' + width + '" dir="rtl" style="direction:rtl;width:' + width + 'px;max-width:' + width + 'px;border-collapse:collapse;mso-table-lspace:0;mso-table-rspace:0;"><tr>';
    }

    function hasContent(s) {
        return !!(s.fullName || s.jobTitle || s.brand || s.slogan || s.mobile || s.phone || s.phone2 || s.phone3 || s.email || s.website || s.address || shownPhoto(s) || socialItems(s).length || (s.showCta && s.ctaLabel && webHref(s.ctaUrl)) || (s.showDisclaimer && s.disclaimer));
    }

    function buildSignature(state) {
        var s = normalize(state);
        activeFont = s.fontFamily || activeFont;
        if (!hasContent(s)) return "";
        var render = TEMPLATES[s.template] || tplClassic;
        var html = render(s);
        if (!html || !s.googleHref) return html;
        return '<link rel="stylesheet" href="' + s.googleHref.replace(/&/g, "&amp;") + '">' + html;
    }

    function buildPlain(state) {
        var s = normalize(state);
        var lines = [];
        if (s.fullName) lines.push(s.fullName);
        var role = [s.jobTitle, s.brand].filter(Boolean).join(" | ");
        if (role) lines.push(role);
        if (s.slogan) lines.push(s.slogan);
        contactItems(s).forEach(function (item) { lines.push(item.label + ": " + item.text); });
        socialItems(s).forEach(function (item) { lines.push(item.label + ": " + item.href); });
        if (s.showCta && s.ctaLabel && webHref(s.ctaUrl)) lines.push(s.ctaLabel + ": " + webHref(s.ctaUrl));
        if (s.showDisclaimer && s.disclaimer) lines.push(s.disclaimer);
        return lines.join("\n");
    }

    root.PersianQrEmailSignature = {
        buildSignature: buildSignature,
        buildPlain: buildPlain,
        socialHref: socialHref,
        normalize: normalize
    };

    function boot() {
        var app = document.getElementById("esigApp");
        if (!app || app.getAttribute("data-ready") === "1") return;
        app.setAttribute("data-ready", "1");
        var seed = {};
        try { seed = JSON.parse((document.getElementById("esigSeed") || {}).textContent || "{}"); } catch (e) { seed = {}; }
        var key = "psqr-email-sig-choice-" + (app.getAttribute("data-id") || "0");
        var state = stateFromSeed(seed);
        var saved = readChoice(key);
        if (saved) {
            if (saved.template && TEMPLATES[saved.template]) state.template = saved.template;
            if (saved.color) state.color = saved.color;
            if (saved.font && fontSpec(saved.font).id === saved.font) state.font = saved.font;
            if (saved.show) state.show = saved.show;
        }
        var timer = 0;
        var fields = fieldOptions(seed);
        var fieldsBox = document.getElementById("esigFields");
        var fieldsEmpty = document.getElementById("esigFieldsEmpty");
        fieldsBox.innerHTML = fields.map(function (item) {
            var checked = state.show[item.key] !== false ? " checked" : "";
            return '<label><input type="checkbox" name="esig-field" value="' + esc(item.key) + '"' + checked + '>' + esc(item.label) + '</label>';
        }).join("");
        if (fieldsEmpty) fieldsEmpty.hidden = fields.length > 0;
        var fontSelect = document.getElementById("esigFont");
        var fontHtml = "";
        var lastGroup = "";
        FONTS.forEach(function (font) {
            if (font.group !== lastGroup) {
                if (lastGroup) fontHtml += "</optgroup>";
                fontHtml += '<optgroup label="' + esc(font.group) + '">';
                lastGroup = font.group;
            }
            fontHtml += '<option value="' + esc(font.id) + '">' + esc(font.label) + '</option>';
        });
        if (lastGroup) fontHtml += "</optgroup>";
        fontSelect.innerHTML = fontHtml;
        fontSelect.value = state.font || "tahoma";

        document.getElementById("esigSwatches").innerHTML = COLORS.map(function (color) {
            return '<button type="button" class="esig-swatch" data-color="' + color + '" style="background:' + color + '" aria-label="' + color + '"></button>';
        }).join("");
        document.getElementById("esigColor").value = toLongHex(state.color);
        paint();

        document.getElementById("esigTemplates").addEventListener("click", function (event) {
            var btn = event.target.closest("[data-template]");
            if (!btn) return;
            state.template = btn.getAttribute("data-template");
            persist();
            paint();
        });
        document.getElementById("esigSwatches").addEventListener("click", function (event) {
            var btn = event.target.closest("[data-color]");
            if (!btn) return;
            state.color = btn.getAttribute("data-color");
            document.getElementById("esigColor").value = toLongHex(state.color);
            persist();
            paint();
        });
        document.getElementById("esigColor").addEventListener("input", function () {
            state.color = this.value;
            persist();
            paint();
        });
        fieldsBox.addEventListener("change", function (event) {
            var box = event.target;
            if (!box || box.name !== "esig-field") return;
            state.show[box.value] = box.checked;
            persist();
            paint();
        });
        fontSelect.addEventListener("change", function () {
            state.font = fontSpec(this.value).id;
            persist();
            paint();
        });
        document.getElementById("esigDesk").addEventListener("click", function () { setMode(false); });
        document.getElementById("esigMobile").addEventListener("click", function () { setMode(true); });
        document.getElementById("esigPreview").addEventListener("click", function (event) {
            if (event.target.closest("a")) event.preventDefault();
        });
        document.getElementById("esigGetHtml").addEventListener("click", openHtml);
        var htmlClose = document.getElementById("esigHtmlClose");
        if (htmlClose) htmlClose.addEventListener("click", function () { document.getElementById("esigHtmlModal").hidden = true; });
        var htmlModal = document.getElementById("esigHtmlModal");
        if (htmlModal) htmlModal.addEventListener("click", function (event) { if (event.target === htmlModal) htmlModal.hidden = true; });
        document.getElementById("esigCopyRich").addEventListener("click", function () { copyRich(); });
        document.getElementById("esigCopyCode").addEventListener("click", function () { copyCode(); });
        document.getElementById("esigDownload").addEventListener("click", downloadHtml);

        function paint() {
            ensureFont(state.font, function () { paint(); });
            var html = buildSignature(state);
            var slot = document.getElementById("esigPreview");
            slot.innerHTML = html || '<div class="esig-empty">این کارت هنوز نام یا راه تماسی ندارد. اول اطلاعات کارت ویزیت را کامل کنید.</div>';
            document.querySelectorAll(".esig-tpl").forEach(function (btn) {
                btn.classList.toggle("active", btn.getAttribute("data-template") === state.template);
            });
            document.querySelectorAll(".esig-swatch").forEach(function (btn) {
                btn.classList.toggle("active", btn.getAttribute("data-color").toLowerCase() === String(state.color).toLowerCase());
            });
            fitPreview();
        }

        function fitPreview() {
            var slot = document.getElementById("esigPreview");
            var mail = document.getElementById("esigMail");
            var table = slot.querySelector("table");
            if (!table) {
                slot.style.height = "";
                return;
            }
            table.style.transform = "";
            table.style.transformOrigin = "top right";
            var mobile = mail.classList.contains("is-mobile");
            var natural = table.offsetWidth || 520;
            var style = window.getComputedStyle(slot);
            var padX = (parseFloat(style.paddingLeft) || 0) + (parseFloat(style.paddingRight) || 0);
            var padY = (parseFloat(style.paddingTop) || 0) + (parseFloat(style.paddingBottom) || 0);
            var avail = Math.max(0, slot.clientWidth - padX);
            var scale = mobile ? 0.62 : (avail > 0 ? Math.min(1, avail / natural) : 1);
            if (scale < 0.995) {
                table.style.transform = "scale(" + scale.toFixed(3) + ")";
                slot.style.height = Math.ceil(table.offsetHeight * scale + padY) + "px";
                slot.style.overflow = "hidden";
            } else {
                slot.style.height = "";
                slot.style.overflow = "";
            }
        }

        function setMode(mobile) {
            document.getElementById("esigMail").classList.toggle("is-mobile", mobile);
            document.getElementById("esigMobile").classList.toggle("active", mobile);
            document.getElementById("esigDesk").classList.toggle("active", !mobile);
            paint();
        }

        function persist() {
            clearTimeout(timer);
            timer = setTimeout(function () {
                try { localStorage.setItem(key, JSON.stringify({ template: state.template, color: state.color, font: state.font, show: state.show })); } catch (e) { }
            }, 150);
        }

        function currentHtml() {
            return buildSignature(state);
        }

        function openHtml() {
            document.getElementById("esigHtmlCode").value = currentHtml();
            document.getElementById("esigCopyMsg").textContent = "";
            document.getElementById("esigHtmlModal").hidden = false;
        }

        function copyRich() {
            var html = document.getElementById("esigHtmlCode").value || currentHtml();
            var plain = buildPlain(state);
            var done = function (ok, note) { document.getElementById("esigCopyMsg").textContent = ok ? (note || "آماده جای‌گذاری است. در کادر امضای جیمیل Paste کنید.") : "کپی نشد. کد را از کادر انتخاب کنید."; };
            if (navigator.clipboard && root.ClipboardItem) {
                navigator.clipboard.write([
                    new ClipboardItem({
                        "text/html": new Blob([html], { type: "text/html" }),
                        "text/plain": new Blob([plain], { type: "text/plain" })
                    })
                ]).then(function () { done(true); }).catch(function () { copyText(html).then(function (ok) { done(ok, "کد متنی کپی شد."); }); });
                return;
            }
            copyText(html).then(function (ok) { done(ok, "کد متنی کپی شد."); });
        }

        function copyCode() {
            var html = document.getElementById("esigHtmlCode").value || currentHtml();
            copyText(html).then(function (ok) {
                document.getElementById("esigCopyMsg").textContent = ok ? "کد HTML کپی شد." : "کپی نشد. کد را از کادر انتخاب کنید.";
            });
        }

        function copyText(text) {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                return navigator.clipboard.writeText(text).then(function () { return true; }).catch(function () { return false; });
            }
            return Promise.resolve(false);
        }

        function downloadHtml() {
            var html = document.getElementById("esigHtmlCode").value || currentHtml();
            var blob = new Blob([html], { type: "text/html;charset=utf-8" });
            var link = document.createElement("a");
            link.href = URL.createObjectURL(blob);
            link.download = "email-signature.html";
            link.click();
            setTimeout(function () { URL.revokeObjectURL(link.href); }, 1000);
        }
    }

    function stateFromSeed(seed) {
        var socials = {};
        SOCIALS.forEach(function (item) { socials[item.key] = (seed.socials && seed.socials[item.key]) || ""; });
        return {
            template: "classic",
            color: seed.color || "#5F41FB",
            fullName: seed.fullName || "",
            jobTitle: seed.jobTitle || "",
            brand: seed.brand || "",
            slogan: seed.slogan || "",
            mobile: seed.mobile || "",
            phone: seed.phone || "",
            phone2: seed.phone2 || "",
            phone2Title: seed.phone2Title || "شماره",
            phone3: seed.phone3 || "",
            phone3Title: seed.phone3Title || "شماره",
            email: seed.email || "",
            website: seed.website || "",
            address: seed.address || "",
            photo: seed.photo || "",
            showPhoto: !!seed.photo,
            socials: socials,
            extras: seed.extras || [],
            font: "tahoma",
            show: {},
            showCta: seed.showCta !== false && !!seed.ctaUrl,
            ctaLabel: seed.ctaLabel || "مشاهده کارت ویزیت",
            ctaUrl: seed.ctaUrl || seed.cardUrl || "",
            showDisclaimer: false,
            disclaimer: ""
        };
    }

    function fieldOptions(seed) {
        var items = [];
        function add(key, label, value) {
            if (String(value || "").trim()) items.push({ key: key, label: label });
        }
        add("photo", "لوگو", seed.photo);
        add("fullName", "نام", seed.fullName);
        add("jobTitle", "سمت", seed.jobTitle);
        add("brand", "برند", seed.brand);
        add("slogan", "شعار", seed.slogan);
        add("mobile", "موبایل", seed.mobile);
        add("phone", "تلفن", seed.phone);
        add("phone2", seed.phone2Title || "شماره", seed.phone2);
        add("phone3", seed.phone3Title || "شماره", seed.phone3);
        add("email", "ایمیل", seed.email);
        add("website", "وب‌سایت", seed.website);
        add("address", "آدرس", seed.address);
        SOCIALS.forEach(function (item) {
            add("social:" + item.key, item.label, seed.socials && seed.socials[item.key]);
        });
        (seed.extras || []).slice(0, 4).forEach(function (extra, index) {
            if (extra && extra.url && extra.label) items.push({ key: "extra:" + index, label: String(extra.label) });
        });
        if (seed.ctaUrl || seed.cardUrl) items.push({ key: "cta", label: "دکمه کارت ویزیت" });
        return items;
    }

    function ensureFont(id, onReady) {
        var spec = fontSpec(id);
        if (!spec.google) return;
        var linkId = "esig-gfont-" + spec.id;
        if (document.getElementById(linkId)) return;
        var link = document.createElement("link");
        link.id = linkId;
        link.rel = "stylesheet";
        link.href = "https://fonts.googleapis.com/css2?family=" + spec.google + "&display=swap";
        if (typeof onReady === "function") link.onload = onReady;
        document.head.appendChild(link);
    }

    function readChoice(key) {
        try {
            var data = JSON.parse(localStorage.getItem(key) || "null");
            if (!data || typeof data !== "object") return null;
            var show = data.show && typeof data.show === "object" ? data.show : null;
            return { template: data.template || "", color: data.color || "", font: data.font || "", show: show };
        } catch (e) {
            return null;
        }
    }

    function toLongHex(color) {
        var c = safeColor(color);
        if (c.length === 4) return "#" + c.slice(1).split("").map(function (ch) { return ch + ch; }).join("");
        return c;
    }

    root.dnBootEmailSignature = boot;
    if (typeof document !== "undefined") {
        if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", boot);
        else boot();
    }
})(typeof window !== "undefined" ? window : globalThis);
