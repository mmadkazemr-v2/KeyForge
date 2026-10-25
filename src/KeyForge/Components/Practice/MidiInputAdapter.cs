namespace KeyForge.Components.Practice;

/// <summary>Normalizes browser MIDI messages into presentation and runtime input events.</summary>
public static class MidiInputAdapter
{
    private const int NoteOnCommand = 0x90;
    private const int NoteOffCommand = 0x80;
    private const int ControlChangeCommand = 0xB0;
    private const int PitchBendCommand = 0xE0;
    private const int SustainControlNumber = 64;

    public static ExerciseInputEvent? TryCreate(
        MidiInputMessage message,
        DateTime receivedAt,
        double browserTimelineOriginMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!TryGetKind(message, out var kind) ||
            !double.IsFinite(message.TimestampMilliseconds) ||
            !double.IsFinite(browserTimelineOriginMilliseconds) ||
            message.TimestampMilliseconds < browserTimelineOriginMilliseconds ||
            receivedAt.Kind != DateTimeKind.Utc)
        {
            return null;
        }

        var offsetMilliseconds = message.TimestampMilliseconds - browserTimelineOriginMilliseconds;
        if (!double.IsFinite(offsetMilliseconds) || offsetMilliseconds > TimeSpan.MaxValue.TotalMilliseconds)
        {
            return null;
        }

        var isNote = kind is MidiEventKind.NoteOn or MidiEventKind.NoteOff;
        return new ExerciseInputEvent
        {
            Timestamp = receivedAt,
            Offset = TimeSpan.FromMilliseconds(offsetMilliseconds),
            Source = InputSource.Midi,
            Type = kind switch
            {
                MidiEventKind.NoteOn => InputEventType.Note,
                MidiEventKind.NoteOff => InputEventType.NoteOff,
                MidiEventKind.ControlChange => InputEventType.ControlChange,
                MidiEventKind.PitchBend => InputEventType.PitchBend,
                _ => throw new InvalidOperationException("Unsupported MIDI event kind.")
            },
            Value = isNote ? MusicalNote.FromMidiNumber(message.Data1).Value : $"{message.Data1}:{message.Data2}",
            Velocity = kind == MidiEventKind.NoteOn ? message.Data2 : null,
            MidiKind = kind,
            DeviceId = message.DeviceId,
            DeviceName = message.DeviceName,
            MidiChannel = message.Channel,
            MidiData1 = message.Data1,
            MidiData2 = message.Data2,
            BrowserTimestampMilliseconds = message.TimestampMilliseconds,
            Sequence = message.Sequence
        };
    }

    public static ExerciseInputEvent? TryCreateBeat(
        MidiInputMessage message,
        bool isActive,
        DateTime receivedAt,
        double browserTimelineOriginMilliseconds)
    {
        if (!isActive || !IsNoteOn(message)) return null;
        var input = TryCreate(message, receivedAt, browserTimelineOriginMilliseconds);
        return input is null ? null : new ExerciseInputEvent
        {
            Timestamp = input.Timestamp,
            Offset = input.Offset,
            Source = input.Source,
            Type = InputEventType.Beat,
            Value = "beat",
            Velocity = input.Velocity,
            MidiKind = input.MidiKind,
            DeviceId = input.DeviceId,
            DeviceName = input.DeviceName,
            MidiChannel = input.MidiChannel,
            MidiData1 = input.MidiData1,
            MidiData2 = input.MidiData2,
            BrowserTimestampMilliseconds = input.BrowserTimestampMilliseconds,
            Sequence = input.Sequence
        };
    }

    public static bool IsNoteOn(MidiInputMessage message) =>
        TryGetKind(message, out var kind) && kind == MidiEventKind.NoteOn;

    public static bool IsNoteOff(MidiInputMessage message) =>
        TryGetKind(message, out var kind) && kind == MidiEventKind.NoteOff;

    public static MidiPresentationEvent? TryCreatePresentationEvent(MidiInputMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (IsNoteOn(message))
        {
            return new MidiPresentationEvent
            {
                Type = MidiPresentationEventType.NoteOn,
                Note = MusicalNote.FromMidiNumber(message.Data1),
                Velocity = message.Data2
            };
        }

        if (IsNoteOff(message))
        {
            return new MidiPresentationEvent
            {
                Type = MidiPresentationEventType.NoteOff,
                Note = MusicalNote.FromMidiNumber(message.Data1)
            };
        }

        if (TryGetKind(message, out var kind) && kind == MidiEventKind.ControlChange &&
            message.Data1 == SustainControlNumber)
        {
            return new MidiPresentationEvent
            {
                Type = MidiPresentationEventType.SustainChanged,
                IsSustainDown = message.Data2 >= 64
            };
        }

        return null;
    }

    private static bool TryGetKind(MidiInputMessage message, out MidiEventKind kind)
    {
        ArgumentNullException.ThrowIfNull(message);
        kind = default;
        if (message.Status is < 0x80 or > 0xEF || message.Data1 is < 0 or > 127 || message.Data2 is < 0 or > 127)
        {
            return false;
        }

        var command = message.Status & 0xF0;
        if (command == NoteOnCommand)
        {
            kind = message.Data2 == 0 ? MidiEventKind.NoteOff : MidiEventKind.NoteOn;
            return true;
        }
        if (command == NoteOffCommand)
        {
            kind = MidiEventKind.NoteOff;
            return true;
        }
        if (command == ControlChangeCommand)
        {
            kind = MidiEventKind.ControlChange;
            return true;
        }
        if (command == PitchBendCommand)
        {
            kind = MidiEventKind.PitchBend;
            return true;
        }
        return false;
    }
}
