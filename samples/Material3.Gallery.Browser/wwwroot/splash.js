// Boot splash shown while the .NET runtime downloads and starts. One indicator variant is picked
// at random per page load and rendered in a worker via OffscreenCanvas, so the animation stays
// smooth while the main thread is blocked by wasm download/compile/instantiate.
import { draw, LIGHT, DARK, VARIANTS } from './splash-draw.js';

const canvas = document.getElementById('splash-canvas');
const variant = VARIANTS[Math.floor(Math.random() * VARIANTS.length)];

const darkQuery = window.matchMedia('(prefers-color-scheme: dark)');

const dpr = window.devicePixelRatio || 1;
canvas.style.width = variant.width + 'px';
canvas.style.height = variant.height + 'px';
canvas.width = Math.round(variant.width * dpr);
canvas.height = Math.round(variant.height * dpr);

let stop = () => {};

if (typeof canvas.transferControlToOffscreen === 'function' && typeof Worker === 'function') {
    const worker = new Worker('./splash-worker.js', { type: 'module' });
    const offscreen = canvas.transferControlToOffscreen();
    worker.postMessage({ type: 'start', canvas: offscreen, variant, dpr, dark: darkQuery.matches }, [offscreen]);
    darkQuery.addEventListener('change', e => worker.postMessage({ type: 'scheme', dark: e.matches }));
    stop = () => {
        worker.postMessage({ type: 'stop' });
        worker.terminate();
    };
} else {
    // Fallback: render on the main thread (animation may stutter while wasm loads).
    const ctx = canvas.getContext('2d');
    ctx.scale(dpr, dpr);
    let scheme = darkQuery.matches ? DARK : LIGHT;
    darkQuery.addEventListener('change', e => {
        scheme = e.matches ? DARK : LIGHT;
    });

    const start = performance.now();
    let running = true;
    const frame = now => {
        if (!running) return;
        ctx.clearRect(0, 0, variant.width, variant.height);
        draw(ctx, scheme, variant.name, now - start, variant.width, variant.height);
        requestAnimationFrame(frame);
    };
    requestAnimationFrame(frame);
    stop = () => {
        running = false;
    };
}

globalThis.m3Splash = {
    hide() {
        const splash = document.getElementById('splash');
        if (!splash) return;
        splash.classList.add('splash--hidden');
        setTimeout(() => {
            stop();
            splash.remove();
        }, 300);
    },
};
