import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';

const adapterPath = new URL('../../../../../src/KeyForge/Components/Practice/MidiInputControl.razor.js', import.meta.url);

async function importAdapter() {
    const source = await readFile(adapterPath, 'utf8');
    return import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}#${Math.random()}`);
}

class FakeMidiPort {
    constructor(id, name, manufacturer = 'Alesis') {
        this.id = id;
        this.name = name;
        this.manufacturer = manufacturer;
        this.state = 'connected';
        this.connection = 'closed';
        this.onmidimessage = null;
        this.openCount = 0;
        this.closeCount = 0;
        this.openError = null;
    }
    async open() {
        this.openCount++;
        if (this.openError) throw this.openError;
        this.connection = 'open';
        return this;
    }
    async close() {
        this.closeCount++;
        this.connection = 'closed';
        return this;
    }
    emit(data, timeStamp) {
        this.onmidimessage?.({ data, timeStamp });
    }
}

function createAccess(ports) {
    return {
        inputs: new Map(ports.map(port => [port.id, port])),
        onstatechange: null,
        change: async () => await this.onstatechange?.()
    };
}

async function withNavigator(requestMIDIAccess, callback) {
    const original = Object.getOwnPropertyDescriptor(globalThis, 'navigator');
    Object.defineProperty(globalThis, 'navigator', { configurable: true, value: { requestMIDIAccess } });
    try { return await callback(); }
    finally {
        if (original) Object.defineProperty(globalThis, 'navigator', original);
        else delete globalThis.navigator;
    }
}

test('MIDI selection is explicit; event metadata and timestamp survive, then exact device reconnects', async () => {
    const editor = new FakeMidiPort('editor', 'V61 Editor');
    const keyboard = new FakeMidiPort('keyboard', 'V61 MKII');
    const access = createAccess([editor, keyboard]);
    const calls = [];
    let requestedArguments;

    await withNavigator(async (...args) => { requestedArguments = args; return access; }, async () => {
        const midi = await importAdapter();
        const enabled = await midi.enable({ invokeMethodAsync: async (...args) => calls.push(args) });
        assert.deepEqual(requestedArguments, []);
        assert.equal(enabled.status, 8);
        assert.equal(enabled.selectedDeviceId, null);
        assert.equal(editor.onmidimessage, null);
        assert.equal(keyboard.onmidimessage, null);

        const selected = await midi.selectInput('keyboard');
        assert.equal(selected.status, 4);
        assert.equal(selected.selectedDeviceId, 'keyboard');
        assert.equal(keyboard.openCount, 1);
        assert.equal(editor.onmidimessage, null);

        keyboard.emit([0x9f, 72, 111], 12_530);
        await new Promise(resolve => setTimeout(resolve, 0));
        assert.equal(calls[0][0], 'HandleMidiMessage');
        assert.deepEqual(calls[0][1], {
            deviceId: 'keyboard', deviceName: 'Alesis V61 MKII', status: 0x9f, data1: 72, data2: 111,
            timestampMilliseconds: 12_530, sequence: 1
        });

        keyboard.state = 'disconnected';
        await access.onstatechange();
        assert.equal(keyboard.onmidimessage, null);
        assert.equal(calls.at(-1)[1].status, 6);
        assert.equal(calls.at(-1)[1].selectedDeviceId, 'keyboard');

        const unrelated = new FakeMidiPort('unrelated', 'Other Keyboard');
        access.inputs.set(unrelated.id, unrelated);
        await access.onstatechange();
        assert.equal(calls.at(-1)[1].selectedDeviceId, 'keyboard');
        assert.equal(unrelated.onmidimessage, null);

        keyboard.state = 'connected';
        await access.onstatechange();
        assert.equal(keyboard.onmidimessage instanceof Function, true);
        assert.equal(calls.at(-1)[1].status, 4);
        assert.equal(calls.at(-1)[1].selectedDeviceId, 'keyboard');
        assert.equal(midi.getBrowserTimestamp() >= 0, true);
        await midi.dispose();
    });
});

test('permission denial, no devices, and selected port open failure remain distinct', async () => {
    await withNavigator(async () => { throw Object.assign(new Error('blocked by user'), { name: 'NotAllowedError' }); }, async () => {
        const midi = await importAdapter();
        const state = await midi.enable({ invokeMethodAsync: async () => {} });
        assert.equal(state.status, 2);
        assert.match(state.errorMessage, /blocked by user/);
    });

    const access = createAccess([]);
    await withNavigator(async () => access, async () => {
        const midi = await importAdapter();
        const state = await midi.enable({ invokeMethodAsync: async () => {} });
        assert.equal(state.status, 3);
        await midi.dispose();
    });

    const port = new FakeMidiPort('failure', 'Unavailable Input');
    port.openError = new Error('port open rejected');
    const failingAccess = createAccess([port]);
    await withNavigator(async () => failingAccess, async () => {
        const midi = await importAdapter();
        await midi.enable({ invokeMethodAsync: async () => {} });
        const state = await midi.selectInput('failure');
        assert.equal(state.status, 7);
        assert.match(state.errorMessage, /port open rejected/);
        await midi.dispose();
    });
});
