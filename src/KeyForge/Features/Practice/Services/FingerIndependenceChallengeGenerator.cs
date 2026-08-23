namespace KeyForge.Features.Practice.Services;

/// <summary>Creates a visible note sequence from the configured finger numbers.</summary>
public sealed class FingerIndependenceChallengeGenerator
{
    private static readonly string[] FingerNotes = ["C4", "D4", "E4", "F4", "G4"];

    public ExerciseChallenge Create(FingerIndependenceExerciseDefinition exercise, int? selectedBpm)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        var fingers = exercise.Fingers.Count == 0 ? new[] { 1, 2, 3, 4, 5 } : exercise.Fingers.ToArray();
        if (fingers.Any(finger => finger is < 1 or > 5))
        {
            throw new InvalidOperationException("Finger-independence exercises support finger numbers 1 through 5.");
        }

        var notes = fingers.Select(finger => FingerNotes[finger - 1]).ToArray();
        var bpm = selectedBpm ?? exercise.Tempo?.DefaultBpm;
        TimeSpan? interval = bpm.HasValue
            ? RhythmChallengeGenerator.CalculateBeatInterval(bpm.Value)
            : null;

        return new ExerciseChallenge
        {
            ExerciseId = exercise.Id,
            ExerciseType = ExerciseType.FingerIndependence,
            ExpectedInputs = notes.Select((note, index) => new ExpectedInput
            {
                Type = InputEventType.Note,
                Value = note,
                ExpectedOffset = interval.HasValue ? TimeSpan.FromTicks(interval.Value.Ticks * index) : null
            }).ToArray(),
            FingerIndependence = new FingerIndependenceChallengeDetails
            {
                Hand = exercise.Hand,
                Fingers = fingers,
                ExpectedNotes = notes
            }
        };
    }
}
