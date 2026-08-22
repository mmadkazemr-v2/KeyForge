namespace KeyForge.Features.Practice.Models;

/// <summary>Holds the generated challenge and normalized inputs for the active exercise.</summary>
public sealed class RuntimeExercise
{
    private readonly List<ExerciseInputEvent> _inputEvents = [];

    public RuntimeExercise(ExerciseDefinition definition, ExerciseChallenge challenge, DateTime startedAt)
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
    }

    public ExerciseDefinition Definition { get; }
    public ExerciseChallenge Challenge { get; }
    public DateTime StartedAt { get; }
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
}
