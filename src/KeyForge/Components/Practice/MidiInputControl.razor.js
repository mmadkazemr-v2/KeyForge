let midiAccess;
let dotNetReference;
let selectedInput;
let selectedDeviceId;
let selectedDeviceName;
let selectedDeviceManufacturer;
let errorStatus;
let eventSequence = 0;
let messageDispatch = Promise.resolve();
let disposed = false;
let livePlayEnabled = false;
let liveAudioModule;
let diagnosticsEnabled = false;
const diagnosticCapacity = 512;
const diagnosticSamples = { callbackDelay: [], callbackDuration: [], interopAckRoundTrip: [] };
const diagnosticDropped = { callbackDelay: 0, callbackDuration: 0, interopAckRoundTrip: 0 };
let diagnosticInvalidSamples = 0;

export function setDiagnosticsEnabled(enabled) {
    diagnosticsEnabled = Boolean(enabled);
}

export function getDiagnostics() {
    const summarize = samples => {
        if (samples.length === 0) return { sampleCount: 0, median: null, p95: null, maximum: null, latestSequence: null };
        const values = samples.map(sample => sample.value).sort((a, b) => a - b);
        const percentile = p => values[Math.max(0, Math.ceil(values.length * p) - 1)];
        return {
            sampleCount: values.length, median: percentile(0.5), p95: percentile(0.95), maximum: values.at(-1),
            latestSequence: samples.at(-1).sequence
        };
    };
    return {
        enabled: diagnosticsEnabled,
        capacity: diagnosticCapacity,
        callbackDelay: { ...summarize(diagnosticSamples.callbackDelay), dropped: diagnosticDropped.callbackDelay },
        callbackDuration: { ...summarize(diagnosticSamples.callbackDuration), dropped: diagnosticDropped.callbackDuration },
        interopAckRoundTrip: { ...summarize(diagnosticSamples.interopAckRoundTrip), dropped: diagnosticDropped.interopAckRoundTrip },
        invalidSamples: diagnosticInvalidSamples
    };
}

function recordDiagnostic(name, sequence, value) {
    if (!diagnosticsEnabled) return;
    if (!Number.isFinite(value) || value < 0) {
        diagnosticInvalidSamples++;
        return;
    }
    const samples = diagnosticSamples[name];
    if (samples.length === diagnosticCapacity) {
        samples.shift();
        diagnosticDropped[name]++;
    }
    samples.push({ sequence, value });
}

export async function configureLivePlay(enabled, preset) {
    livePlayEnabled = Boolean(enabled);
    if (!livePlayEnabled) {
        liveAudioModule?.releaseLiveMidiNotes();
        return;
    }
    liveAudioModule ??= await import('../Audio/AudioPlaybackService.js');
    if (preset) liveAudioModule.setInstrumentPreset(preset);
}

export function isSupported() {
    return Boolean(navigator.requestMIDIAccess);
}

export function getBrowserTimestamp() {
    return performance.now();
}

export async function enable(reference) {
    if (!isSupported()) return unsupportedState();
    dotNetReference = reference;
    disposed = false;
    try {
        midiAccess ??= await navigator.requestMIDIAccess();
        midiAccess.onstatechange = notifyDevicesChanged;
        return getState();
    } catch (error) {
        const denied = error?.name === 'NotAllowedError' || error?.name === 'SecurityError';
        return {
            enabled: false,
            status: denied ? 2 : 5,
            selectedDeviceId: null,
            selectedDeviceName: null,
            selectedDeviceManufacturer: null,
            errorMessage: error?.message ?? String(error),
            devices: []
        };
    }
}

export async function selectInput(deviceId) {
    if (!midiAccess) return { ...getState(), status: 0 };
    const input = midiAccess.inputs.get(deviceId);
    if (!input || input.state !== 'connected') return getState();
    await detachSelectedInput();
    selectedDeviceId = input.id;
    selectedDeviceName = input.name ?? input.id;
    selectedDeviceManufacturer = input.manufacturer ?? null;
    errorStatus = undefined;
    try {
        await input.open();
        if (disposed || selectedDeviceId !== input.id || input.state !== 'connected') return getState();
        selectedInput = input;
        input.onmidimessage = handleMidiMessage;
    } catch (error) {
        errorStatus = { status: 7, message: error?.message ?? String(error) };
        selectedInput = undefined;
    }
    return getState();
}

export async function dispose() {
    disposed = true;
    diagnosticsEnabled = false;
    await detachSelectedInput();
    selectedDeviceId = undefined;
    selectedDeviceName = undefined;
    selectedDeviceManufacturer = undefined;
    if (midiAccess) midiAccess.onstatechange = null;
    dotNetReference = undefined;
}

