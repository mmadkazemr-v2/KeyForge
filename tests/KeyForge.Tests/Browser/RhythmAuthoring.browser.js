// Run the app on port 5266 with KeyForge:LessonCatalog:ContentPath pointing to Fixtures/Rhythm.
async page => {
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    await page.addInitScript(() => {
        const port = {
            id: 'rhythm-mock', name: 'Rhythm MIDI test source', manufacturer: 'KeyForge tests',
            state: 'connected', connection: 'closed', onmidimessage: null,
            async open() { this.connection = 'open'; return this; },
            async close() { this.connection = 'closed'; return this; }
        };
        Object.defineProperty(navigator, 'requestMIDIAccess', {
            configurable: true,
            value: async () => ({ inputs: new Map([[port.id, port]]), onstatechange: null })
        });
        window.__rhythmMidi = port;
    });
    await page.goto('http://127.0.0.1:5266/language/en?returnUrl=%2Fpractice%2Frhythm-authoring-fixture');
    await page.waitForSelector('.vexflow-staff svg');
    const staffCount = await page.locator('.vexflow-staff svg').count();
    const noteAndRestCount = await page.locator('.vexflow-staff .vf-stavenote').count();
    if (staffCount !== 2 || noteAndRestCount !== 8)
        throw new Error(`Expected two rendered measures with eight note/rest events; got ${staffCount}/${noteAndRestCount}.`);

    await page.locator('.practice-midi-panel summary').click();
    await page.getByRole('button', { name: 'Enable MIDI', exact: true }).click();
    await page.locator('select[aria-label="MIDI device"]').selectOption('rhythm-mock');
    await page.waitForFunction(() => document.querySelector('select[aria-label="MIDI device"]')?.value === 'rhythm-mock' &&
        Boolean(window.__rhythmMidi.onmidimessage));

    await page.evaluate(() => {
        const root = document.querySelector('.rhythm-workspace');
        const observer = new MutationObserver(() => {
            if (root.dataset.state !== 'active') return;
            observer.disconnect();
            const base = performance.now();
            const play = offset => {
                window.__rhythmMidi.onmidimessage({ data: [0x96, 60, 96], timeStamp: base + offset });
                window.__rhythmMidi.onmidimessage({ data: [0x86, 60, 0], timeStamp: base + offset + 20 });
            };
            // Seven authored onsets, plus one unexpected hit in the first rest's interior.
            for (const offset of [0, 500, 750, 1125, 1500, 2000, 2750, 3000])
                setTimeout(() => play(offset), offset);
        });
        observer.observe(root, { attributes: true, attributeFilter: ['data-state'] });
    });
    await page.getByRole('button', { name: 'Start metronome', exact: true }).click();
    await page.waitForSelector('#attempt-result-title', { timeout: 15_000 });
    const result = await page.locator('.practice-result-summary').innerText();
    if (!/Successful hits\s+7/.test(result) || !/Missed beats\s+0/.test(result) || !/Extra hits\s+1/.test(result))
        throw new Error(`Non-uniform MIDI rhythm did not produce seven matched hits and one extra: ${result}`);

    // Render every supported duration/rest through the actual vendored VexFlow engine.
    const notation = await page.evaluate(async () => {
        const stage = document.createElement('div');
        stage.style.width = '600px';
        document.body.appendChild(stage);
        try {
            const module = await import('/Components/Music/VexFlowStaff.razor.js');
            await module.renderStaff(stage, {
                clef: 'percussion', noteKey: 'b/4', timeSignature: '4/4',
                rhythmMeasures: [
                    [{ duration: 'w' }],
                    [{ duration: 'wr' }],
                    [{ duration: 'hd', dotted: true }, { duration: 'qr' }],
                    [{ duration: '8d', dotted: true }, { duration: '16' }, { duration: 'h' }, { duration: 'q' }],
                    Array.from({ length: 16 }, () => ({ duration: '16' })),
                    [{ duration: 'hr' }, { duration: 'qr' }, { duration: '8r' }, { duration: '16r' }, { duration: '16r' }],
                    [{ duration: 'qdr', dotted: true }, { duration: '8r' }, { duration: 'hr' }]
                ]
            });
            const measures = stage.querySelectorAll('svg').length;
            const notes = stage.querySelectorAll('.vf-stavenote').length;
            if (measures !== 7 || notes !== 32) throw new Error(`Notation render failed: ${measures}/${notes}.`);
            await module.renderStaff(stage, {
                clef: 'percussion', noteKey: 'b/4', timeSignature: '6/8',
                rhythmMeasures: [Array.from({ length: 6 }, () => ({ duration: '8' }))]
            });
            const sixEightBeams = stage.querySelectorAll('.vf-beam').length;
            if (sixEightBeams !== 2) throw new Error(`6/8 should beam two groups of three eighths, got ${sixEightBeams}.`);
            return { measures, notes, sixEightBeams };
        } finally { stage.remove(); }
    });
    if (errors.length) throw new Error(errors.join('; '));
    return { browser: await page.evaluate(() => navigator.userAgent), staffCount, noteAndRestCount, result, notation, pageErrors: errors };
}
