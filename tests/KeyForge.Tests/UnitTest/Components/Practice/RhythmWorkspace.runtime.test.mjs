import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';

const modulePath = new URL('../../../../../src/KeyForge/Components/Practice/RhythmWorkspace.razor.js', import.meta.url);
const flush = async () => { for (let index = 0; index < 12; index++) await Promise.resolve(); };

async function withRuntime(eventCount, callback) {
    const saved = new Map(['document', 'performance', 'setTimeout', 'clearTimeout']
        .map(name => [name, Object.getOwnPropertyDescriptor(globalThis, name)]));
    let now = 12_000; // Count-in has already finished before the rhythm runtime starts.
    let nextId = 0;
    const timers = new Map();
    const activeEvents = [];
    const calls = [];
    const listeners = new Map();
    const classes = () => ({ add() {}, remove() {}, toggle() {} });
    const progress = { setAttribute() {}, querySelector: () => ({ style: { setProperty() {} } }) };
    const text = new Map();
    const markers = Array.from({ length: eventCount }, (_, index) => ({
        dataset: { rhythmBeat: String(index) },
        classList: {
            ...classes(),
            toggle(name, active) { if (name === 'is-active' && active) activeEvents.push([index, now]); }
        }
    }));
    const root = {
        dataset: {}, classList: classes(), focus() {},
        querySelector(selector) {
            if (selector === '[data-rhythm-progress]') return progress;
            if (!text.has(selector)) text.set(selector, { textContent: '' });
            return text.get(selector);
        },
        querySelectorAll: () => markers
    };
    const replacements = {
        document: {
            getElementById: () => root,
            addEventListener: (name, handler) => listeners.set(name, handler),
            removeEventListener: (name, handler) => { if (listeners.get(name) === handler) listeners.delete(name); }
        },
        performance: { now: () => now },
        setTimeout: (handler, delay) => {
            const id = ++nextId;
            timers.set(id, { at: now + Math.max(0, delay), handler });
            return id;
        },
        clearTimeout: id => timers.delete(id)
    };
    for (const [name, value] of Object.entries(replacements))
        Object.defineProperty(globalThis, name, { configurable: true, writable: true, value });

    try {
        const source = await readFile(modulePath, 'utf8');
        const runtime = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}#${Math.random()}`);
        const dotNet = { async invokeMethodAsync(name, ...args) { calls.push({ name, args, at: now }); } };
        const labels = {
            active: 'Active', evaluating: 'Evaluating', digits: '0123456789', audio: true,
            playClick: () => calls.push({ name: 'AudioClick', args: [], at: now })
        };
        const advanceTo = async target => {
            await flush();
            while (true) {
                const next = [...timers.entries()].filter(([, timer]) => timer.at <= target)
                    .sort((left, right) => left[1].at - right[1].at || left[0] - right[0])[0];
                if (!next) break;
                timers.delete(next[0]);
                now = next[1].at;
                next[1].handler();
                await flush();
            }
            now = target;
            await flush();
        };
        await callback({ runtime, dotNet, calls, root, listeners, activeEvents, advanceTo, labels, pendingTimerCount: () => timers.size });
    } finally {
        for (const [name, descriptor] of saved)
            if (descriptor) Object.defineProperty(globalThis, name, descriptor);
            else delete globalThis[name];
    }
}

test('non-uniform event display follows compiled offsets while metronome keeps its own pulse grid', async () => {
    await withRuntime(5, async ({ runtime, dotNet, calls, activeEvents, advanceTo, listeners, labels }) => {
        const offsets = [0, 1000, 1500, 2000, 3000];
        const running = runtime.startRhythm('rhythm', dotNet,
            offsets.map(offsetMilliseconds => ({ offsetMilliseconds, isRest: false })),
            [0, 1000, 2000, 3000], 4000, labels);
        await advanceTo(16_000);
        await running;

        assert.deepEqual(activeEvents, offsets.map((offset, index) => [index, 12_000 + offset]));
        assert.deepEqual(calls.filter(call => call.name === 'AudioClick').map(call => call.at),
            [12_000, 13_000, 14_000, 15_000]);
        assert.equal(calls.find(call => call.name === 'CompleteRhythm').at, 16_000);
        assert.equal(listeners.has('keydown'), false);
    });
});

