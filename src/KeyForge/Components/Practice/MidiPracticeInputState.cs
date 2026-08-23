namespace KeyForge.Components.Practice;

/// <summary>Scoped presentation state for the current MIDI device input.</summary>
public sealed class MidiPracticeInputState
{
    private readonly HashSet<MusicalNote> _pressedNotes = [];
    private readonly HashSet<MusicalNote> _sustainedNotes = [];

    public event Action? Changed;

    public IReadOnlySet<MusicalNote> ActiveNotes => _pressedNotes.Concat(_sustainedNotes).ToHashSet();
    public MusicalNote? LastNote { get; private set; }
    public int? LastVelocity { get; private set; }
    public bool IsSustainDown { get; private set; }

    public void Apply(MidiPresentationEvent midiEvent)
    {
        ArgumentNullException.ThrowIfNull(midiEvent);

        switch (midiEvent.Type)
        {
            case MidiPresentationEventType.NoteOn when midiEvent.Note is not null:
                _pressedNotes.Add(midiEvent.Note);
                _sustainedNotes.Remove(midiEvent.Note);
                LastNote = midiEvent.Note;
                LastVelocity = midiEvent.Velocity;
                break;
            case MidiPresentationEventType.NoteOff when midiEvent.Note is not null:
                _pressedNotes.Remove(midiEvent.Note);
                if (IsSustainDown)
                {
                    _sustainedNotes.Add(midiEvent.Note);
                }
                else
                {
                    _sustainedNotes.Remove(midiEvent.Note);
                }
                break;
            case MidiPresentationEventType.SustainChanged when midiEvent.IsSustainDown is { } isDown:
                IsSustainDown = isDown;
                if (!isDown)
                {
                    _sustainedNotes.Clear();
                }
                break;
            default:
                return;
        }

        Changed?.Invoke();
    }

    public void Reset()
    {
        if (_pressedNotes.Count == 0 && _sustainedNotes.Count == 0 &&
            LastNote is null && LastVelocity is null && !IsSustainDown)
        {
            return;
        }

        _pressedNotes.Clear();
        _sustainedNotes.Clear();
        LastNote = null;
        LastVelocity = null;
        IsSustainDown = false;
        Changed?.Invoke();
    }

    public static MidiVelocityLevel GetVelocityLevel(int velocity) => velocity switch
    {
        <= 42 => MidiVelocityLevel.Soft,
        <= 84 => MidiVelocityLevel.Medium,
        _ => MidiVelocityLevel.Strong
    };
}

public enum MidiVelocityLevel
{
    Soft,
    Medium,
    Strong
}
