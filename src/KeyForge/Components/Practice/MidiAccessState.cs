namespace KeyForge.Components.Practice;

public sealed class MidiAccessState
{
    public bool Enabled { get; init; }
    public MidiInputStatus Status { get; init; }
    public string? SelectedDeviceId { get; init; }
    public IReadOnlyList<MidiDeviceDescriptor>? Devices { get; init; }
}

public enum MidiInputStatus
{
    Disabled,
    Unsupported,
    PermissionDenied,
    NoDevice,
    Connected,
    Error
}