test('post-count-in browser origin is used for keyboard offsets and rest-window hits are forwarded for scoring', async () => {
    await withRuntime(4, async ({ runtime, dotNet, calls, advanceTo, listeners, labels }) => {
        const running = runtime.startRhythm('rhythm', dotNet,
            [0, 1000, 2000, 3000].map((offsetMilliseconds, index) => ({ offsetMilliseconds, isRest: index === 1 })),
            [0, 1000, 2000, 3000], 4000, labels);
        await advanceTo(13_500);
        listeners.get('keydown')({ code: 'KeyK', repeat: false, preventDefault() {} });
        await advanceTo(16_000);
        await running;

        assert.equal(calls.find(call => call.name === 'NotifyRhythmStarted').args[0], 12_000);
        assert.equal(calls.find(call => call.name === 'CaptureRhythmKeyboardInput').args[2], 1500);
        assert.equal(calls.filter(call => call.name === 'CaptureRhythmKeyboardInput').length, 1);
    });
});

test('disconnect pause cancels pending timeline work and does not complete an interrupted attempt', async () => {
    await withRuntime(2, async ({ runtime, dotNet, calls, advanceTo, listeners, root, labels }) => {
        const running = runtime.startRhythm('rhythm', dotNet,
            [{ offsetMilliseconds: 0 }, { offsetMilliseconds: 1000 }], [0, 1000], 2000, labels);
        await advanceTo(12_500);
        runtime.setRhythmPaused('rhythm', 'Ready');
        await advanceTo(14_000);
        await running;

        assert.equal(root.dataset.state, 'ready');
        assert.equal(listeners.has('keydown'), false);
        assert.equal(calls.some(call => call.name === 'CompleteRhythm'), false);
        assert.equal(calls.filter(call => call.name === 'AudioClick').length, 1);
    });
});

test('External metronome mode keeps visual rhythm timing but suppresses KeyForge click audio', async () => {
    await withRuntime(2, async ({ runtime, dotNet, calls, advanceTo, activeEvents, labels }) => {
        labels.audio = false;
        const running = runtime.startRhythm('rhythm', dotNet,
            [{ offsetMilliseconds: 0 }, { offsetMilliseconds: 1000 }], [0, 1000], 2000, labels);
        await advanceTo(14_000);
        await running;

        assert.equal(activeEvents.length, 2);
        assert.equal(calls.filter(call => call.name === 'AudioClick').length, 0);
        assert.equal(calls.some(call => call.name === 'CompleteRhythm'), true);
    });
});

test('stop cancels the pending rhythm deadline and prevents orphaned metronome clicks', async () => {
    await withRuntime(4, async ({ runtime, dotNet, calls, advanceTo, labels, pendingTimerCount }) => {
        const running = runtime.startRhythm('rhythm', dotNet,
            [0, 1000, 2000, 3000].map(offsetMilliseconds => ({ offsetMilliseconds })),
            [0, 1000, 2000, 3000], 4000, labels);
        await running;
        await flush();
        assert.equal(calls.filter(call => call.name === 'AudioClick').length, 1);
        assert.equal(pendingTimerCount(), 1);

        runtime.stopRhythm('rhythm');
        assert.equal(pendingTimerCount(), 0);
        await advanceTo(20_000);

        assert.equal(calls.filter(call => call.name === 'AudioClick').length, 1);
        assert.equal(calls.some(call => call.name === 'CompleteRhythm'), false);
    });
});

test('restarting a workspace cancels its previous scheduler and keeps one active input listener', async () => {
    await withRuntime(4, async ({ runtime, dotNet, calls, advanceTo, labels, listeners }) => {
        const timeline = [0, 1000, 2000, 3000].map(offsetMilliseconds => ({ offsetMilliseconds }));
        const pulses = [0, 1000, 2000, 3000];
        await runtime.startRhythm('rhythm', dotNet, timeline, pulses, 4000, labels);
        await flush();
        await runtime.startRhythm('rhythm', dotNet, timeline, pulses, 4000, labels);
        await flush();

        assert.equal(listeners.has('keydown'), true);
        await advanceTo(16_000);

        assert.deepEqual(calls.filter(call => call.name === 'AudioClick').map(call => call.at),
            [12_000, 12_000, 13_000, 14_000, 15_000]);
        assert.equal(calls.filter(call => call.name === 'CompleteRhythm').length, 1);
        assert.equal(listeners.has('keydown'), false);
    });
});

