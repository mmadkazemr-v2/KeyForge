let vexFlowLoadPromise;

function loadVexFlow() {
    if (globalThis.VexFlow) {
        return Promise.resolve(globalThis.VexFlow);
    }

    vexFlowLoadPromise ??= new Promise((resolve, reject) => {
        const source = new URL('lib/vexflow/vexflow-bravura.js', document.baseURI).href;
        const existingScript = document.querySelector('script[data-keyforge-vexflow]');

        if (existingScript) {
            if (existingScript.dataset.loaded === 'true') {
                globalThis.VexFlow
                    ? resolve(globalThis.VexFlow)
                    : reject(new Error('The VexFlow script loaded without exposing its API.'));
                return;
            }

            existingScript.addEventListener('load', () => resolve(globalThis.VexFlow), { once: true });
            existingScript.addEventListener('error', reject, { once: true });
            return;
        }

        const script = document.createElement('script');
        script.src = source;
        script.async = true;
        script.dataset.keyforgeVexflow = 'true';
        script.addEventListener('load', () => {
            script.dataset.loaded = 'true';
            resolve(globalThis.VexFlow);
        }, { once: true });
        script.addEventListener('error', reject, { once: true });
        document.head.appendChild(script);
    });

    return vexFlowLoadPromise;
}

export async function renderStaff(element, request) {
    if (!element || !request?.clef || !request?.noteKey) {
        throw new Error('A target element, clef, and note key are required.');
    }

    const VexFlow = await loadVexFlow();
    if (!VexFlow) {
        throw new Error('VexFlow could not be loaded.');
    }

    clearStaff(element);
    element.dir = 'ltr';

    const width = Math.max(320, Math.round(element.getBoundingClientRect().width || 480));
    const height = 176;
    const renderer = new VexFlow.Renderer(element, VexFlow.Renderer.Backends.SVG);
    renderer.resize(width, height);

    const context = renderer.getContext();
    const ink = getComputedStyle(element).getPropertyValue('--kf-color-text-primary').trim() || '#f4f1f8';
    context.setFillStyle(ink);
    context.setStrokeStyle(ink);

    const stave = new VexFlow.Stave(12, 28, width - 24);
    stave.addClef(request.clef).setContext(context).draw();

    const note = new VexFlow.StaveNote({
        clef: request.clef,
        keys: [request.noteKey],
        duration: 'q',
    });

    VexFlow.Formatter.FormatAndDraw(context, stave, [note]);
}

export function clearStaff(element) {
    element?.replaceChildren();
}
