let toneLoadPromise;
let synth;
let clickSynth;

function loadTone() {
    if (globalThis.Tone) {
        return Promise.resolve(globalThis.Tone);
    }

    toneLoadPromise ??= new Promise((resolve, reject) => {
        const source = new URL('lib/tone/tone.js', document.baseURI).href;
        const existingScript = document.querySelector('script[data-keyforge-tone]');

        if (existingScript) {
            existingScript.addEventListener('load', () => resolve(globalThis.Tone), { once: true });
            existingScript.addEventListener('error', reject, { once: true });
            return;
        }

        const script = document.createElement('script');
        script.src = source;
        script.async = true;
        script.dataset.keyforgeTone = 'true';
        script.addEventListener('load', () => resolve(globalThis.Tone), { once: true });
        script.addEventListener('error', reject, { once: true });
        document.head.appendChild(script);
    });

    return toneLoadPromise;
}

export async function initialize() {
    const Tone = await loadTone();
    if (!Tone) {
        throw new Error('Tone.js could not be loaded.');
    }

    await Tone.start();
    synth ??= new Tone.PolySynth(Tone.Synth).toDestination();
    clickSynth ??= new Tone.MembraneSynth({ pitchDecay: 0.01, octaves: 1 }).toDestination();
    return true;
}

export function playNote(note) {
    if (!synth || !note) {
        throw new Error('Audio is not initialized.');
    }

    synth.triggerAttackRelease(note, '8n');
}

export function stopActiveNotes() {
    synth?.releaseAll();
}

export function playMetronomeClick() {
    if (!clickSynth) {
        throw new Error('Audio is not initialized.');
    }

    clickSynth.triggerAttackRelease('C6', '32n');
}

export function dispose() {
    synth?.dispose();
    clickSynth?.dispose();
    synth = undefined;
    clickSynth = undefined;
}
