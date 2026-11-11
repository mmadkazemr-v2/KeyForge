const activeRuns = new Map();

const sleep = (run, milliseconds) => new Promise(resolve => {
    if (run.cancelled || milliseconds <= 0) {
        resolve();
        return;
    }

    run.wakeSleep = resolve;
    run.pendingTimer = setTimeout(() => {
        run.pendingTimer = undefined;
        run.wakeSleep = undefined;
        resolve();
    }, milliseconds);
});

function formatNumber(value, digits) {
    return String(value).replace(/[0-9]/g, digit => digits[Number(digit)]);
}

function setText(root, selector, value) {
    const element = root.querySelector(selector);
    if (element) {
        element.textContent = value;
    }
}

function setState(root, state, label) {
    root.dataset.state = state;
    setText(root, "[data-rhythm-state]", label);
}

function showBeat(root, index, beatCount) {
    root.querySelectorAll("[data-rhythm-beat]").forEach(beat => {
        beat.classList.toggle("is-active", Number(beat.dataset.rhythmBeat) === index);
        if (Number(beat.dataset.rhythmBeat) < index) {
            beat.classList.add("is-complete");
        }
    });

    const progress = root.querySelector("[data-rhythm-progress]");
    const percentage = Math.round(((index + 1) / beatCount) * 100);
    if (progress) {
        progress.setAttribute("aria-valuenow", String(percentage));
        const value = progress.querySelector(".kf-progress__value");
        value?.style.setProperty("--kf-progress-value", `${percentage}%`);
    }
}

export async function startRhythm(
    workspaceId,
    dotNetReference,
    timelineEvents,
    pulseOffsetsMilliseconds,
    durationMilliseconds,
    labels) {
    stopRhythm(workspaceId);

    const root = document.getElementById(workspaceId);
    if (!root || !timelineEvents?.length || durationMilliseconds <= 0) {
        return;
    }

    resetRhythmDisplay(root);

    const run = {
        cancelled: false,
        active: false,
        timelineStartedAt: 0,
        pendingInputs: [],
        keyHandler: null,
        flashTimer: 0,
        pendingTimer: undefined,
        wakeSleep: undefined,
        timelineEvents,
        pulseOffsetsMilliseconds,
        durationMilliseconds,
        labels,
        dotNetReference,
        root,
        metronomeAudio: undefined,
        armed: Boolean(labels.firstMidiNote),
        started: false
    };
    activeRuns.set(workspaceId, run);
    let metronomeAudio;
    if (labels.audio && typeof labels.playClick !== "function") {
        try {
            metronomeAudio = await import("../Audio/AudioPlaybackService.js");
        } catch {
            // A click failure must not prevent timestamped exercise scoring.
        }
    }
    if (run.cancelled) return;
    run.metronomeAudio = metronomeAudio;
    root.focus({ preventScroll: true });
    if (run.armed) {
        setState(root, "waiting", labels.waiting);
        setText(root, "[data-rhythm-countdown]", labels.waiting);
        return;
    }
    const countInOrigin = performance.now();
    const countInBeats = labels.countInBeats || 0;
    const pulseInterval = labels.beatIntervalMilliseconds || 0;
    for (let beat = 0; beat < countInBeats; beat++) {
        await sleep(run, Math.max(0, countInOrigin + beat * pulseInterval - performance.now()));
        if (run.cancelled) return;
        setText(root, "[data-rhythm-countdown]", formatNumber(beat + 1, labels.digits));
        if (labels.audio) {
            try {
                if (typeof labels.playClick === "function") labels.playClick();
                else metronomeAudio?.playMetronomeClick();
            } catch { /* Scoring remains available without audio. */ }
        }
    }
    await sleep(run, Math.max(0, countInOrigin + countInBeats * pulseInterval - performance.now()));
    if (run.cancelled) return;
    startRhythmAt(workspaceId, countInOrigin + countInBeats * pulseInterval, labels.runVersion);
}

