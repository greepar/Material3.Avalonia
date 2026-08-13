// Canvas ports of the Material3.Avalonia progress indicators. No DOM access, so this module
// can run either on the main thread or inside the splash worker.

// M3 baseline primary / secondaryContainer for seed #6750A4, light and dark.
export const LIGHT = { primary: '#6750A4', track: '#E8DEF8' };
export const DARK = { primary: '#D0BCFF', track: '#4A4458' };

export const VARIANTS = [
    { name: 'loading-indicator', width: 48, height: 48 },
    { name: 'circular-wavy', width: 48, height: 48 },
    { name: 'wavy-bar', width: 240, height: 16 },
];

// ---- LoadingIndicator: polar radius sampling + morphing (port of LoadingIndicator.cs) ----
const SAMPLES = 360;
const SEGMENT_MS = 650;
const ACTIVE_MS = SEGMENT_MS * 0.72;
const GLOBAL_ROTATION_MS = 4666;
const SEGMENT_ROTATION_DEG = 90;
const SCALE_AMPLITUDE = 0.12;

function star(n, outer, inner) {
    const pts = [];
    for (let i = 0; i < 2 * n; i++) {
        const r = i % 2 === 0 ? outer : inner;
        const a = Math.PI / n * i;
        pts.push([r * Math.cos(a), r * Math.sin(a)]);
    }
    return pts;
}

function regular(n, radius) {
    const pts = [];
    for (let i = 0; i < n; i++) {
        const a = 2 * Math.PI / n * i;
        pts.push([radius * Math.cos(a), radius * Math.sin(a)]);
    }
    return pts;
}

// Ray-casts from the origin at each grid angle; every shape here is star-shaped w.r.t. the origin.
function sampleSharp(vertices) {
    const out = new Float64Array(SAMPLES);
    for (let i = 0; i < SAMPLES; i++) {
        const theta = 2 * Math.PI * i / SAMPLES;
        const dx = Math.cos(theta), dy = Math.sin(theta);
        let best = 0;
        for (let v = 0; v < vertices.length; v++) {
            const a = vertices[v], b = vertices[(v + 1) % vertices.length];
            const ex = b[0] - a[0], ey = b[1] - a[1];
            const denom = dx * ey - dy * ex;
            if (Math.abs(denom) < 1e-12) continue;
            const t = (a[0] * ey - a[1] * ex) / denom;
            const s = (a[0] * dy - a[1] * dx) / denom;
            if (t > 0 && s >= -1e-9 && s <= 1 + 1e-9 && t > best) best = t;
        }
        out[i] = best;
    }
    return out;
}

// Two moving-average passes over r(theta) approximate corner rounding.
function smooth(radii, window) {
    const half = Math.max(1, window >> 1);
    let current = radii;
    for (let pass = 0; pass < 2; pass++) {
        const next = new Float64Array(SAMPLES);
        const span = 2 * half + 1;
        for (let i = 0; i < SAMPLES; i++) {
            let sum = 0;
            for (let j = -half; j <= half; j++) sum += current[(i + j + SAMPLES) % SAMPLES];
            next[i] = sum / span;
        }
        current = next;
    }
    return current;
}

function shift(radii, degrees) {
    const out = new Float64Array(SAMPLES);
    for (let i = 0; i < SAMPLES; i++) out[i] = radii[((i - degrees) % SAMPLES + SAMPLES) % SAMPLES];
    return out;
}

function normalize(radii) {
    let minX = Infinity, maxX = -Infinity, minY = Infinity, maxY = -Infinity;
    for (let i = 0; i < SAMPLES; i++) {
        const theta = 2 * Math.PI * i / SAMPLES;
        const x = radii[i] * Math.cos(theta), y = radii[i] * Math.sin(theta);
        if (x < minX) minX = x;
        if (x > maxX) maxX = x;
        if (y < minY) minY = y;
        if (y > maxY) maxY = y;
    }
    const scale = 1 / Math.max(maxX - minX, maxY - minY);
    for (let i = 0; i < SAMPLES; i++) radii[i] *= scale;
    return radii;
}

