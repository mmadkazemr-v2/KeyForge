namespace KeyForge.Components.Practice;

/// <summary>Presentation-only interpretation of a MIDI message.</summary>
public sealed class MidiPresentationEvent
{
    public required MidiPresentationEventType Type { get; init; }
    public MusicalNote? Note { get; init; }
    public int? Velocity { get; init; }
    public bool? IsSustainDown { get; init; }
}

public enum MidiPresentationEventType
{
    NoteOn,
    NoteOff,
    SustainChanged
}
