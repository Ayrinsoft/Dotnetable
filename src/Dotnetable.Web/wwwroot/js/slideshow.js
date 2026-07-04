// Lightweight click-to-zoom lightbox for .site-slideshow-lightbox links (see slideshow.css).
// Bootstrap's own JS (already loaded) drives the carousel itself via data-bs-* attributes;
// this only adds the fancybox-style overlay on top of the active slide's image.
(function () {
    function gallery(group) {
        var links = document.querySelectorAll('.site-slideshow-lightbox[data-lightbox-group="' + group + '"]');
        return Array.prototype.map.call(links, function (a) {
            var img = a.querySelector('img');
            return { href: a.getAttribute('href'), alt: img ? img.alt : '' };
        });
    }

    function open(group, startIndex) {
        var items = gallery(group);
        if (items.length === 0) return;

        var overlay = document.createElement('div');
        overlay.className = 'site-lightbox-overlay';

        var closeBtn = '<button type="button" class="site-lightbox-close" aria-label="Close">&times;</button>';
        var prevBtn = items.length > 1 ? '<button type="button" class="site-lightbox-prev" aria-label="Previous">&#10094;</button>' : '';
        var nextBtn = items.length > 1 ? '<button type="button" class="site-lightbox-next" aria-label="Next">&#10095;</button>' : '';
        overlay.innerHTML = closeBtn + prevBtn + '<img class="site-lightbox-image" src="" alt="" />' + nextBtn;

        document.body.appendChild(overlay);
        document.body.classList.add('site-lightbox-open');

        var img = overlay.querySelector('.site-lightbox-image');
        var current = startIndex;

        function render() {
            img.src = items[current].href;
            img.alt = items[current].alt || '';
        }

        function close() {
            document.removeEventListener('keydown', onKeyDown);
            overlay.remove();
            document.body.classList.remove('site-lightbox-open');
        }

        function prev() { current = (current - 1 + items.length) % items.length; render(); }
        function next() { current = (current + 1) % items.length; render(); }

        function onKeyDown(e) {
            if (e.key === 'Escape') close();
            else if (e.key === 'ArrowLeft') prev();
            else if (e.key === 'ArrowRight') next();
        }

        overlay.querySelector('.site-lightbox-close').addEventListener('click', close);
        overlay.addEventListener('click', function (e) { if (e.target === overlay) close(); });

        var prevEl = overlay.querySelector('.site-lightbox-prev');
        var nextEl = overlay.querySelector('.site-lightbox-next');
        if (prevEl) prevEl.addEventListener('click', prev);
        if (nextEl) nextEl.addEventListener('click', next);

        document.addEventListener('keydown', onKeyDown);
        render();
    }

    document.addEventListener('click', function (e) {
        var link = e.target.closest('.site-slideshow-lightbox');
        if (!link) return;
        e.preventDefault();

        var group = link.getAttribute('data-lightbox-group');
        var links = Array.prototype.slice.call(
            document.querySelectorAll('.site-slideshow-lightbox[data-lightbox-group="' + group + '"]'));
        var index = links.indexOf(link);
        open(group, index < 0 ? 0 : index);
    });
})();