test('exercise startup resolves at the browser timeline origin instead of waiting for exercise completion', async () => {
    await withRuntime(2, async ({ runtime, dotNet, calls, labels, pendingTimerCount }) => {
        await runtime.startRhythm('rhythm', dotNet,
            [{ offsetMilliseconds: 0 }, { offsetMilliseconds: 1000 }], [0, 1000], 2000, labels);
        await flush();

        assert.equal(calls.some(call => call.name === 'NotifyRhythmStarted'), true);
        assert.equal(calls.some(call => call.name === 'CompleteRhythm'), false);
        assert.equal(pendingTimerCount(), 1);
        runtime.stopRhythm('rhythm');
        await flush();
        assert.equal(pendingTimerCount(), 0);
    });
});

test('delayed start interop cannot shift clicks and cancellation suppresses completion', async () => {
    await withRuntime(2, async ({ runtime, dotNet, calls, advanceTo, labels }) => {
        let release;
        const invoke = dotNet.invokeMethodAsync.bind(dotNet);
        dotNet.invokeMethodAsync = (name, ...args) => {
            if (name === 'NotifyRhythmStarted') {
                invoke(name, ...args);
                return new Promise(resolve => { release = resolve; });
            }
            return invoke(name, ...args);
        };
        await runtime.startRhythm('rhythm', dotNet,
            [{ offsetMilliseconds: 0 }, { offsetMilliseconds: 1000 }], [0, 1000], 2000, labels);
        await advanceTo(13500);
        assert.deepEqual(calls.filter(c => c.name === 'AudioClick').map(c => c.at), [12000, 13000]);
        runtime.stopRhythm('rhythm');
        release();
        await advanceTo(16000);
        assert.equal(calls.some(c => c.name === 'CompleteRhythm'), false);
    });
});

test('count-in and exercise pulses share the same browser grid despite delayed interop', async () => {
    await withRuntime(2, async ({ runtime, dotNet, calls, advanceTo, labels }) => {
        labels.countInBeats = 2;
        labels.beatIntervalMilliseconds = 1000;
        let release;
        const invoke = dotNet.invokeMethodAsync.bind(dotNet);
        dotNet.invokeMethodAsync = (name, ...args) => {
            invoke(name, ...args);
            return name === 'NotifyRhythmStarted' ? new Promise(resolve => { release = resolve; }) : Promise.resolve();
        };
        const running = runtime.startRhythm('rhythm', dotNet,
            [{ offsetMilliseconds: 0 }, { offsetMilliseconds: 1000 }], [0, 1000], 2000, labels);
        await advanceTo(15500);
        assert.deepEqual(calls.filter(c => c.name === 'AudioClick').map(c => c.at), [12000, 13000, 14000, 15000]);
        assert.equal(calls.find(c => c.name === 'NotifyRhythmStarted').args[0], 14000);
        release();
        await running;
        await advanceTo(16000);
        assert.equal(calls.filter(c => c.name === 'CompleteRhythm').length, 1);
    });
});

test('external metronome preserves local timeline and count-in without scheduling audio, including restart', async () => {
    await withRuntime(2, async ({ runtime, dotNet, calls, advanceTo, labels, pendingTimerCount }) => {
        const silent = { ...labels, countInBeats: 2, beatIntervalMilliseconds: 1000, audio: false, playClick() { throw new Error('Silent mode scheduled audio'); } };
        const events = [{ offsetMilliseconds: 0 }, { offsetMilliseconds: 1000 }];
        const running = runtime.startRhythm('rhythm', dotNet, events, [0, 1000], 2000, silent);
        await advanceTo(16000);
        await running;
        assert.equal(calls.find(c => c.name === 'NotifyRhythmStarted').args[0], 14000);
        assert.equal(calls.filter(c => c.name === 'CompleteRhythm').length, 1);
        assert.equal(calls.some(c => c.name === 'AudioClick'), false);
        const restarted = runtime.startRhythm('rhythm', dotNet, events, [0, 1000], 2000, silent);
        runtime.stopRhythm('rhythm');
         await flush();
        assert.equal(pendingTimerCount(), 0);
    });
});
