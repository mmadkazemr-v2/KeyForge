import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';

const practice = await readFile(new URL('../../../../../src/KeyForge/Components/Pages/Practice.razor', import.meta.url), 'utf8');
test('Practice defaults to external sound with no implicit activation and preserves modes during navigation', () => {
    assert.match(practice, /private bool _internalMidiAudioEnabled;/);
    assert.match(practice, /_metronomeAudioMode = PracticeMetronomeMode.External;/);
    assert.equal((practice.match(/_metronomeAudioMode =/g) ?? []).length, 2);
    assert.match(practice, /@if \(_internalMidiAudioEnabled \|\| IsKeyForgeMetronomeEnabled\)[\s\S]*?<AudioActivationControl/);
    assert.doesNotMatch(practice, /InitializeAsync|PrepareActivationAsync/);
    assert.match(practice, /LivePlayEnabled="_internalMidiAudioEnabled"/);
    assert.match(practice, /SilentListeningLimitation/);
    assert.equal((practice.match(/MetronomeAudioEnabled="IsKeyForgeMetronomeEnabled"/g) ?? []).length, 2);
});

test('Keyboard Lab requires explicit audio opt-in while retaining MIDI diagnostics', async () => {
    const lab = await readFile(new URL('../../../../../src/KeyForge/Components/Pages/KeyboardLab.razor', import.meta.url), 'utf8');
    assert.match(lab, /private bool _internalAudioEnabled;/);
    assert.match(lab, /LivePlayEnabled="_internalAudioEnabled"/);
    assert.match(lab, /AudioEnabled="_internalAudioEnabled"/);
    assert.match(lab, /@if \(_internalAudioEnabled\)[\s\S]*?<AudioActivationControl/);
    assert.match(lab, /if \(_internalAudioEnabled\) await Audio.PlayNoteAsync/);
});