function pillRadii() {
    const capRadius = 0.5, capCenter = 0.125;
    const out = new Float64Array(SAMPLES);
    for (let i = 0; i < SAMPLES; i++) {
        const theta = 2 * Math.PI * i / SAMPLES;
        const cos = Math.cos(theta), sin = Math.sin(theta);
        const flat = Math.abs(sin) < 1e-9 ? Infinity : capRadius / Math.abs(sin);
        if (flat * Math.abs(cos) <= capCenter + 1e-9) {
            out[i] = flat;
            continue;
        }
        const c = cos >= 0 ? capCenter : -capCenter;
        out[i] = c * cos + Math.sqrt(capRadius * capRadius - c * c * sin * sin);
    }
    return out;
}

function ovalRadii() {
    const a = 1, b = 0.7;
    const out = new Float64Array(SAMPLES);
    for (let i = 0; i < SAMPLES; i++) {
        const theta = 2 * Math.PI * i / SAMPLES;
        out[i] = a * b / Math.sqrt(b * b * Math.cos(theta) ** 2 + a * a * Math.sin(theta) ** 2);
    }
    return out;
}

const win = (rounding, corners) => Math.max(2, Math.round(rounding * SAMPLES / corners));

let shapes = null;
function getShapes() {
    if (!shapes) {
        shapes = [
            normalize(shift(smooth(sampleSharp(star(10, 1, 0.65)), win(0.1, 20)), 18)),
            normalize(shift(smooth(sampleSharp(star(9, 1, 0.8)), win(0.5, 18)), -90)),
            normalize(shift(smooth(sampleSharp(regular(5, 1)), win(0.3, 5)), -18)),
            normalize(shift(pillRadii(), -45)),
            normalize(smooth(sampleSharp(star(8, 1, 0.8)), win(0.15, 16))),
            normalize(shift(smooth(sampleSharp(star(4, 1, 0.5)), win(0.3, 8)), -45)),
            normalize(shift(ovalRadii(), -45)),
        ];
    }
    return shapes;
}

// cubic-bezier(0.38, 1.21, 0.22, 1.0), solved by bisection on x.
function morphEase(t) {
    if (t <= 0) return 0;
    if (t >= 1) return 1;
    const bezier = (u, c1, c2) => {
        const v = 1 - u;
        return 3 * v * v * u * c1 + 3 * v * u * u * c2 + u * u * u;
    };
    let lo = 0, hi = 1;
    for (let i = 0; i < 32; i++) {
        const mid = (lo + hi) / 2;
        if (bezier(mid, 0.38, 0.22) < t) lo = mid; else hi = mid;
    }
    return bezier((lo + hi) / 2, 1.21, 1.0);
}

export function drawLoadingIndicator(ctx, scheme, elapsedMs, w, h) {
    const s = getShapes();
    const segIndex = Math.max(0, Math.floor(elapsedMs / SEGMENT_MS));
    const eased = morphEase(Math.min((elapsedMs - segIndex * SEGMENT_MS) / ACTIVE_MS, 1));
    const from = s[segIndex % s.length];
    const to = s[(segIndex + 1) % s.length];

    const pulse = 1 + SCALE_AMPLITUDE * Math.sin(Math.PI * Math.min(Math.max(eased, 0), 1)) ** 2;
    const rotation = (elapsedMs / GLOBAL_ROTATION_MS * 360 + (segIndex + eased) * SEGMENT_ROTATION_DEG) * Math.PI / 180;
    const radiusScale = Math.min(w, h) * (38 / 48) * pulse;

    ctx.save();
    ctx.translate(w / 2, h / 2);
    ctx.rotate(rotation);
    ctx.beginPath();
    for (let i = 0; i < SAMPLES; i++) {
        const r = (from[i] + (to[i] - from[i]) * eased) * radiusScale;
        const theta = 2 * Math.PI * i / SAMPLES;
        const x = r * Math.cos(theta), y = r * Math.sin(theta);
        if (i === 0) ctx.moveTo(x, y); else ctx.lineTo(x, y);
    }
    ctx.closePath();
    ctx.fillStyle = scheme.primary;
    ctx.fill();
    ctx.restore();
}

// ---- CircularProgressIndicator (wavy) ----
const ROTATION_SECONDS = 1.33;
const CYCLE_SECONDS = 1.333;
const MIN_SWEEP_DEG = 10;
const MAX_SWEEP_DEG = 270;
const easeInOut = t => t < 0.5 ? 2 * t * t : 1 - Math.pow(-2 * t + 2, 2) / 2;

