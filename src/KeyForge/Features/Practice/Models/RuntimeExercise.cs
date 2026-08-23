namespace KeyForge.Features.Practice.Models;

/// <summary>Holds the generated challenge and normalized inputs for the active exercise.</summary>
public sealed class RuntimeExercise
{
    private readonly List<ExerciseInputEvent> _inputEvents = [];

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
