let toneLoadPromise;
let initialization;
let synth;
let clickSynth;
let pendingSequenceTimers = [];
let generation = 0;
const activationButtons = new WeakSet();

function cancelPendingSequence() {
    for (const timer of pendingSequenceTimers) {
        clearTimeout(timer);
    }
    pendingSequenceTimers = [];
    synth?.releaseAll();
}

function loadTone() {
    if (globalThis.Tone) {
        return Promise.resolve(globalThis.Tone);
    }

    toneLoadPromise ??= new Promise((resolve, reject) => {
        const source = new URL('lib/tone/tone.js', document.baseURI).href;
        const existingScript = document.querySelector('script[data-keyforge-tone]');

        if (existingScript) {
            if (existingScript.dataset.loaded === 'true') {
                existingScript.remove();
                reject(new Error('Tone.js loaded without exposing its API.'));
                return;
            }
            existingScript.addEventListener('load', () => resolve(globalThis.Tone), { once: true });
            existingScript.addEventListener('error', () => {
                existingScript.remove();
                reject(new Error('Tone.js could not be loaded.'));
            }, { once: true });
            return;
        }

        const script = document.createElement('script');
        script.src = source;
        script.async = true;
        script.dataset.keyforgeTone = 'true';
        script.addEventListener('load', () => {
            if (!globalThis.Tone) {
                script.remove();
                reject(new Error('Tone.js loaded without exposing its API.'));
                return;
            }
            script.dataset.loaded = 'true';
            resolve(globalThis.Tone);
        }, { once: true });
        script.addEventListener('error', () => {
            script.remove();
            reject(new Error(`Tone.js could not be loaded from ${source}.`));
        }, { once: true });
        document.head.appendChild(script);
    }).catch(error => {
        toneLoadPromise = undefined;
        throw error;
    });

    return toneLoadPromise;
}

export async function prepareActivation(button) {
    if (button && !activationButtons.has(button)) {
        activationButtons.add(button);
        // This runs locally before Blazor's delegated click travels to the server.
        button.addEventListener('click', () => {
            void initialize().catch(() => { /* The service reports the initialization error. */ });
        }, { capture: true });
    }
    // Preload the library only. Tone.start and synth creation remain gesture driven.
    await loadTone();
}

export function initialize() {
    if (initialization) return initialization;
    const currentGeneration = generation;
    // With preloaded Tone, startTone reaches Tone.start synchronously in the click.
    const attempt = globalThis.Tone
        ? startTone(globalThis.Tone, currentGeneration)
        : loadTone().then(Tone => startTone(Tone, currentGeneration));
    initialization = attempt;
    void attempt.finally(() => {
        if (initialization === attempt) initialization = undefined;
    }).catch(() => {});
    return attempt;
}

async function startTone(Tone, currentGeneration) {
    if (!Tone) {
        throw new Error('Tone.js is not available.');
    }

    const context = Tone.getContext();
    if (context.state === 'closed') return 'closed';
    if (context.state !== 'running') {
        let timeout;
        try {
            // A blocked resume can remain pending indefinitely in Chromium.
            await Promise.race([
                Tone.start(),
                new Promise(resolve => { timeout = setTimeout(resolve, 2000); })
            ]);
        } catch (error) {
            if (error?.name === 'NotAllowedError') return 'blocked';
            throw error;
        } finally {
            clearTimeout(timeout);
        }
    }
    if (currentGeneration !== generation) return 'unavailable';
    if (context.state !== 'running') return context.state;
    synth ??= new Tone.PolySynth(Tone.Synth, { volume: -12 }).toDestination();
    clickSynth ??= new Tone.MembraneSynth({
        volume: -18,
        pitchDecay: 0.01,
        octaves: 1,
        envelope: { attack: 0.001, decay: 0.04, sustain: 0, release: 0.03 }
    }).toDestination();
    return context.state;
}

export function getState() {
    return globalThis.Tone?.getContext().state ?? 'uninitialized';
}

function requireRunning() {
    if (getState() !== 'running') {
        throw new Error(`AudioContext is ${getState()}; enable audio before playback.`);
    }
}

export function playNote(note) {
    requireRunning();
    if (!synth || !note) {
        throw new Error('Audio is not initialized.');
    }

    synth.triggerAttackRelease(note, '8n');
}

export function playSequence(notes) {
    requireRunning();
    if (!synth) {
        throw new Error('Audio is not initialized.');
    }

    cancelPendingSequence();
    for (const item of notes ?? []) {
        if (!item?.note) {
            continue;
        }

        const delay = Math.max(0, Number(item.delayMilliseconds) || 0);
        pendingSequenceTimers.push(setTimeout(() => {
            synth?.triggerAttackRelease(item.note, '8n');
        }, delay));
    }
}

export function stopActiveNotes() {
    cancelPendingSequence();
    clickSynth?.triggerRelease();
}

export function playMetronomeClick() {
    requireRunning();
    if (!clickSynth) {
        throw new Error('Audio is not initialized.');
    }

    clickSynth.triggerAttackRelease('C6', '32n');
}

export function dispose() {
    generation++;
    initialization = undefined;
    cancelPendingSequence();
    synth?.dispose();
    clickSynth?.dispose();
    synth = undefined;
    clickSynth = undefined;
}
