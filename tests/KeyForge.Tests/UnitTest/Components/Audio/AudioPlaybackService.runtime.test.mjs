import { test, afterEach } from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../../../../src/KeyForge/wwwroot/Components/Audio/AudioPlaybackService.js', import.meta.url), 'utf8');
let moduleId = 0;
const load = () => import(`data:text/javascript;base64,${Buffer.from(source + `\n// test ${moduleId++}`).toString('base64')}`);

function tone(state = 'suspended') {
    const context = { state };
    const nodes = [];
    let starts = 0;
    class Synth {
        constructor(...options) { this.options = options; this.notes = []; nodes.push(this); }
        toDestination() { this.connected = true; return this; }
        triggerAttackRelease(...note) { this.notes.push(note); }
        releaseAll() { this.released = true; }
        triggerRelease() { this.released = true; }
        dispose() { this.disposed = true; }
    }
    globalThis.Tone = {
        getContext: () => context,
        start: async () => { starts++; context.state = 'running'; },
        PolySynth: Synth, Synth, MembraneSynth: Synth
    };
    return { context, nodes, starts: () => starts };
}

afterEach(() => { delete globalThis.Tone; delete globalThis.document; });

test('uses the Tone 15 context property and shares note/click nodes across repeated initialization', async () => {
    const runtime = tone();
    const audio = await load();
    assert.equal(await audio.initialize(), 'running');
    for (const note of ['C4', 'E4', 'G4', 'C5']) audio.playNote(note);
    audio.playMetronomeClick();
    assert.equal(await audio.initialize(), 'running');
    assert.equal(runtime.starts(), 1);
    assert.equal(runtime.nodes.length, 2);
    assert.ok(runtime.nodes.every(node => node.connected));
    assert.deepEqual(runtime.nodes[0].notes.map(note => note[0]), ['C4', 'E4', 'G4', 'C5']);
    assert.equal(runtime.nodes[1].notes[0][0], 'C6');
});

test('preparation does not unlock audio; the native click reaches start synchronously', async () => {
    const runtime = tone();
    const audio = await load();
    const button = new EventTarget();
    await audio.prepareActivation(button);
    await audio.prepareActivation(button);
    assert.equal(runtime.starts(), 0);
    assert.equal(runtime.nodes.length, 0);
    button.dispatchEvent(new Event('click'));
    assert.equal(runtime.starts(), 1);
    assert.equal(await audio.initialize(), 'running');
});

test('concurrent initialization shares a pending resume and creates one pair of synths', async () => {
    const runtime = tone();
    let resume;
    Tone.start = () => new Promise(resolve => { resume = () => { runtime.context.state = 'running'; resolve(); }; });
    const audio = await load();
    const first = audio.initialize();
    const second = audio.initialize();
    assert.equal(first, second);
    resume();
    await Promise.all([first, second]);
    assert.equal(runtime.nodes.length, 2);
});

test('a JavaScript error remains an error and a later gesture can retry', async () => {
    const runtime = tone();
    Tone.start = async () => { throw new TypeError('native failure'); };
    const audio = await load();
    await assert.rejects(audio.initialize(), /native failure/);
    assert.equal(runtime.nodes.length, 0);
    Tone.start = async () => { runtime.context.state = 'running'; };
    assert.equal(await audio.initialize(), 'running');
});

test('permission rejection and closed context are distinct from JavaScript failures', async () => {
    const runtime = tone();
    Tone.start = async () => { throw new DOMException('gesture required', 'NotAllowedError'); };
    const audio = await load();
    assert.equal(await audio.initialize(), 'blocked');
    runtime.context.state = 'closed';
    assert.equal(await audio.initialize(), 'closed');
    assert.throws(() => audio.playMetronomeClick(), /closed/);
});

test('failed script loading is retryable', async () => {
    const scripts = [];
    globalThis.document = {
        baseURI: 'http://localhost/',
        querySelector: () => scripts.find(script => !script.removed),
        createElement: () => Object.assign(new EventTarget(), { dataset: {}, remove() { this.removed = true; } }),
        head: { appendChild(script) { scripts.push(script); } }
    };
    const audio = await load();
    const failed = audio.prepareActivation(new EventTarget());
    scripts[0].dispatchEvent(new Event('error'));
    await assert.rejects(failed, /could not be loaded/);
    const retried = audio.prepareActivation(new EventTarget());
    tone();
    scripts[1].dispatchEvent(new Event('load'));
    await retried;
    assert.equal(await audio.initialize(), 'running');
    assert.equal(scripts.length, 2);
});

test('stop cancels queued notes and releases both paths; disposal is idempotent', async () => {
    const runtime = tone('running');
    const audio = await load();
    await audio.initialize();
    audio.playSequence([{ note: 'G4', delayMilliseconds: 30 }]);
    audio.stopActiveNotes();
    audio.stopActiveNotes();
    await new Promise(resolve => setTimeout(resolve, 50));
    assert.equal(runtime.nodes[0].notes.length, 0);
    assert.ok(runtime.nodes.every(node => node.released));
    audio.dispose();
    audio.dispose();
    assert.ok(runtime.nodes.every(node => node.disposed));
    assert.throws(() => audio.playNote('C4'), /not initialized/);
    await audio.initialize();
    assert.equal(runtime.nodes.length, 4);
});

test('disposal during initialization does not resurrect synths', async () => {
    const runtime = tone();
    let resume;
    Tone.start = () => new Promise(resolve => { resume = () => { runtime.context.state = 'running'; resolve(); }; });
    const audio = await load();
    const pending = audio.initialize();
    audio.dispose();
    resume();
    assert.equal(await pending, 'unavailable');
    assert.equal(runtime.nodes.length, 0);
});