export function startRhythmAt(workspaceId, browserTimestampMilliseconds, runVersion) {
    const run = activeRuns.get(workspaceId);
    if (!run || run.cancelled || run.started || run.labels.runVersion !== runVersion ||
        !Number.isFinite(browserTimestampMilliseconds) || browserTimestampMilliseconds < 0) return;
    run.started = true;
    run.armed = false;
    const { root, dotNetReference, labels, timelineEvents, pulseOffsetsMilliseconds, durationMilliseconds, metronomeAudio } = run;
    run.timelineStartedAt = browserTimestampMilliseconds;
    // Start browser scheduling immediately; interop latency must not shift the pulse grid.
    const notification = dotNetReference.invokeMethodAsync("NotifyRhythmStarted", run.timelineStartedAt, labels.runVersion);
    run.pendingInputs.push(notification);
    notification.catch(() => { if (activeRuns.get(workspaceId) === run) stopRhythm(workspaceId); });
    if (run.cancelled) return;

    run.active = true;
    setState(root, "active", labels.active);

    run.keyHandler = event => {
        if (!run.active || event.code !== "KeyK" || event.repeat) {
            return;
        }

        event.preventDefault();
        const elapsedMilliseconds = performance.now() - run.timelineStartedAt;
        const promise = dotNetReference.invokeMethodAsync(
            "CaptureRhythmKeyboardInput",
            event.code,
            event.repeat,
            elapsedMilliseconds, labels.runVersion);
        run.pendingInputs.push(promise);

        const hitCount = root.querySelector("[data-rhythm-hit-count]");
        if (hitCount) {
            run.hitCount = (run.hitCount || 0) + 1;
            hitCount.textContent = formatNumber(run.hitCount, labels.digits);
        }

        root.classList.remove("is-hit");
        void root.offsetWidth;
        root.classList.add("is-hit");
        clearTimeout(run.flashTimer);
        run.flashTimer = setTimeout(() => root.classList.remove("is-hit"), 120);
    };
    document.addEventListener("keydown", run.keyHandler);

void runRhythmTimeline(workspaceId, run, root, dotNetReference, timelineEvents, pulseOffsetsMilliseconds, durationMilliseconds, labels, metronomeAudio)
        .catch(() => stopRhythm(workspaceId));
}

async function runRhythmTimeline(workspaceId, run, root, dotNetReference, timelineEvents, pulseOffsetsMilliseconds, durationMilliseconds, labels, metronomeAudio) {
    let eventIndex = 0;
    let pulseIndex = 0;
    while (eventIndex < timelineEvents.length || pulseIndex < pulseOffsetsMilliseconds.length) {
        const eventOffset = eventIndex < timelineEvents.length
            ? timelineEvents[eventIndex].offsetMilliseconds
            : Number.POSITIVE_INFINITY;
        const pulseOffset = pulseIndex < pulseOffsetsMilliseconds.length
            ? pulseOffsetsMilliseconds[pulseIndex]
            : Number.POSITIVE_INFINITY;
        const nextOffset = Math.min(eventOffset, pulseOffset);
        await sleep(run, Math.max(0, run.timelineStartedAt + nextOffset - performance.now()));
        if (run.cancelled) return;

        if (eventOffset <= pulseOffset) {
            showBeat(root, eventIndex, timelineEvents.length);
            setText(root, "[data-rhythm-countdown]", formatNumber(eventIndex + 1, labels.digits));
            eventIndex++;
        }
        if (pulseOffset <= eventOffset) {
            if (labels.audio && (metronomeAudio || typeof labels.playClick === "function")) {
                try {
                    if (typeof labels.playClick === "function") labels.playClick();
                    else metronomeAudio.playMetronomeClick();
                } catch {
                    // Keep the musical timeline active if browser audio is unavailable.
                }
            }
            pulseIndex++;
        }
    }

    const endTarget = run.timelineStartedAt + durationMilliseconds;
    await sleep(run, Math.max(0, endTarget - performance.now()));
    if (run.cancelled) return;

    run.active = false;
    document.removeEventListener("keydown", run.keyHandler);
    setState(root, "evaluating", labels.evaluating);
    setText(root, "[data-rhythm-countdown]", labels.evaluating);
    await Promise.allSettled(run.pendingInputs);

    if (!run.cancelled) {
        await dotNetReference.invokeMethodAsync("CompleteRhythm", labels.runVersion);
    }
    if (activeRuns.get(workspaceId) === run) activeRuns.delete(workspaceId);
}

export function stopRhythm(workspaceId) {
    const run = activeRuns.get(workspaceId);
    if (!run) {
        return;
    }

    run.cancelled = true;
    run.active = false;
    clearTimeout(run.flashTimer);
    if (run.pendingTimer !== undefined) {
        clearTimeout(run.pendingTimer);
        run.pendingTimer = undefined;
        const wake = run.wakeSleep;
        run.wakeSleep = undefined;
        wake?.();
    }
    if (run.keyHandler) {
        document.removeEventListener("keydown", run.keyHandler);
    }
    activeRuns.delete(workspaceId);
}

export function setRhythmPaused(workspaceId, label) {
    stopRhythm(workspaceId);
    const root = document.getElementById(workspaceId);
    if (!root) return;
    resetRhythmDisplay(root);
    setState(root, "ready", label);
    setText(root, "[data-rhythm-countdown]", label);
}

function resetRhythmDisplay(root) {
    root.querySelectorAll("[data-rhythm-beat]").forEach(beat => {
        beat.classList.remove("is-active", "is-complete");
    });
    const progress = root.querySelector("[data-rhythm-progress]");
    progress?.setAttribute("aria-valuenow", "0");
    progress?.querySelector(".kf-progress__value")?.style.setProperty("--kf-progress-value", "0%");
    const hitCount = root.querySelector("[data-rhythm-hit-count]");
    if (hitCount) hitCount.textContent = "0";
    root.classList.remove("is-hit");
}