function handleMidiMessage(event) {
    if (disposed || !dotNetReference || !event?.data || !selectedInput || selectedInput.state !== 'connected') return;
    const callbackStartedAt = diagnosticsEnabled ? performance.now() : 0;
    const timestampIsValid = Number.isFinite(event.timeStamp);
    const eventTimestamp = timestampIsValid ? event.timeStamp : performance.now();
    const [status = 0, data1 = 0, data2 = 0] = event.data;
    const message = {
        deviceId: selectedDeviceId ?? '',
        deviceName: selectedDeviceManufacturer
            ? `${selectedDeviceManufacturer} ${selectedDeviceName ?? ''}`.trim()
            : selectedDeviceName ?? '',
        status,
        data1,
        data2,
        timestampMilliseconds: eventTimestamp,
        sequence: ++eventSequence
    };
    if (diagnosticsEnabled) {
        if (timestampIsValid) recordDiagnostic('callbackDelay', message.sequence, callbackStartedAt - eventTimestamp);
        else diagnosticInvalidSamples++;
    }
    // Tone runs directly in this browser callback for playable latency. The
    // same input still reaches Blazor for diagnostics and keyboard display.
    if (livePlayEnabled && liveAudioModule) {
        try { liveAudioModule.handleMidiInput(status, data1, data2); }
        catch { /* Keep MIDI diagnostics flowing if audio is unavailable. */ }
    }
    if (diagnosticsEnabled) recordDiagnostic('callbackDuration', message.sequence, performance.now() - callbackStartedAt);
    messageDispatch = messageDispatch
        .then(() => {
            if (!disposed && selectedInput?.state === 'connected' && selectedDeviceId === message.deviceId) {
                const invokeStartedAt = diagnosticsEnabled ? performance.now() : 0;
                return dotNetReference?.invokeMethodAsync('HandleMidiMessage', message)?.then(() => {
                    if (diagnosticsEnabled) recordDiagnostic('interopAckRoundTrip', message.sequence, performance.now() - invokeStartedAt);
                });
            }
        })
        .catch(() => {
            if (diagnosticsEnabled) diagnosticInvalidSamples++;
        });
}

async function notifyDevicesChanged() {
    if (disposed || !midiAccess) return;
    if (selectedDeviceId) {
        const candidate = midiAccess.inputs.get(selectedDeviceId);
        if (candidate?.state === 'connected' && !selectedInput) {
            selectedDeviceName = candidate.name ?? selectedDeviceName;
            selectedDeviceManufacturer = candidate.manufacturer ?? selectedDeviceManufacturer;
            try {
                await candidate.open();
                if (!disposed && selectedDeviceId === candidate.id && candidate.state === 'connected') {
                    selectedInput = candidate;
                    candidate.onmidimessage = handleMidiMessage;
                    errorStatus = undefined;
                }
            } catch (error) {
                errorStatus = { status: 7, message: error?.message ?? String(error) };
            }
        } else if (selectedInput && (selectedInput.state !== 'connected' || !midiAccess.inputs.has(selectedDeviceId))) {
            await detachSelectedInput();
        }
    }
    await notifyState();
}

async function notifyState() {
    try {
        await dotNetReference?.invokeMethodAsync('HandleMidiDevicesChanged', getState());
    } catch {
        // A disconnected Blazor circuit no longer needs device-state updates.
    }
}

async function detachSelectedInput() {
    liveAudioModule?.releaseLiveMidiNotes();
    const previous = selectedInput;
    selectedInput = undefined;
    if (!previous) return;
    previous.onmidimessage = null;
    try {
        await previous.close();
    } catch {
        // The device can disappear while its port is being closed.
    }
}

function getDevices() {
    return midiAccess ? Array.from(midiAccess.inputs.values())
        .filter(input => input.state === 'connected')
        .map(input => ({ id: input.id, name: input.name, manufacturer: input.manufacturer })) : [];
}

function getState() {
    const devices = getDevices();
    let status = 0;
    if (midiAccess) {
        if (errorStatus) status = errorStatus.status;
        else if (selectedDeviceId && selectedInput?.state === 'connected') status = 4;
        else if (selectedDeviceId) status = 6;
        else if (devices.length === 0) status = 3;
        else status = 8;
    }
    return {
        enabled: Boolean(midiAccess),
        status,
        selectedDeviceId: selectedDeviceId ?? null,
        selectedDeviceName: selectedDeviceName ?? null,
        selectedDeviceManufacturer: selectedDeviceManufacturer ?? null,
        errorMessage: errorStatus?.message ?? null,
        devices
    };
}

function unsupportedState() {
    return {
        enabled: false, status: 1, selectedDeviceId: null,
        selectedDeviceName: null, selectedDeviceManufacturer: null,
        errorMessage: null, devices: []
    };
}
