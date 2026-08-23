namespace KeyForge.Components.Practice;

/// <summary>Normalizes a virtual-piano selection into the shared runtime event.</summary>
public static class VirtualPianoInputAdapter
{
    public static ExerciseInputEvent Create(
        MusicalNote note,
        DateTime exerciseStartedAt,
        DateTime occurredAt,
        DateTime? timelineStartedAt = null)
    {
        ArgumentNullException.ThrowIfNull(note);

        if (occurredAt < exerciseStartedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(occurredAt));
        }

        var offsetOrigin = timelineStartedAt ?? exerciseStartedAt;
        if (offsetOrigin < exerciseStartedAt || occurredAt < offsetOrigin)
        {
            throw new ArgumentOutOfRangeException(nameof(timelineStartedAt));
        }

        return new ExerciseInputEvent
        {
            Timestamp = occurredAt,
            Offset = occurredAt - offsetOrigin,
            Source = InputSource.VirtualPiano,
            Type = InputEventType.Note,
            Value = note.Value
        };
    }
}
