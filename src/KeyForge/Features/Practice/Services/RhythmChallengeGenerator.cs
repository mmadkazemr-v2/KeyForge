namespace KeyForge.Features.Practice.Services;

/// <summary>Generates pulse timing for the current rhythm model.</summary>
public sealed class RhythmChallengeGenerator
{
    private const double MillisecondsPerMinute = 60_000d;

    public ExerciseChallenge Create(RhythmExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        var beatInterval = CalculateBeatInterval(exercise.Tempo);
        var beatsPerPattern = GetBeatsPerPattern(exercise.TimeSignature);
        if (exercise.PatternCount <= 0)
        {
            throw new InvalidOperationException(
                $"Rhythm exercise '{exercise.Id}' must define at least one pattern.");
        }

        var beatCount = checked(beatsPerPattern * exercise.PatternCount);
        var expectedInputs = Enumerable.Range(0, beatCount)
            .Select(index => new ExpectedInput
            {
                Type = InputEventType.Beat,
                Value = "beat",
                ExpectedOffset = TimeSpan.FromTicks(beatInterval.Ticks * index)
            })
            .ToList();

        return new ExerciseChallenge
        {
            ExerciseId = exercise.Id,
            ExerciseType = ExerciseType.Rhythm,
            ExpectedInputs = expectedInputs
        };
    }

    public static TimeSpan CalculateBeatInterval(int beatsPerMinute)
    {
        if (beatsPerMinute <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(beatsPerMinute),
                "Beats per minute must be greater than zero.");
        }

        return TimeSpan.FromMilliseconds(MillisecondsPerMinute / beatsPerMinute);
    }

    public static int GetBeatsPerPattern(string timeSignature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeSignature);

        var parts = timeSignature.Split('/', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out var numerator) ||
            !int.TryParse(parts[1], out var denominator) ||
            numerator <= 0 || denominator <= 0)
        {
            throw new InvalidOperationException(
                $"Time signature '{timeSignature}' is not valid.");
        }

        return numerator;
    }
}
