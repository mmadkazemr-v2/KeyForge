// Run with browser_run_code_unsafe(filename: ...) against a real Chrome or Edge tab.
// Checks actual Web Audio output signals; speaker/headphone audibility still needs a listener.
async (page) => {
    // A fresh context avoids previous site activation masking autoplay defects.
    page = await page.context().browser().newPage();
    const errors = [];
    const onError = error => errors.push(error.message);
    const onConsole = message => { if (message.type() === 'error') errors.push(message.text()); };
    page.on('pageerror', onError);
    page.on('console', onConsole);
    const check = (condition, message) => { if (!condition) throw new Error(message); };
    const sample = (duration = 550) => page.evaluate(async duration => {
        if (!globalThis.__audioProbe) return 0;
        const values = new Float32Array(__audioProbe.fftSize);
        let peak = 0;
        const until = performance.now() + duration;
        while (performance.now() < until) {
            __audioProbe.getFloatTimeDomainData(values);
            for (const value of values) peak = Math.max(peak, Math.abs(value));
            await new Promise(resolve => setTimeout(resolve, 8));
        }
        return peak;
    }, duration);

    try {
        await page.goto('http://127.0.0.1:5264/language/en?returnUrl=/dev/audio');
        await page.bringToFront();
        await page.waitForFunction(() => globalThis.Tone?.version === '15.1.22');
        await page.evaluate(async () => {
            // Also exercise resume when the browser already permits this origin to play.
            await Tone.getContext().rawContext.suspend();
            globalThis.__resumeChecks = [];
            const resume = AudioContext.prototype.resume;
            AudioContext.prototype.resume = function (...args) {
                __resumeChecks.push({ active: navigator.userActivation.isActive, state: this.state });
                if (globalThis.__failAudioResume) throw new TypeError('Audio verification failure');
                return resume.apply(this, args);
            };
            globalThis.__failAudioResume = true;
        });
        await page.getByRole('button', { name: 'Enable audio', exact: true }).click();
        await page.locator('.audio-activation__error').waitFor();
        check((await page.locator('.audio-activation__error').innerText()).includes('unavailable'), 'A JavaScript error was mislabeled as a permission denial');
        await page.evaluate(() => { __failAudioResume = false; });
        await page.getByRole('button', { name: 'Enable audio', exact: true }).click();
        await page.getByRole('button', { name: 'Play note C4', exact: true }).waitFor();
        const activation = await page.evaluate(() => ({ state: Tone.getContext().state, resumes: __resumeChecks }));
        check(activation.state === 'running', 'Audio context did not become ready');
        check(activation.resumes.some(resume => resume.active), 'Resume did not execute during a browser user gesture');

        await page.evaluate(() => {
            globalThis.__testContext = Tone.getContext().rawContext;
            globalThis.__audioProbe = __testContext.createAnalyser();
            __audioProbe.fftSize = 2048;
            Tone.getDestination().connect(__audioProbe);
            globalThis.__clickTimes = [];
            const click = Tone.MembraneSynth.prototype.triggerAttackRelease;
            Tone.MembraneSynth.prototype.triggerAttackRelease = function (...args) {
                __clickTimes.push(performance.now());
                return click.apply(this, args);
            };
        });

        const notes = {};
        for (const note of ['C4', 'E4', 'G4', 'C5']) {
            await page.getByRole('button', { name: `Play note ${note}`, exact: true }).click();
            notes[note] = await sample();
            check(notes[note] > 0.001, `No note output signal for ${note}`);
        }

        const metronome = [];
        for (const bpm of [60, 80, 120]) {
            if (bpm === 120) {
                for (let step = 0; step < 8; step++)
                    await page.getByRole('button', { name: 'Increase tempo', exact: true }).click();
            } else {
                await page.locator('.tempo-selector').getByRole('button', { name: `${bpm} BPM`, exact: bpm !== 80 }).click();
            }
            await page.evaluate(() => { __clickTimes.length = 0; });
            await page.getByRole('button', { name: 'Start metronome', exact: true }).click();
            const clickPeak = await sample(1200);
            check(clickPeak > 0.001, `No metronome output signal at ${bpm} BPM`);
            await page.getByRole('button', { name: 'Play note G4', exact: true }).click();
            const combinedPeak = await sample();
            await page.waitForFunction(() => __clickTimes.length >= 4, undefined, { timeout: 6500 });
            const clicks = await page.evaluate(() => [...__clickTimes]);
            await page.getByRole('button', { name: 'Stop metronome', exact: true }).click();
            await page.waitForTimeout(1500);
            const silence = await sample(150);
            const stoppedCount = await page.evaluate(() => __clickTimes.length);
            check(stoppedCount === clicks.length, 'Clicks continued after stopping');
            check(silence < 0.0001, 'Audio did not settle to silence after stop');
            check(await page.evaluate(() => __testContext === Tone.getContext().rawContext), 'Audio context was recreated');
            metronome.push({ bpm, clickPeak, combinedPeak, clicks: clicks.length, intervalsMs: clicks.slice(1).map((time, i) => Math.round(time - clicks[i])), silence });
        }

        await page.getByRole('button', { name: 'Start metronome', exact: true }).click();
        check(await sample(800) > 0.001, 'Restart produced no output');
        await page.getByRole('link', { name: 'Home', exact: true }).last().click();
        await page.waitForTimeout(1500);
        check(await sample(150) < 0.0001, 'Navigation left audio playing');
        check(errors.length === 0, `Browser errors: ${errors.join('; ')}`);
        return { userAgent: await page.evaluate(() => navigator.userAgent), retryAfterFailure: 'passed', activation, notes, metronome, restart: 'passed', navigation: 'passed', errors };
    } finally {
        page.off('pageerror', onError);
        page.off('console', onConsole);
    }
}
