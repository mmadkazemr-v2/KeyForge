namespace KeyForge.Components.Practice;

/// <summary>Normalizes standard MIDI note messages into the practice runtime input contract.</summary>
public static class MidiInputAdapter
{
    private const int NoteOnCommand = 0x90;
    private const int NoteOffCommand = 0x80;

    public static ExerciseInputEvent? TryCreate(
        MidiInputMessage message,
        DateTime exerciseStartedAt,
        DateTime occurredAt,
        DateTime? timelineStartedAt = null)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!IsNoteOn(message) || occurredAt < exerciseStartedAt)
        {
            return null;
        }

        var timelineStart = timelineStartedAt ?? exerciseStartedAt;
        if (timelineStart < exerciseStartedAt || occurredAt < timelineStart)
        {
            return null;
        }

        return new ExerciseInputEvent
        {
            Timestamp = occurredAt,
            Offset = occurredAt - timelineStart,
            Source = InputSource.Midi,
            Type = InputEventType.Note,
            Value = MusicalNote.FromMidiNumber(message.Data1).Value,
            Velocity = message.Data2
        };
    }

    public static ExerciseInputEvent? TryCreateBeat(
        MidiInputMessage message,
        bool isActive,
        DateTime timelineStartedAt,
        DateTime occurredAt)
    {
        if (!isActive || !IsNoteOn(message) || occurredAt < timelineStartedAt)
        {
            return null;
        }

        var offset = occurredAt - timelineStartedAt;
        return new ExerciseInputEvent
        {
            Timestamp = occurredAt,
            Offset = offset,
            Source = InputSource.Midi,
            Type = InputEventType.Beat,
            Value = "beat",
            Velocity = message.Data2
        };
    }

    public static bool IsNoteOn(MidiInputMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var command = message.Status & 0xF0;
        return command == NoteOnCommand &&
            message.Data1 is >= 0 and <= 127 &&
            message.Data2 is > 0 and <= 127;
    }

    public static bool IsNoteOff(MidiInputMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var command = message.Status & 0xF0;
        return message.Data1 is >= 0 and <= 127 &&
            message.Data2 is >= 0 and <= 127 &&
            (command == NoteOffCommand || (command == NoteOnCommand && message.Data2 == 0));
    }
}
