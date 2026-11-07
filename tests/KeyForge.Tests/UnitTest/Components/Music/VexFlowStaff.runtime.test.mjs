import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

test('single-note sharp adds an explicit accidental while natural notes retain their rendering path', async () => {
    const saved = globalThis.VexFlow;
    const savedStyle = globalThis.getComputedStyle;
    const notes = [];
    try {
        const context = { setFillStyle() {}, setStrokeStyle() {} };
        class Renderer { static Backends = { SVG: 1 }; resize() {} getContext() { return context; } }
        class Stave { addClef() { return this; } setContext() { return this; } draw() {} }
        class StaveNote {
            constructor(options) { this.options = options; this.modifiers = []; notes.push(this); }
            addModifier(modifier, index) { this.modifiers.push([modifier, index]); }
        }
        class Accidental { constructor(value) { this.value = value; } }
        globalThis.VexFlow = { Renderer, Stave, StaveNote, Accidental, Formatter: { FormatAndDraw() {} } };
        globalThis.getComputedStyle = () => ({ getPropertyValue: () => '#fff' });
        const source = await readFile(new URL('../../../../../src/KeyForge/Components/Music/VexFlowStaff.razor.js', import.meta.url), 'utf8');
        const module = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
        const element = { replaceChildren() {}, getBoundingClientRect: () => ({ width: 500 }) };
        await module.renderStaff(element, { clef: 'treble', noteKey: 'f#/4', accidental: '#' });
        await module.renderStaff(element, { clef: 'bass', noteKey: 'c/3' });
        assert.equal(notes[0].options.keys[0], 'f#/4');
        assert.equal(notes[0].modifiers[0][0].value, '#');
        assert.equal(notes[0].modifiers[0][1], 0);
        assert.equal(notes[1].modifiers.length, 0);
    } finally {
        if (saved === undefined) delete globalThis.VexFlow; else globalThis.VexFlow = saved;
        if (savedStyle === undefined) delete globalThis.getComputedStyle; else globalThis.getComputedStyle = savedStyle;
    }
});
