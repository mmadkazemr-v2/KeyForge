let midiAccess;
let dotNetReference;
let selectedInput;
let selectedDeviceId;

export function isSupported() {
    return Boolean(navigator.requestMIDIAccess);
}

export async function enable(reference) {
    if (!isSupported()) {
        return unsupportedState();
    }

    dotNetReference = reference;
    try {
        midiAccess ??= await navigator.requestMIDIAccess();
        midiAccess.onstatechange = notifyDevicesChanged;
        const devices = getDevices();
        if (!selectedDeviceId && devices.length > 0) {
            setSelectedInput(devices[0].id);
        }
        return getState();
    } catch (error) {
        return {
            enabled: false,
            status: error?.name === 'NotAllowedError' ? 2 : 5,
            selectedDeviceId: null,
            devices: []
        };
    }
}

export function selectInput(deviceId) {
    if (!midiAccess) {
        return { enabled: false, status: 0, selectedDeviceId: null, devices: [] };
    }

    setSelectedInput(deviceId);
    return getState();
}

export function dispose() {
    if (selectedInput) {
        selectedInput.onmidimessage = null;
    }
    selectedInput = undefined;
    selectedDeviceId = undefined;
    if (midiAccess) {
        midiAccess.onstatechange = null;
    }
    dotNetReference = undefined;
}

function setSelectedInput(deviceId) {
    if (selectedInput) {
        selectedInput.onmidimessage = null;
    }

    selectedInput = midiAccess?.inputs.get(deviceId);
    selectedDeviceId = selectedInput ? deviceId : undefined;
    if (selectedInput) {
        selectedInput.onmidimessage = handleMidiMessage;
    }
}

function handleMidiMessage(event) {
    if (!dotNetReference || !event?.data) {
        return;
    }

    const [status = 0, data1 = 0, data2 = 0] = event.data;
    dotNetReference.invokeMethodAsync('HandleMidiMessage', {
        deviceId: selectedDeviceId ?? '',
        status,
        data1,
        data2,
        timestampMilliseconds: event.timeStamp ?? performance.now()
    });
}

function notifyDevicesChanged() {
    if (selectedDeviceId && !midiAccess.inputs.has(selectedDeviceId)) {
        if (selectedInput) {
            selectedInput.onmidimessage = null;
        }
        selectedInput = undefined;
        selectedDeviceId = undefined;
    }
    dotNetReference?.invokeMethodAsync('HandleMidiDevicesChanged', getState());
}

function getDevices() {
    return midiAccess ? Array.from(midiAccess.inputs.values())
        .filter(input => input.state === 'connected')
        .map(input => ({ id: input.id, name: input.name, manufacturer: input.manufacturer })) : [];
}

function getState() {
    const devices = getDevices();
    return {
        enabled: Boolean(midiAccess),
        status: devices.length === 0 ? 3 : selectedInput ? 4 : 3,
        selectedDeviceId: selectedDeviceId ?? null,
        devices
    };
}

function unsupportedState() {
    return { enabled: false, status: 1, selectedDeviceId: null, devices: [] };
}
