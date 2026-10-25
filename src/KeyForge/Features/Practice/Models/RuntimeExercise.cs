namespace KeyForge.Features.Practice.Models;

/// <summary>Holds the generated challenge and normalized inputs for the active exercise.</summary>
public sealed class RuntimeExercise
{
    private readonly List<ExerciseInputEvent> _inputEvents = [];
    private readonly List<ExerciseInputEvent> _midiEvents = [];

    private readonly Func<int?, ExerciseChallenge>? _challengeFactory;

    public RuntimeExercise(
        ExerciseDefinition definition,
        ExerciseChallenge challenge,
        DateTime startedAt,
        int? selectedBpm = null,
        Func<int?, ExerciseChallenge>? challengeFactory = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(challenge);

        if (!string.Equals(definition.Id, challenge.ExerciseId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The challenge must belong to the runtime exercise definition.",
                nameof(challenge));
        }

        Definition = definition;
        Challenge = challenge;
        StartedAt = startedAt;
        SelectedBpm = selectedBpm;
        _challengeFactory = challengeFactory;
    }

    public ExerciseDefinition Definition { get; }
    public ExerciseChallenge Challenge { get; private set; }
    public DateTime StartedAt { get; }
    public int? SelectedBpm { get; private set; }
    public IReadOnlyList<ExerciseInputEvent> InputEvents => _inputEvents.AsReadOnly();
    /// <summary>All normalized MIDI messages received during this runtime, including non-scored NoteOff and controller events.</summary>
    public IReadOnlyList<ExerciseInputEvent> MidiEvents => _midiEvents.OrderBy(input => input.Sequence ?? long.MaxValue).ToArray();

    public void CaptureMidiEvent(ExerciseInputEvent inputEvent)
    {
        ArgumentNullException.ThrowIfNull(inputEvent);
        if (inputEvent.Source != InputSource.Midi || inputEvent.MidiKind is null)
        {
            throw new ArgumentException("Only normalized MIDI events can be recorded in the MIDI event stream.", nameof(inputEvent));
        }
        if (inputEvent.Offset < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(inputEvent), "Input offsets cannot be negative.");
        }
        if (inputEvent.Timestamp < StartedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(inputEvent), "Input events cannot occur before the runtime exercise starts.");
        }
        _midiEvents.Add(inputEvent);
    }

    /// <summary>Clears captured input when an interrupted live performance is explicitly restarted.</summary>
    public void ResetCapturedInputs()
    {
        _inputEvents.Clear();
        _midiEvents.Clear();
    }

    public void CaptureInput(ExerciseInputEvent inputEvent)
    {
        ArgumentNullException.ThrowIfNull(inputEvent);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputEvent.Value);

        if (inputEvent.Offset < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(inputEvent),
                "Input offsets cannot be negative.");
        }

        if (inputEvent.Timestamp < StartedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(inputEvent),
                "Input events cannot occur before the runtime exercise starts.");
        }

        _inputEvents.Add(inputEvent);
    }

    /// <summary>Changes the runtime-only tempo before input is captured.</summary>
    public void SetSelectedBpm(int bpm)
    {
        var tempo = Definition.Tempo ?? throw new InvalidOperationException(
            "This exercise does not define tempo settings.");
        if (!tempo.AllowUserChange || !tempo.Supports(bpm))
        {
            throw new InvalidOperationException("The selected BPM is not allowed by this exercise.");
        }
        if (_inputEvents.Count > 0)
        {
            throw new InvalidOperationException("Tempo cannot change after exercise input has been captured.");
        }

        SelectedBpm = bpm;
        if (_challengeFactory is not null)
        {
            Challenge = _challengeFactory(bpm);
        }
    }
}
