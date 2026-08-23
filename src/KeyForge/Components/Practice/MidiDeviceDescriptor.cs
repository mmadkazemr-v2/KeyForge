namespace KeyForge.Components.Practice;

public sealed class MidiDeviceDescriptor
{
    public required string Id { get; init; }
    public string? Name { get; init; }
    public string? Manufacturer { get; init; }
    public string DisplayName => string.IsNullOrWhiteSpace(Manufacturer)
        ? Name ?? Id
        : $"{Manufacturer} {Name ?? Id}";
}
