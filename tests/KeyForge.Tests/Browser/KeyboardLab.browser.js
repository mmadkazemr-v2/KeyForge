async (page) => {
    const errors = [];
    const waitFor = async (predicate, message, timeout = 8000) => {
        const deadline = Date.now() + timeout;
        while (Date.now() < deadline) {
            if (await page.evaluate(predicate)) return;
            await page.waitForTimeout(50);
        }
        throw new Error(message);
    };
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
    await page.addInitScript(() => {
        const port = {
            id: 'v61-performance', name: 'V61 MKII', manufacturer: 'Alesis', state: 'connected',
            connection: 'closed', onmidimessage: null,
            async open() { this.connection = 'open'; return this; },
            async close() { this.connection = 'closed'; return this; }
        };
        const access = { inputs: new Map([[port.id, port]]), onstatechange: null };
        Object.defineProperty(navigator, 'requestMIDIAccess', {
            configurable: true, value: async (...args) => { window.__midiArgs = args; return access; }
        });
        window.__midiMock = {
            port, access,
            emit(data, timestamp = performance.now()) { port.onmidimessage?.({ data, timeStamp: timestamp }); },
            async disconnect() { port.state = 'disconnected'; await access.onstatechange?.({ port }); },
            async reconnect() { port.state = 'connected'; await access.onstatechange?.({ port }); }
        };
    });

    await page.goto('http://127.0.0.1:5264/language/en?returnUrl=%2F');
    await page.getByRole('link', { name: 'Keyboard Lab', exact: true }).click();
    await waitFor(() => location.pathname === '/keyboard-lab', 'Keyboard Lab route did not open.');
    await page.waitForFunction(() => globalThis.Tone?.version === '15.1.22');
    if (await page.locator('.practice-session, .practice-feedback').count())
        throw new Error('Keyboard Lab rendered a lesson session or attempt feedback.');
    if (await page.locator('.piano-keyboard__key').count() < 61)
        throw new Error('The free-play keyboard does not show the expected V61 key range.');

    await page.getByRole('button', { name: 'Enable audio', exact: true }).click();
    await page.getByRole('button', { name: 'Enable MIDI', exact: true }).click();
    await page.locator('select[aria-label="MIDI device"]').selectOption('v61-performance');
    await waitFor(() => document.body.innerText.includes('MIDI device connected'), 'Selected mock MIDI device did not connect.');
    const midiRequestArgs = await page.evaluate(() => window.__midiArgs);
    if (midiRequestArgs.length !== 0) throw new Error('MIDI access requested unnecessary privileges.');
    if (!await page.locator('.midi-input-control__diagnostics').innerText().then(text => text.includes('Alesis')))
        throw new Error('Selected device manufacturer/name was not shown.');

    await page.evaluate(() => {
        const context = Tone.getContext().rawContext;
        window.__probe = context.createAnalyser();
        window.__probe.fftSize = 2048;
        Tone.getDestination().connect(window.__probe);
        window.__attackEvents = [];
        const attack = Tone.PolySynth.prototype.triggerAttack;
        Tone.PolySynth.prototype.triggerAttack = function (...args) {
            window.__attackEvents.push({ args, at: performance.now() });
            return attack.apply(this, args);
        };
        window.__shortNotes = [];
        const attackRelease = Tone.PolySynth.prototype.triggerAttackRelease;
        Tone.PolySynth.prototype.triggerAttackRelease = function (...args) {
            window.__shortNotes.push(args);
            return attackRelease.apply(this, args);
        };
        window.__clickEvents = [];
        const click = Tone.MembraneSynth.prototype.triggerAttackRelease;
        Tone.MembraneSynth.prototype.triggerAttackRelease = function (...args) {
            window.__clickEvents.push(performance.now());
            return click.apply(this, args);
        };
    });
    const peak = duration => page.evaluate(async duration => {
        const values = new Float32Array(__probe.fftSize);
        let maximum = 0;
        const until = performance.now() + duration;
        while (performance.now() < until) {
            __probe.getFloatTimeDomainData(values);
            for (const value of values) maximum = Math.max(maximum, Math.abs(value));
            await new Promise(resolve => setTimeout(resolve, 8));
        }
        return maximum;
    }, duration);

    // Direct Tone attack occurs before the delayed Blazor presentation message arrives.
    await page.evaluate(() => window.__midiMock.emit([0x90, 60, 30]));
    const directAttack = await page.evaluate(() => __attackEvents.at(-1));
    if (!directAttack || directAttack.args[0] !== 'C4' || directAttack.args[2] !== 30 / 127)
        throw new Error('MIDI NoteOn did not reach the local Tone path with velocity before UI dispatch.');
    await waitFor(() => {
        const diagnostics = document.querySelector('.keyboard-lab__diagnostics')?.innerText ?? '';
        return diagnostics.includes('C4') && diagnostics.includes('30') && diagnostics.includes('1');
    }, 'MIDI note, velocity, or channel diagnostics did not update.');
    const softPeak = await peak(100);
    await page.evaluate(() => window.__midiMock.emit([0x80, 60, 0]));
    await waitFor(() => !document.querySelector('.piano-keyboard__key--midi-active'), 'NoteOff did not clear the live key highlight.');
    await page.waitForTimeout(350);
    await page.evaluate(() => window.__midiMock.emit([0x90, 60, 120]));
    const loudPeak = await peak(100);
    if (softPeak <= 0.0001 || loudPeak <= softPeak * 1.2)
        throw new Error(`Velocity did not produce distinct audible levels (soft ${softPeak}, loud ${loudPeak}).`);
    await page.evaluate(() => window.__midiMock.emit([0x90, 60, 0]));

    // Chord and sustain use the same shared poly synth.
    await page.evaluate(() => {
        __midiMock.emit([0x90, 60, 90]);
        __midiMock.emit([0x90, 64, 90]);
        __midiMock.emit([0x90, 67, 90]);
    });
    const chordPeak = await peak(120);
    await page.evaluate(() => {
        __midiMock.emit([0xb0, 64, 127]);
        __midiMock.emit([0x80, 60, 0]);
        __midiMock.emit([0x80, 64, 0]);
        __midiMock.emit([0x80, 67, 0]);
    });
    const sustainedPeak = await peak(150);
    await waitFor(() => document.querySelector('.keyboard-lab__diagnostics')?.innerText.includes('Pedal down'), 'Sustain-down status did not update.');
    await page.evaluate(() => __midiMock.emit([0xb0, 64, 0]));
    await waitFor(() => document.querySelector('.keyboard-lab__diagnostics')?.innerText.includes('Pedal up'), 'Sustain-up status did not update.');
    if (chordPeak <= 0.0001 || sustainedPeak <= 0.0001)
        throw new Error('Chord or pedal-held audio was not audible.');
    await page.waitForTimeout(1100);

    const presetSelect = page.locator('select[aria-label="Practice sounds"]');
    await page.evaluate(() => { window.__contextBeforePresets = Tone.getContext().rawContext; });
    const presetIds = await presetSelect.locator('option').evaluateAll(options => options.map(option => option.value));
    if (presetIds.length < 6) throw new Error('Fewer than six practice sounds are available.');
    for (const presetId of presetIds) {
        await presetSelect.selectOption(presetId);
        await page.evaluate(() => window.__midiMock.emit([0x90, 67, 90]));
        await page.evaluate(() => window.__midiMock.emit([0x80, 67, 0]));
    }
    if (!await page.evaluate(() => Tone.getContext().rawContext === window.__contextBeforePresets))
        throw new Error('Changing practice sounds recreated the AudioContext.');

    await page.getByRole('button', { name: 'Piano key, note C4' }).click();
    await waitFor(() => __shortNotes.some(note => note[0] === 'C4'), 'Virtual piano input did not use the selected instrument sound.');
    const virtualPianoPeak = await peak(180);
    if (virtualPianoPeak <= 0.0001) throw new Error('Virtual piano note produced no audio output.');
    await page.getByRole('button', { name: 'Decrease tempo' }).click();
    await page.getByRole('button', { name: 'Decrease tempo' }).click();
    await page.getByRole('button', { name: 'Decrease tempo' }).click();
    await page.getByRole('button', { name: 'Decrease tempo' }).click();
    await page.getByRole('button', { name: 'Start metronome' }).click();
    await waitFor(() => document.querySelector('.metronome-control__header h3')?.textContent.includes('60'), 'Standalone metronome did not start at 60 BPM.');
    await page.evaluate(() => window.__midiMock.emit([0x90, 72, 90]));
    const combinedPeak = await peak(500);
    await waitFor(() => __clickEvents.length >= 2, 'Metronome did not produce Tone clicks.', 6000);
    await page.evaluate(() => window.__midiMock.emit([0x80, 72, 0]));
    await page.getByRole('button', { name: 'Stop metronome' }).click();
    if (combinedPeak <= 0.0001) throw new Error('Instrument and metronome did not coexist audibly.');

    await page.evaluate(() => window.__midiMock.emit([0x90, 60, 110]));
    await page.evaluate(() => window.__midiMock.disconnect());
    await waitFor(() => document.body.innerText.includes('Selected MIDI device disconnected'), 'Device disconnect was not reflected in the diagnostics.');
    await page.waitForTimeout(500);
    const disconnected = await page.locator('.piano-keyboard__key--midi-active, .piano-keyboard__key--midi-sustained').count();
    if (disconnected !== 0) throw new Error('Active notes remained highlighted after MIDI disconnect.');
    await page.evaluate(() => window.__midiMock.reconnect());
    await waitFor(() => document.body.innerText.includes('MIDI device connected'), 'The selected mock device did not reconnect.');
    await page.evaluate(() => window.__midiMock.emit([0x90, 64, 100]));
    await page.getByRole('link', { name: 'Home', exact: true }).click();
    await waitFor(() => location.pathname === '/', 'Navigating away from Keyboard Lab did not complete.');
    await page.waitForTimeout(700);
    const navigationPeak = await peak(180);
    if (navigationPeak > 0.0001) throw new Error('Navigating away left live MIDI audio playing.');
    if (errors.length) throw new Error(`Browser errors: ${errors.join('; ')}`);
    return {
        browser: await page.evaluate(() => navigator.userAgent),
        routeAndNavigation: 'passed',
        noLessonSessionOrProgressUi: true,
        keyboardKeys: await page.locator('.piano-keyboard__key').count(),
        localMidiToneAttack: directAttack,
        softPeak,
        loudPeak,
        chordPeak,
        sustainedPeak,
        virtualPianoPeak,
        presets: presetIds,
        metronomeAndInstrumentPeak: combinedPeak,
        disconnectClearedKeys: disconnected === 0,
        reconnectAndNavigationCleanup: navigationPeak <= 0.0001,
        pageErrors: errors
    };
};
