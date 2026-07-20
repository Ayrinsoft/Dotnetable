// Interactive crop selector for the media upload editor.
// After drawing a selection you can move/resize it (Photoshop-style). Optional fixed aspect ratio.
window.mediaEditor = (function () {
    const states = {};
    const HANDLE = 8;
    const HANDLE_DRAW = 6;
    const MIN_SIZE = 8;

    function imageBounds(state) {
        return {
            left: state.offsetX,
            top: state.offsetY,
            right: state.offsetX + state.drawWidth,
            bottom: state.offsetY + state.drawHeight,
            width: state.drawWidth,
            height: state.drawHeight,
        };
    }

    function redraw(state) {
        const ctx = state.ctx;
        ctx.clearRect(0, 0, state.canvas.width, state.canvas.height);
        ctx.drawImage(state.img, state.offsetX, state.offsetY, state.drawWidth, state.drawHeight);

        if (!state.rect || state.rect.w <= 0 || state.rect.h <= 0) return;

        const r = state.rect;
        const b = imageBounds(state);

        ctx.save();
        ctx.fillStyle = "rgba(0, 0, 0, 0.45)";
        ctx.fillRect(b.left, b.top, b.width, Math.max(0, r.y - b.top));
        ctx.fillRect(b.left, r.y + r.h, b.width, Math.max(0, b.bottom - (r.y + r.h)));
        ctx.fillRect(b.left, r.y, Math.max(0, r.x - b.left), r.h);
        ctx.fillRect(r.x + r.w, r.y, Math.max(0, b.right - (r.x + r.w)), r.h);

        ctx.strokeStyle = "#1976d2";
        ctx.lineWidth = 2;
        ctx.setLineDash([]);
        ctx.strokeRect(r.x, r.y, r.w, r.h);

        ctx.strokeStyle = "rgba(255,255,255,0.35)";
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.moveTo(r.x + r.w / 3, r.y);
        ctx.lineTo(r.x + r.w / 3, r.y + r.h);
        ctx.moveTo(r.x + (2 * r.w) / 3, r.y);
        ctx.lineTo(r.x + (2 * r.w) / 3, r.y + r.h);
        ctx.moveTo(r.x, r.y + r.h / 3);
        ctx.lineTo(r.x + r.w, r.y + r.h / 3);
        ctx.moveTo(r.x, r.y + (2 * r.h) / 3);
        ctx.lineTo(r.x + r.w, r.y + (2 * r.h) / 3);
        ctx.stroke();

        ctx.fillStyle = "#fff";
        ctx.strokeStyle = "#1976d2";
        ctx.lineWidth = 1.5;
        for (const h of handlePoints(r)) {
            const s = HANDLE_DRAW;
            ctx.fillRect(h.x - s / 2, h.y - s / 2, s, s);
            ctx.strokeRect(h.x - s / 2, h.y - s / 2, s, s);
        }
        ctx.restore();
    }

    function handlePoints(r) {
        const mx = r.x + r.w / 2;
        const my = r.y + r.h / 2;
        return [
            { id: "nw", x: r.x, y: r.y },
            { id: "n", x: mx, y: r.y },
            { id: "ne", x: r.x + r.w, y: r.y },
            { id: "e", x: r.x + r.w, y: my },
            { id: "se", x: r.x + r.w, y: r.y + r.h },
            { id: "s", x: mx, y: r.y + r.h },
            { id: "sw", x: r.x, y: r.y + r.h },
            { id: "w", x: r.x, y: my },
        ];
    }

    /** Fit w×h into image; if aspect is set, keep ratio and shrink as needed. */
    function placeRect(state, x, y, w, h) {
        const b = imageBounds(state);
        const ar = state.aspectRatio;

        if (w < 0) { x += w; w = -w; }
        if (h < 0) { y += h; h = -h; }

        if (ar && ar > 0) {
            h = w / ar;
            if (w > b.width) { w = b.width; h = w / ar; }
            if (h > b.height) { h = b.height; w = h * ar; }
            if (w > b.width) { w = b.width; h = w / ar; }
        } else {
            if (w > b.width) w = b.width;
            if (h > b.height) h = b.height;
        }

        if (x < b.left) x = b.left;
        if (y < b.top) y = b.top;
        if (x + w > b.right) x = b.right - w;
        if (y + h > b.bottom) y = b.bottom - h;

        return { x, y, w: Math.max(0, w), h: Math.max(0, h) };
    }

    function pointerPos(state, evt) {
        const bounds = state.canvas.getBoundingClientRect();
        const scaleX = state.canvas.width / bounds.width;
        const scaleY = state.canvas.height / bounds.height;
        const src = evt.touches && evt.touches.length ? evt.touches[0]
            : evt.changedTouches && evt.changedTouches.length ? evt.changedTouches[0]
            : evt;
        return { x: (src.clientX - bounds.left) * scaleX, y: (src.clientY - bounds.top) * scaleY };
    }

    function hitTest(state, p) {
        const r = state.rect;
        if (!r || r.w < 1 || r.h < 1) return null;

        for (const h of handlePoints(r)) {
            if (Math.abs(p.x - h.x) <= HANDLE && Math.abs(p.y - h.y) <= HANDLE) {
                return h.id;
            }
        }

        const nearL = Math.abs(p.x - r.x) <= HANDLE && p.y >= r.y - HANDLE && p.y <= r.y + r.h + HANDLE;
        const nearR = Math.abs(p.x - (r.x + r.w)) <= HANDLE && p.y >= r.y - HANDLE && p.y <= r.y + r.h + HANDLE;
        const nearT = Math.abs(p.y - r.y) <= HANDLE && p.x >= r.x - HANDLE && p.x <= r.x + r.w + HANDLE;
        const nearB = Math.abs(p.y - (r.y + r.h)) <= HANDLE && p.x >= r.x - HANDLE && p.x <= r.x + r.w + HANDLE;
        if (nearT && nearL) return "nw";
        if (nearT && nearR) return "ne";
        if (nearB && nearL) return "sw";
        if (nearB && nearR) return "se";
        if (nearT) return "n";
        if (nearB) return "s";
        if (nearL) return "w";
        if (nearR) return "e";

        if (p.x >= r.x && p.x <= r.x + r.w && p.y >= r.y && p.y <= r.y + r.h) {
            return "move";
        }
        return null;
    }

    function cursorFor(mode) {
        switch (mode) {
            case "n":
            case "s": return "ns-resize";
            case "e":
            case "w": return "ew-resize";
            case "nw":
            case "se": return "nwse-resize";
            case "ne":
            case "sw": return "nesw-resize";
            case "move": return "move";
            default: return "crosshair";
        }
    }

    function sizeFromDrag(start, p, aspect) {
        let dx = p.x - start.x;
        let dy = p.y - start.y;
        let w = Math.abs(dx);
        let h = Math.abs(dy);

        if (aspect && aspect > 0) {
            // Fit the aspect-ratio box inside the free drag rectangle
            if (w / aspect <= h || h === 0) {
                h = w / aspect;
            } else {
                w = h * aspect;
            }
            if (w < MIN_SIZE) { w = MIN_SIZE; h = w / aspect; }
            if (h < MIN_SIZE) { h = MIN_SIZE; w = h * aspect; }
        }

        const x = dx >= 0 ? start.x : start.x - w;
        const y = dy >= 0 ? start.y : start.y - h;
        return { x, y, w, h };
    }

    function applyResize(origin, mode, p, aspect) {
        const right = origin.x + origin.w;
        const bottom = origin.y + origin.h;
        const left = origin.x;
        const top = origin.y;

        if (!aspect || aspect <= 0) {
            let x = left, y = top, w = origin.w, h = origin.h;
            if (mode.includes("e")) w = p.x - left;
            if (mode.includes("s")) h = p.y - top;
            if (mode.includes("w")) { w = right - p.x; x = p.x; }
            if (mode.includes("n")) { h = bottom - p.y; y = p.y; }
            if (w < MIN_SIZE) {
                if (mode.includes("w")) x = right - MIN_SIZE;
                w = MIN_SIZE;
            }
            if (h < MIN_SIZE) {
                if (mode.includes("n")) y = bottom - MIN_SIZE;
                h = MIN_SIZE;
            }
            return { x, y, w, h };
        }

        // Fixed aspect: opposite edge/corner is the anchor
        let ax, ay;
        if (mode.includes("w")) ax = right;
        else if (mode.includes("e")) ax = left;
        else ax = left + origin.w / 2;

        if (mode.includes("n")) ay = bottom;
        else if (mode.includes("s")) ay = top;
        else ay = top + origin.h / 2;

        let w, h, x, y;

        if (mode === "n" || mode === "s") {
            h = Math.max(MIN_SIZE, Math.abs(p.y - ay));
            w = h * aspect;
            y = mode === "s" ? ay : ay - h;
            x = ax - w / 2;
        } else if (mode === "e" || mode === "w") {
            w = Math.max(MIN_SIZE, Math.abs(p.x - ax));
            h = w / aspect;
            x = mode === "e" ? ax : ax - w;
            y = ay - h / 2;
        } else {
            // Corner: grow so both dimensions match aspect toward the pointer
            const rawW = Math.max(MIN_SIZE, Math.abs(p.x - ax));
            const rawH = Math.max(MIN_SIZE, Math.abs(p.y - ay));
            if (rawW / rawH > aspect) {
                h = rawH;
                w = h * aspect;
            } else {
                w = rawW;
                h = w / aspect;
            }
            x = (mode.includes("e") || p.x >= ax) && !mode.includes("w") ? ax : ax - w;
            // Prefer handle direction over pointer when both encoded in mode
            if (mode.includes("e")) x = ax;
            if (mode.includes("w")) x = ax - w;
            if (mode.includes("s")) y = ay;
            else y = ay - h;
        }

        return { x, y, w, h };
    }

    /** Resize existing rect to match aspect, keeping center, max size that still fits image. */
    function reframeToAspect(state) {
        if (!state.rect || !state.aspectRatio || state.aspectRatio <= 0) return;
        const ar = state.aspectRatio;
        const b = imageBounds(state);
        const r = state.rect;
        const cx = r.x + r.w / 2;
        const cy = r.y + r.h / 2;

        // Largest rect with this aspect that fits in the image
        let w = b.width;
        let h = w / ar;
        if (h > b.height) {
            h = b.height;
            w = h * ar;
        }
        // Prefer not to grow beyond previous size when possible? Use max of current area-ish:
        // Keep roughly current max dimension when possible
        const curMax = Math.max(r.w, r.h * ar);
        if (curMax > 0 && curMax < w) {
            w = curMax;
            h = w / ar;
            if (h < MIN_SIZE) { h = MIN_SIZE; w = h * ar; }
        }

        state.rect = placeRect(state, cx - w / 2, cy - h / 2, w, h);
        redraw(state);
    }

    function bindEvents(state) {
        const canvas = state.canvas;

        const onDown = (e) => {
            e.preventDefault();
            const p = pointerPos(state, e);
            const hit = hitTest(state, p);

            if (hit === "move") {
                state.mode = "move";
                state.originRect = { ...state.rect };
                state.start = p;
            } else if (hit) {
                state.mode = hit;
                state.originRect = { ...state.rect };
                state.start = p;
            } else {
                state.mode = "draw";
                state.start = p;
                state.originRect = null;
                state.rect = { x: p.x, y: p.y, w: 0, h: 0 };
            }
            state.dragging = true;
            canvas.style.cursor = cursorFor(state.mode === "draw" ? null : state.mode);
        };

        const onMove = (e) => {
            const p = pointerPos(state, e);

            if (!state.dragging) {
                canvas.style.cursor = cursorFor(hitTest(state, p));
                return;
            }

            e.preventDefault();
            const ar = state.aspectRatio;

            if (state.mode === "draw") {
                const raw = sizeFromDrag(state.start, p, ar);
                state.rect = placeRect(state, raw.x, raw.y, raw.w, raw.h);
            } else if (state.mode === "move" && state.originRect) {
                const dx = p.x - state.start.x;
                const dy = p.y - state.start.y;
                const b = imageBounds(state);
                let next = {
                    x: state.originRect.x + dx,
                    y: state.originRect.y + dy,
                    w: state.originRect.w,
                    h: state.originRect.h,
                };
                if (next.x < b.left) next.x = b.left;
                if (next.y < b.top) next.y = b.top;
                if (next.x + next.w > b.right) next.x = b.right - next.w;
                if (next.y + next.h > b.bottom) next.y = b.bottom - next.h;
                state.rect = next;
            } else if (state.originRect) {
                const raw = applyResize(state.originRect, state.mode, p, ar);
                state.rect = placeRect(state, raw.x, raw.y, raw.w, raw.h);
            }

            redraw(state);
        };

        const onUp = () => {
            if (!state.dragging) return;
            state.dragging = false;
            if (state.rect && (state.rect.w < 4 || state.rect.h < 4)) {
                state.rect = null;
                redraw(state);
            }
            state.mode = null;
            state.originRect = null;
            state.start = null;
            canvas.style.cursor = "crosshair";
        };

        canvas.addEventListener("mousedown", onDown);
        canvas.addEventListener("mousemove", onMove);
        window.addEventListener("mouseup", onUp);
        canvas.addEventListener("touchstart", onDown, { passive: false });
        canvas.addEventListener("touchmove", onMove, { passive: false });
        window.addEventListener("touchend", onUp);

        state.listeners = { onDown, onMove, onUp };
    }

    function init(canvasId, imageDataUrl, maxWidth, maxHeight) {
        return new Promise((resolve) => {
            if (states[canvasId]) dispose(canvasId);

            const canvas = document.getElementById(canvasId);
            if (!canvas) { resolve(false); return; }
            const ctx = canvas.getContext("2d");
            const img = new Image();
            img.onload = () => {
                const scale = Math.min(maxWidth / img.width, maxHeight / img.height, 1);
                const drawWidth = Math.round(img.width * scale);
                const drawHeight = Math.round(img.height * scale);
                canvas.width = maxWidth;
                canvas.height = maxHeight;
                const offsetX = Math.round((maxWidth - drawWidth) / 2);
                const offsetY = Math.round((maxHeight - drawHeight) / 2);

                const state = {
                    canvas, ctx, img, drawWidth, drawHeight, offsetX, offsetY,
                    rect: null, dragging: false, mode: null, start: null, originRect: null,
                    aspectRatio: null,
                };
                states[canvasId] = state;
                canvas.style.cursor = "crosshair";
                redraw(state);
                bindEvents(state);
                resolve(true);
            };
            img.onerror = () => resolve(false);
            img.src = imageDataUrl;
        });
    }

    /**
     * @param {string} canvasId
     * @param {number|null|undefined} ratio width/height (e.g. 1, 16/9). null/0 = free
     * @param {boolean} [createIfEmpty] when true and no selection, create centered crop with this ratio
     */
    function setAspectRatio(canvasId, ratio, createIfEmpty) {
        const state = states[canvasId];
        if (!state) return;

        const ar = ratio && ratio > 0 ? ratio : null;
        state.aspectRatio = ar;

        if (ar) {
            if (state.rect && state.rect.w >= 4 && state.rect.h >= 4) {
                reframeToAspect(state);
            } else if (createIfEmpty) {
                const b = imageBounds(state);
                let w = b.width * 0.8;
                let h = w / ar;
                if (h > b.height * 0.8) {
                    h = b.height * 0.8;
                    w = h * ar;
                }
                state.rect = placeRect(state, b.left + (b.width - w) / 2, b.top + (b.height - h) / 2, w, h);
                redraw(state);
            }
        }
    }

    function getCrop(canvasId) {
        const state = states[canvasId];
        if (!state || !state.rect || state.rect.w < 4 || state.rect.h < 4) return null;
        const r = state.rect;
        return {
            x: (r.x - state.offsetX) / state.drawWidth,
            y: (r.y - state.offsetY) / state.drawHeight,
            width: r.w / state.drawWidth,
            height: r.h / state.drawHeight,
        };
    }

    function clearCrop(canvasId) {
        const state = states[canvasId];
        if (!state) return;
        state.rect = null;
        state.dragging = false;
        state.mode = null;
        redraw(state);
    }

    function dispose(canvasId) {
        const state = states[canvasId];
        if (!state) return;
        const { onDown, onMove, onUp } = state.listeners || {};
        if (onDown) {
            state.canvas.removeEventListener("mousedown", onDown);
            state.canvas.removeEventListener("touchstart", onDown);
        }
        if (onMove) {
            state.canvas.removeEventListener("mousemove", onMove);
            state.canvas.removeEventListener("touchmove", onMove);
        }
        if (onUp) {
            window.removeEventListener("mouseup", onUp);
            window.removeEventListener("touchend", onUp);
        }
        delete states[canvasId];
    }

    return { init, getCrop, clearCrop, dispose, setAspectRatio };
})();
