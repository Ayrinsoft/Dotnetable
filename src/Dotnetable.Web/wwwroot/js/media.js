/* =====================================================================
   Uploaded media, loaded only when it is seen or clicked (see MediaHtml.cs).
   - img[data-src] / [data-bg]: fetched when scrolled into view. Hidden tabs,
     collapsed sections and inactive carousel slides never intersect, so they
     stay unloaded until shown.
   - img.dn-zoom: the page shows the thumbnail; data-full opens on click.
   - .dn-video: a play button; the <video> is built from its <template> on click.
   ===================================================================== */
(function () {
    "use strict";

    var observer = "IntersectionObserver" in window
        ? new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                observer.unobserve(entry.target);
                load(entry.target);
            });
        }, { rootMargin: "200px 0px" })
        : null;

    function load(el) {
        var bg = el.getAttribute("data-bg");
        if (bg) {
            el.removeAttribute("data-bg");
            el.style.backgroundImage = "url(\"" + bg.replace(/"/g, "%22") + "\")";
            return;
        }

        var src = el.getAttribute("data-src");
        if (!src) return;

        // A thumbnail an old upload does not have yet: show the original instead.
        var fallback = el.getAttribute("data-fallback");
        if (fallback) {
            el.addEventListener("error", function onError() {
                el.removeEventListener("error", onError);
                el.src = fallback;
            });
        }

        var picture = el.parentElement && el.parentElement.tagName === "PICTURE" ? el.parentElement : null;
        if (picture) {
            picture.querySelectorAll("source[data-srcset]").forEach(function (source) {
                source.srcset = source.getAttribute("data-srcset");
                source.removeAttribute("data-srcset");
            });
        }

        var srcset = el.getAttribute("data-srcset");
        if (srcset) {
            el.srcset = srcset;
            el.removeAttribute("data-srcset");
        }
        el.removeAttribute("data-src");
        el.src = src;
    }

    function scan(root) {
        if (!root || !root.querySelectorAll) return;
        var items = root.querySelectorAll("img[data-src], [data-bg]");
        if (root.matches && root.matches("img[data-src], [data-bg]")) items = [root].concat(Array.prototype.slice.call(items));
        Array.prototype.forEach.call(items, function (el) {
            if (observer) observer.observe(el); else load(el);
        });
    }
    window.dnMedia = { scan: scan, load: load };

    scan(document);

    // Content added later (comments, script-rendered widgets) is picked up too.
    if ("MutationObserver" in window) {
        new MutationObserver(function (mutations) {
            mutations.forEach(function (m) {
                m.addedNodes.forEach(function (node) { if (node.nodeType === 1) scan(node); });
            });
        }).observe(document.body, { childList: true, subtree: true });
    }

    // A carousel slide becomes visible only mid-transition; start its image as the slide begins.
    document.addEventListener("slide.bs.carousel", function (e) {
        if (!e.relatedTarget) return;
        e.relatedTarget.querySelectorAll("img[data-src], [data-bg]").forEach(function (el) {
            if (observer) observer.unobserve(el);
            load(el);
        });
    });

    // --- Lightbox: the original only on click ---------------------------
    function openLightbox(url, alt) {
        var overlay = document.createElement("div");
        overlay.className = "site-lightbox-overlay";
        overlay.innerHTML = '<button type="button" class="site-lightbox-close" aria-label="Close">&times;</button>'
            + '<img class="site-lightbox-image" alt="" />';
        var img = overlay.querySelector(".site-lightbox-image");
        img.alt = alt || "";
        img.src = url;

        function close() {
            document.removeEventListener("keydown", onKeyDown);
            overlay.remove();
            document.body.classList.remove("site-lightbox-open");
        }
        function onKeyDown(e) { if (e.key === "Escape") close(); }

        overlay.querySelector(".site-lightbox-close").addEventListener("click", close);
        overlay.addEventListener("click", function (e) { if (e.target === overlay) close(); });
        document.addEventListener("keydown", onKeyDown);

        document.body.appendChild(overlay);
        document.body.classList.add("site-lightbox-open");
    }

    // --- Video: nothing is fetched before the visitor presses play ------
    function playVideo(wrapper) {
        var template = wrapper.querySelector("template");
        if (!template) return;
        var fragment = template.content.cloneNode(true);
        var video = fragment.querySelector("video");
        wrapper.replaceWith(fragment);
        if (video) {
            // Started by the visitor's own click, so playing right away is what they asked for.
            var played = video.play();
            if (played && played.catch) played.catch(function () {});
        }
    }

    document.addEventListener("click", function (e) {
        var zoom = e.target.closest("img.dn-zoom[data-full]");
        if (zoom) {
            e.preventDefault();
            openLightbox(zoom.getAttribute("data-full"), zoom.alt);
            return;
        }
        var video = e.target.closest(".dn-video");
        if (video) {
            e.preventDefault();
            playVideo(video);
        }
    });

    document.addEventListener("keydown", function (e) {
        if (e.key !== "Enter" && e.key !== " ") return;
        var video = e.target.closest && e.target.closest(".dn-video");
        if (!video) return;
        e.preventDefault();
        playVideo(video);
    });
})();
