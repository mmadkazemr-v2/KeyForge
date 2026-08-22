const activeRuns = new Map();

const sleep = milliseconds => new Promise(resolve => setTimeout(resolve, milliseconds));

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
    beatIntervalMilliseconds,
    beatCount,
    labels) {
    stopRhythm(workspaceId);

    const root = document.getElementById(workspaceId);
    if (!root || beatCount <= 0 || beatIntervalMilliseconds <= 0) {
        return;
    }

    const run = {
        cancelled: false,
        active: false,
        timelineStartedAt: 0,
        pendingInputs: [],
        keyHandler: null,
        flashTimer: 0
    };
    activeRuns.set(workspaceId, run);
    root.focus({ preventScroll: true });
    setState(root, "countdown", labels.countdown);

    for (const value of [3, 2, 1]) {
        if (run.cancelled) return;
        setText(root, "[data-rhythm-countdown]", formatNumber(value, labels.digits));
        await sleep(1000);
    }

    if (run.cancelled) return;
    setText(root, "[data-rhythm-countdown]", labels.start);
    await sleep(400);
    if (run.cancelled) return;

    await dotNetReference.invokeMethodAsync("NotifyRhythmStarted");
    if (run.cancelled) return;

    run.timelineStartedAt = performance.now();
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
            elapsedMilliseconds);
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

    for (let index = 0; index < beatCount; index++) {
        const target = run.timelineStartedAt + index * beatIntervalMilliseconds;
        await sleep(Math.max(0, target - performance.now()));
        if (run.cancelled) return;
        showBeat(root, index, beatCount);
        setText(root, "[data-rhythm-countdown]", formatNumber(index + 1, labels.digits));
    }

    const endTarget = run.timelineStartedAt + beatCount * beatIntervalMilliseconds;
    await sleep(Math.max(0, endTarget - performance.now()));
    if (run.cancelled) return;

    run.active = false;
    document.removeEventListener("keydown", run.keyHandler);
    setState(root, "evaluating", labels.evaluating);
    setText(root, "[data-rhythm-countdown]", labels.evaluating);
    await Promise.allSettled(run.pendingInputs);

    if (!run.cancelled) {
        await dotNetReference.invokeMethodAsync("CompleteRhythm");
    }
}

export function stopRhythm(workspaceId) {
    const run = activeRuns.get(workspaceId);
    if (!run) {
        return;
    }

    run.cancelled = true;
    run.active = false;
    clearTimeout(run.flashTimer);
    if (run.keyHandler) {
        document.removeEventListener("keydown", run.keyHandler);
    }
    activeRuns.delete(workspaceId);
}
