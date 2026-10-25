async page => {
    const errors = [];
    const waitForPage = async (predicate, message, timeoutMilliseconds = 8_000) => {
        const deadline = Date.now() + timeoutMilliseconds;
        while (Date.now() < deadline) {
            if (await page.evaluate(predicate)) return;
            await page.waitForTimeout(100);
        }
        throw new Error(message);
    };
    page.on('pageerror', error => errors.push(error.message));
    await page.addInitScript(() => {
        const createPort = (id, name, manufacturer = 'Alesis') => ({
            id, name, manufacturer, state: 'connected', connection: 'closed', onmidimessage: null,
            async open() { this.connection = 'open'; return this; },
            async close() { this.connection = 'closed'; return this; }
        });
        const editor = createPort('alesis-editor', 'V61 MKII Editor');
        const keyboard = createPort('alesis-keyboard', 'V61 MKII');
        const access = {
            inputs: new Map([[editor.id, editor], [keyboard.id, keyboard]]),
            onstatechange: null
        };
        let requestArguments = null;
        Object.defineProperty(navigator, 'requestMIDIAccess', {
            configurable: true,
            value: async (...args) => { requestArguments = args; return access; }
        });
        window.__midiMock = {
            access, editor, keyboard,
            get requestArguments() { return requestArguments; },
            emit(id, data, timestamp = performance.now()) {
                const port = access.inputs.get(id);
                port?.onmidimessage?.({ data, timeStamp: timestamp });
            },
            async disconnect(id) {
                const port = access.inputs.get(id);
                port.state = 'disconnected';
                await access.onstatechange?.({ port });
            },
            async reconnect(id) {
                const port = access.inputs.get(id);
                port.state = 'connected';
                await access.onstatechange?.({ port });
            },
            async addUnrelated() {
                const other = createPort('unrelated', 'Other MIDI Keyboard', 'Other');
                access.inputs.set(other.id, other);
                await access.onstatechange?.({ port: other });
                return other;
            }
        };
    });

    await page.goto('http://127.0.0.1:5264/language/en?returnUrl=%2Fpractice%2Flesson-01');
    await page.bringToFront();
    await waitForPage(() => Boolean(document.querySelector('.practice-midi-panel summary')), 'Practice MIDI panel did not render.');
    await page.waitForTimeout(500);
    await page.evaluate(() => document.querySelector('.practice-midi-panel summary').click());
    await waitForPage(() => Boolean(document.querySelector('.practice-midi-panel[open]')), 'Practice MIDI panel did not open.');
    await page.evaluate(() => [...document.querySelectorAll('button')].find(button => button.textContent.includes('Enable MIDI'))?.click());
    await waitForPage(() => Boolean(document.querySelector('select[aria-label="MIDI device"] option[value="alesis-keyboard"]')), 'MIDI inputs were not enumerated.');
    const initialValue = await page.locator('select[aria-label="MIDI device"]').inputValue();
    const optionNames = await page.locator('select[aria-label="MIDI device"] option').allTextContents();
    const requestArguments = await page.evaluate(() => window.__midiMock.requestArguments);
    if (initialValue !== '' || optionNames.length < 3 || requestArguments.length !== 0) {
        throw new Error('MIDI inputs were auto-selected, not enumerated, or requested with extra privileges.');
    }

    await page.evaluate(() => {
        const selector = document.querySelector('select[aria-label="MIDI device"]');
        selector.value = 'alesis-keyboard';
        selector.dispatchEvent(new Event('change', { bubbles: true }));
    });
    await waitForPage(() => [...document.querySelectorAll('.midi-input-control__diagnostics dd')].some(dd => dd.textContent.trim() === 'MIDI device connected'), 'Selected MIDI input did not connect.');
    await page.evaluate(() => window.__midiMock.emit('alesis-keyboard', [0x9f, 60, 99]));
    await waitForPage(() => [...document.querySelectorAll('.midi-input-control__diagnostics dd')].some(dd => dd.textContent.trim() === 'C4'), 'NoteOn did not reach the MIDI UI.');
    await page.evaluate(() => window.__midiMock.emit('alesis-keyboard', [0xbf, 64, 127]));
    await waitForPage(() => [...document.querySelectorAll('.midi-input-control__diagnostics dd')].some(dd => dd.textContent.trim() === 'Pedal down'), 'Sustain control change did not reach the MIDI UI.');
    await page.evaluate(() => window.__midiMock.emit('alesis-keyboard', [0x8f, 60, 32]));

    await page.evaluate(() => window.__midiMock.disconnect('alesis-keyboard'));
    await waitForPage(() => [...document.querySelectorAll('.midi-input-control__diagnostics dd')].some(dd => dd.textContent.trim() === 'Selected MIDI device disconnected'), 'MIDI disconnect was not shown.');
    const disconnectedSelection = await page.locator('select[aria-label="MIDI device"]').inputValue();
    await page.evaluate(async () => await window.__midiMock.addUnrelated());
    const afterUnrelated = await page.locator('select[aria-label="MIDI device"]').inputValue();
    const unrelatedHasHandler = await page.evaluate(() => Boolean(window.__midiMock.access.inputs.get('unrelated').onmidimessage));
    if (disconnectedSelection !== '__selected-unavailable' || afterUnrelated !== '__selected-unavailable' || unrelatedHasHandler) {
        throw new Error('An unrelated MIDI device replaced the disconnected selection.');
    }

    await page.evaluate(() => window.__midiMock.reconnect('alesis-keyboard'));
    await waitForPage(() => [...document.querySelectorAll('.midi-input-control__diagnostics dd')].some(dd => dd.textContent.trim() === 'MIDI device connected'), 'Exact MIDI device did not reconnect.');
    const finalSelection = await page.locator('select[aria-label="MIDI device"]').inputValue();
    const keyboardHandlerRestored = await page.evaluate(() => Boolean(window.__midiMock.keyboard.onmidimessage));
    if (finalSelection !== 'alesis-keyboard' || !keyboardHandlerRestored || errors.length > 0) {
        throw new Error(`MIDI reconnect failed or browser errors occurred: ${errors.join('; ')}`);
    }

    await page.getByRole('button', { name: 'Start metronome' }).click();
    await waitForPage(() => document.querySelector('.rhythm-workspace')?.dataset.state === 'active', 'Rhythm practice did not start.', 8_000);
    const midiTimelineStart = await page.evaluate(() => performance.now());
    for (let index = 0; index < 16; index++) {
        if (index > 0) await page.waitForTimeout(750);
        await page.evaluate(timestamp =>
            window.__midiMock.emit('alesis-keyboard', [0x90, 60, 100], timestamp),
        midiTimelineStart + index * 750);
    }
    await waitForPage(() => Boolean(document.querySelector('#attempt-result-title')), 'Browser-timestamped MIDI rhythm attempt did not finish.', 20_000);
    const rhythmResult = await page.locator('.practice-result-summary').innerText();
    if (!rhythmResult.includes('Successful hits\n16') ||
        !rhythmResult.includes('Missed beats\n0') ||
        !rhythmResult.includes('Average timing error')) {
        throw new Error(`Browser-timestamped MIDI rhythm input was not scored as expected: ${rhythmResult}`);
    }

    return {
        userAgent: await page.evaluate(() => navigator.userAgent),
        permissionRequestHasNoExtraOptions: requestArguments.length === 0,
        enumeratedInputs: optionNames,
        explicitlySelectedDevice: finalSelection,
        noteOnAndSustainReachedUi: true,
        noteOffAcceptedByTransport: true,
        disconnectDetected: true,
        unrelatedDeviceRejected: true,
        exactDeviceReconnected: true,
        browserRelativeRhythmTiming: rhythmResult,
        pageErrors: errors
    };
}
