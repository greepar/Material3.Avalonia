// Renders the boot splash off the main thread, so the animation keeps running smoothly while
// the main thread is busy downloading, compiling and instantiating the .NET wasm runtime.
import { draw, LIGHT, DARK } from './splash-draw.js';

let ctx = null;
let variant = null;
let scheme = LIGHT;
// In a worker, requestAnimationFrame timestamps come from the document's time origin rather than
// the worker's own performance.now(), so the first frame defines the origin instead.
let start = null;
let running = false;

function frame(now) {
    if (!running) return;
    start ??= now;
    ctx.clearRect(0, 0, variant.width, variant.height);
    draw(ctx, scheme, variant.name, now - start, variant.width, variant.height);
    requestAnimationFrame(frame);
}

self.onmessage = e => {
    const msg = e.data;
    if (msg.type === 'start') {
        variant = msg.variant;
        scheme = msg.dark ? DARK : LIGHT;
        ctx = msg.canvas.getContext('2d');
        ctx.scale(msg.dpr, msg.dpr);
        running = true;
        requestAnimationFrame(frame);
    } else if (msg.type === 'scheme') {
        scheme = msg.dark ? DARK : LIGHT;
    } else if (msg.type === 'stop') {
        running = false;
    }
};
