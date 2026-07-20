// Interactive crop selector for the media upload editor.
// After drawing a selection you can move it or resize via handles (Photoshop-style) until save.
window.mediaEditor = (function () {
    const states = {};
    const HANDLE = 8;       // half-size of resize handle hit area (canvas px)
    const HANDLE_DRAW = 6;  // visual handle size
    const MIN_SIZE = 8;     // minimum crop side in canvas px

    function redraw(state) {
        const ctx = state.ctx;
        ctx.clearRect(0, 0, state.canvas.width, state.canvas.height);
        ctx.drawImage(state.img, state.offsetX, state.offsetY, state.drawWidth, state.drawHeight);

        if (!state.rect || state.rect.w <= 0 || state.rect.h <= 0) return;

        const r = state.rect;
        const imgL = state.offsetX;
        const imgT = state.offsetY;
        const imgR = state.offsetX + state.drawWidth;
        const imgB = state.offsetY + state.drawHeight;

        // Dim outside the crop (Photoshop-like)
        ctx.save();
        ctx.fillStyle = "rgba(0, 0, 0, 0.45)";
        // top
        ctx.fillRect(imgL, imgT, state.drawWidth, Math.max(0, r.y - imgT));
        // bottom
        ctx.fillRect(imgL, r.y + r.h, state.drawWidth, Math.max(0, imgB - (r.y + r.h)));
        // left
        ctx.fillRect(imgL, r.y, Math.max(0, r.x - imgL), r.h);
        // right
        ctx.fillRect(r.x + r.w, r.y, Math.max(0, imgR - (r.x + r.w)), r.h);

        ctx.strokeStyle = "#1976d2";
        ctx.lineWidth = 2;
        ctx.setLineDash([]);
        ctx.strokeRect(r.x, r.y, r.w, r.h);

        // Rule-of-thirds guides
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

        // Corner + edge handles
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

    function clampRectToImage(state, rect) {
        let { x, y, w, h } = rect;
        // Normalize negative sizes
        if (w < 0) { x += w; w = -w; }
        if (h < 0) { y += h; h = -h; }

        const left = state.offsetX;
        const top = state.offsetY;
        const right = state.offsetX + state.drawWidth;
        const bottom = state.offsetY + state.drawHeight;

        if (x < left) { w -= left - x; x = left; }
        if (y < top) { h -= top - y; y = top; }
        if (x + w > right) w = right - x;
        if (y + h > bottom) h = bottom - y;

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

        // Edge hit (thin strip) for resize without exact handle
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

    function applyResize(origin, mode, p) {
        let { x, y, w, h } = origin;
        const right = x + w;
        const bottom = y + h;

        if (mode.includes("e")) w = p.x - x;
        if (mode.includes("s")) h = p.y - y;
        if (mode.includes("w")) { w = right - p.x; x = p.x; }
        if (mode.includes("n")) { h = bottom - p.y; y = p.y; }

        // Keep minimum size by clamping against the opposite edge
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
                state.mode = hit; // resize handle id
                state.originRect = { ...state.rect };
                state.start = p;
            } else {
                // New selection
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
                const hover = hitTest(state, p);
                canvas.style.cursor = cursorFor(hover);
                return;
            }

            e.preventDefault();

            if (state.mode === "draw") {
                const rect = {
                    x: Math.min(state.start.x, p.x),
                    y: Math.min(state.start.y, p.y),
                    w: Math.abs(p.x - state.start.x),
                    h: Math.abs(p.y - state.start.y),
                };
                state.rect = clampRectToImage(state, rect);
            } else if (state.mode === "move" && state.originRect) {
                const dx = p.x - state.start.x;
                const dy = p.y - state.start.y;
                let next = {
                    x: state.originRect.x + dx,
                    y: state.originRect.y + dy,
                    w: state.originRect.w,
                    h: state.originRect.h,
                };
                // Keep full rect inside image bounds (don't shrink on move)
                const left = state.offsetX;
                const top = state.offsetY;
                const right = state.offsetX + state.drawWidth;
                const bottom = state.offsetY + state.drawHeight;
                if (next.x < left) next.x = left;
                if (next.y < top) next.y = top;
                if (next.x + next.w > right) next.x = right - next.w;
                if (next.y + next.h > bottom) next.y = bottom - next.h;
                state.rect = next;
            } else if (state.originRect) {
                const next = applyResize(state.originRect, state.mode, p);
                state.rect = clampRectToImage(state, next);
                // After clamp, re-enforce min size if possible
                if (state.rect.w < MIN_SIZE || state.rect.h < MIN_SIZE) {
                    const fixed = { ...state.rect };
                    if (fixed.w < MIN_SIZE) fixed.w = MIN_SIZE;
                    if (fixed.h < MIN_SIZE) fixed.h = MIN_SIZE;
                    state.rect = clampRectToImage(state, fixed);
                }
            }

            redraw(state);
        };

        const onUp = () => {
            if (!state.dragging) return;
            state.dragging = false;
            // Drop tiny accidental selections
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
            // Replace previous instance if re-init
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

    return { init, getCrop, clearCrop, dispose };
})();
