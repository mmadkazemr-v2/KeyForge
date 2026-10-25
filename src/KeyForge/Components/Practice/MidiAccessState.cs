namespace KeyForge.Components.Practice;

public sealed class MidiAccessState
{
    public bool Enabled { get; init; }
    public MidiInputStatus Status { get; init; }
    public string? SelectedDeviceId { get; init; }
    public string? SelectedDeviceName { get; init; }
    public string? SelectedDeviceManufacturer { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<MidiDeviceDescriptor>? Devices { get; init; }
}

public enum MidiInputStatus
{
    Disabled,
    Unsupported,
    PermissionDenied,
    NoDevice,
    Connected,
    Error,
    DeviceDisconnected,
    DeviceOpenFailure,
    DeviceSelectionRequired
}
