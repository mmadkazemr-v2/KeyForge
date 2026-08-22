namespace KeyForge.Components.Practice;

/// <summary>Normalizes a virtual-piano selection into the shared runtime event.</summary>
public static class VirtualPianoInputAdapter
{
    public static ExerciseInputEvent Create(
        MusicalNote note,
        DateTime exerciseStartedAt,
        DateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(note);

        if (occurredAt < exerciseStartedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(occurredAt));
        }

        return new ExerciseInputEvent
        {
            Timestamp = occurredAt,
            Offset = occurredAt - exerciseStartedAt,
            Source = InputSource.VirtualPiano,
            Type = InputEventType.Note,
            Value = note.Value
        };
    }
}