export function drawCircular(ctx, scheme, elapsed, w, h) {
    const stroke = 4;
    const amplitude = 2;
    const radius = (Math.min(w, h) - stroke) / 2 - amplitude;
    if (radius <= 0) return;

    const cycle = elapsed / CYCLE_SECONDS;
    const frac = cycle - Math.floor(cycle);
    const grow = MAX_SWEEP_DEG - MIN_SWEEP_DEG;
    let sweep, startOffset;
    if (frac < 0.5) {
        sweep = MIN_SWEEP_DEG + grow * easeInOut(frac * 2);
        startOffset = 0;
    } else {
        const p = easeInOut((frac - 0.5) * 2);
        sweep = MAX_SWEEP_DEG - grow * p;
        startOffset = grow * p;
    }
    const start = -90 + elapsed / ROTATION_SECONDS * 360 + Math.floor(cycle) * grow + startOffset;

    ctx.save();
    ctx.translate(w / 2, h / 2);
    ctx.lineWidth = stroke;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
    ctx.strokeStyle = scheme.primary;
    ctx.beginPath();
    const phase = elapsed * 1.5;
    const waveCount = 12;
    for (let a = 0; a <= sweep; a += 2) {
        const rad = (start + Math.min(a, sweep)) * Math.PI / 180;
        const r = radius + amplitude * Math.sin(waveCount * rad + phase);
        const x = r * Math.cos(rad), y = r * Math.sin(rad);
        if (a === 0) ctx.moveTo(x, y); else ctx.lineTo(x, y);
    }
    ctx.stroke();
    ctx.restore();
}

// ---- WavyProgressBar, indeterminate sweep (port of WavyProgressBar.cs) ----
const INDETERMINATE_CYCLE_SECONDS = 2;
const INDETERMINATE_SEGMENT_FRACTION = 0.4;
const WAVE_SPEED_PX_PER_SECOND = 40;
const WAVELENGTH = 40;
const TRACK_GAP = 4;

export function drawBar(ctx, scheme, elapsed, w, h) {
    const stroke = 4;
    const inset = stroke / 2;
    const y = h / 2;
    const left = inset;
    const right = w - inset;
    const width = right - left;
    if (width <= 0) return;

    const amp = 3;
    const phase = elapsed * WAVE_SPEED_PX_PER_SECOND * 2 * Math.PI / WAVELENGTH;

    // A segment sweeping left-to-right and wrapping around.
    const segLen = width * INDETERMINATE_SEGMENT_FRACTION;
    const cycle = elapsed / INDETERMINATE_CYCLE_SECONDS;
    const travel = (cycle - Math.floor(cycle)) * (width + segLen);
    const segStart = Math.max(left, left + travel - segLen);
    const segEnd = Math.min(right, left + travel);
    const hasSegment = segEnd - segStart > 1;

    ctx.save();
    ctx.lineWidth = stroke;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';

    // Track on both sides of the segment, separated by TRACK_GAP (never drawn behind the wave).
    ctx.strokeStyle = scheme.track;
    ctx.beginPath();
    if (hasSegment) {
        const leftTrackEnd = segStart - inset - TRACK_GAP;
        if (leftTrackEnd > left) {
            ctx.moveTo(left, y);
            ctx.lineTo(leftTrackEnd, y);
        }
        const rightTrackStart = segEnd + inset + TRACK_GAP;
        if (right > rightTrackStart) {
            ctx.moveTo(rightTrackStart, y);
            ctx.lineTo(right, y);
        }
    } else {
        ctx.moveTo(left, y);
        ctx.lineTo(right, y);
    }
    ctx.stroke();

    if (hasSegment) {
        ctx.strokeStyle = scheme.primary;
        ctx.beginPath();
        for (let x = segStart; x <= segEnd; x += 2.5) {
            const py = y + amp * Math.sin(x / WAVELENGTH * 2 * Math.PI + phase);
            if (x === segStart) ctx.moveTo(x, py); else ctx.lineTo(x, py);
        }
        ctx.lineTo(segEnd, y + amp * Math.sin(segEnd / WAVELENGTH * 2 * Math.PI + phase));
        ctx.stroke();
    }
    ctx.restore();
}

export function draw(ctx, scheme, variantName, elapsedMs, w, h) {
    switch (variantName) {
        case 'loading-indicator':
            drawLoadingIndicator(ctx, scheme, elapsedMs, w, h);
            break;
        case 'circular-wavy':
            drawCircular(ctx, scheme, elapsedMs / 1000, w, h);
            break;
        case 'wavy-bar':
            drawBar(ctx, scheme, elapsedMs / 1000, w, h);
            break;
    }
}
