// Minimal mouse-driven crop selector for the media upload editor. No external dependencies.
window.mediaEditor = (function () {
    const states = {};

    function redraw(state) {
        const ctx = state.ctx;
        ctx.clearRect(0, 0, state.canvas.width, state.canvas.height);
        ctx.drawImage(state.img, state.offsetX, state.offsetY, state.drawWidth, state.drawHeight);
        if (state.rect) {
            ctx.save();
            ctx.strokeStyle = "#1976d2";
            ctx.lineWidth = 2;
            ctx.setLineDash([6, 4]);
            ctx.strokeRect(state.rect.x, state.rect.y, state.rect.w, state.rect.h);
            ctx.fillStyle = "rgba(25, 118, 210, 0.15)";
            ctx.fillRect(state.rect.x, state.rect.y, state.rect.w, state.rect.h);
            ctx.restore();
        }
    }

    function clampRectToImage(state, rect) {
        const left = Math.max(rect.x, state.offsetX);
        const top = Math.max(rect.y, state.offsetY);
        const right = Math.min(rect.x + rect.w, state.offsetX + state.drawWidth);
        const bottom = Math.min(rect.y + rect.h, state.offsetY + state.drawHeight);
        return { x: left, y: top, w: Math.max(0, right - left), h: Math.max(0, bottom - top) };
    }

    function pointerPos(state, evt) {
        const bounds = state.canvas.getBoundingClientRect();
        const scaleX = state.canvas.width / bounds.width;
        const scaleY = state.canvas.height / bounds.height;
        const clientX = evt.touches ? evt.touches[0].clientX : evt.clientX;
        const clientY = evt.touches ? evt.touches[0].clientY : evt.clientY;
        return { x: (clientX - bounds.left) * scaleX, y: (clientY - bounds.top) * scaleY };
    }

    function init(canvasId, imageDataUrl, maxWidth, maxHeight) {
        return new Promise((resolve) => {
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

                const state = { canvas, ctx, img, drawWidth, drawHeight, offsetX, offsetY, rect: null, dragging: false };
                states[canvasId] = state;
                redraw(state);

                let start = null;
                const onDown = (e) => {
                    e.preventDefault();
                    start = pointerPos(state, e);
                    state.dragging = true;
                    state.rect = { x: start.x, y: start.y, w: 0, h: 0 };
                };
                const onMove = (e) => {
                    if (!state.dragging) return;
                    const p = pointerPos(state, e);
                    const rect = {
                        x: Math.min(start.x, p.x),
                        y: Math.min(start.y, p.y),
                        w: Math.abs(p.x - start.x),
                        h: Math.abs(p.y - start.y),
                    };
                    state.rect = clampRectToImage(state, rect);
                    redraw(state);
                };
                const onUp = () => { state.dragging = false; };

                canvas.addEventListener("mousedown", onDown);
                canvas.addEventListener("mousemove", onMove);
                window.addEventListener("mouseup", onUp);
                canvas.addEventListener("touchstart", onDown, { passive: false });
                canvas.addEventListener("touchmove", onMove, { passive: false });
                window.addEventListener("touchend", onUp);

                state.listeners = { onDown, onMove, onUp };
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
        redraw(state);
    }

    function dispose(canvasId) {
        const state = states[canvasId];
        if (!state) return;
        const { onDown, onMove, onUp } = state.listeners || {};
        if (onDown) state.canvas.removeEventListener("mousedown", onDown);
        if (onMove) state.canvas.removeEventListener("mousemove", onMove);
        if (onUp) { window.removeEventListener("mouseup", onUp); window.removeEventListener("touchend", onUp); }
        if (onDown) state.canvas.removeEventListener("touchstart", onDown);
        if (onMove) state.canvas.removeEventListener("touchmove", onMove);
        delete states[canvasId];
    }

    return { init, getCrop, clearCrop, dispose };
})();
