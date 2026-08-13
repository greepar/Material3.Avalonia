import { dotnet } from './_framework/dotnet.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

let supportsBrotli = false;
try {
    new DecompressionStream('brotli');
    supportsBrotli = true;
} catch {
    // The runtime falls back to the uncompressed asset on older browsers.
}

const splash = globalThis.m3Splash;

function loadCompressedResource(type, name, defaultUri, integrity, behavior) {
    if (!supportsBrotli || type.includes('js') || type === 'configuration' || type === 'manifest') {
        return undefined;
    }

    return fetchCompressedResource(defaultUri, behavior);
}

async function fetchCompressedResource(defaultUri, behavior) {
    const compressed = await fetch(`${defaultUri}.br`, { cache: 'no-cache' });
    if (!compressed.ok || !compressed.body) {
        return fetch(defaultUri, { cache: 'no-cache' });
    }

    const contentType = behavior === 'dotnetwasm'
        ? 'application/wasm'
        : 'application/octet-stream';
    const body = compressed.body.pipeThrough(new DecompressionStream('brotli'));
    return new Response(body, { headers: { 'Content-Type': contentType } });
}

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .withResourceLoader(loadCompressedResource)
    .create();

const config = dotnetRuntime.getConfig();

// Avalonia renders into a canvas it appends to #out; hide the splash on the first painted frame.
const out = document.getElementById('out');
const observer = new MutationObserver(() => {
    if (out.querySelector('canvas')) {
        observer.disconnect();
        requestAnimationFrame(() => requestAnimationFrame(() => splash?.hide()));
    }
});
observer.observe(out, { childList: true, subtree: true });

await dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);

observer.disconnect();
splash?.hide();
