namespace KeyForge.Features.Practice.Services;

/// <summary>Creates the configured repeated note sequence for a speed drill.</summary>
public sealed class SpeedChallengeGenerator
{
    public ExerciseChallenge Create(SpeedExerciseDefinition exercise, int? selectedBpm)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        if (exercise.Repetitions <= 0)
        {
            throw new InvalidOperationException("Speed exercise repetitions must be greater than zero.");
        }

        var bpm = selectedBpm ?? exercise.Tempo?.DefaultBpm ?? throw new InvalidOperationException(
            "Speed exercises require a runtime tempo.");
        var pattern = GetPatternNotes(exercise.Pattern);
        var notes = Enumerable.Range(0, exercise.Repetitions)
            .SelectMany(_ => pattern)
            .ToArray();
        var interval = RhythmChallengeGenerator.CalculateBeatInterval(bpm);

        return new ExerciseChallenge
        {
            ExerciseId = exercise.Id,
            ExerciseType = ExerciseType.Speed,
            ExpectedInputs = notes.Select((note, index) => new ExpectedInput
            {
                Type = InputEventType.Note,
                Value = note,
                ExpectedOffset = TimeSpan.FromTicks(interval.Ticks * index)
            }).ToArray(),
            Speed = new SpeedChallengeDetails { Pattern = exercise.Pattern, ExpectedNotes = notes }
        };
    }

    public static IReadOnlyList<string> GetPatternNotes(SpeedPattern pattern) => pattern switch
    {
        SpeedPattern.Scale => ["C4", "D4", "E4", "F4", "G4"],
        SpeedPattern.Arpeggio => ["C4", "E4", "G4", "C5"],
        SpeedPattern.RepeatedNote => ["C4"],
        SpeedPattern.Chromatic => ["C4", "C#4", "D4", "D#4", "E4"],
        _ => throw new ArgumentOutOfRangeException(nameof(pattern))
    };
}
