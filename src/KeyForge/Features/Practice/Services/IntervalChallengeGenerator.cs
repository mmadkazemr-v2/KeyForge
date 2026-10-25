namespace KeyForge.Features.Practice.Services;

/// <summary>Creates deterministic, playable interval questions from one definition.</summary>
public sealed class IntervalChallengeGenerator
{
    public ExerciseChallenge Create(IntervalExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        var source = ParseNaturalStartingNote(exercise.Id, exercise.StartingNote);
        if (exercise.Intervals.Count == 0)
        {
            throw new InvalidOperationException($"Interval exercise '{exercise.Id}' does not define any intervals.");
        }

        var questions = Enumerable.Range(0, NoteReadingChallengeGenerator.DefaultQuestionCount)
            .Select(index => CreateQuestion(exercise, source, index))
            .ToArray();

        return new ExerciseChallenge
        {
            ExerciseId = exercise.Id,
            ExerciseType = exercise.Type,
            ExpectedInputs = questions.Select(question => new ExpectedInput
            {
                Type = InputEventType.Note,
                Value = question.Target.Value
            }).ToArray(),
            Interval = new IntervalChallengeDetails
            {
                Questions = questions.Select(question => new IntervalQuestion
                {
                    SourceNote = source.Value,
                    Interval = question.Interval,
                    Direction = question.Direction
                }).ToArray()
            }
        };
    }

    public static MusicalNote GetTargetNote(MusicalNote source, string interval, ExerciseDirection direction)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(interval);

        var semitones = GetSemitones(interval);
        var directionMultiplier = direction == ExerciseDirection.Descending ? -1 : 1;
        var targetMidi = source.MidiNumber + directionMultiplier * semitones;
        if (targetMidi < 0 || targetMidi > 127)
        {
            throw new InvalidOperationException("The configured interval is outside the supported piano range.");
        }

        // The source note is the only note rendered on the current staff. The
        // virtual piano and note evaluator already support chromatic targets.
        return MusicalNote.FromMidiNumber(targetMidi);
    }

    private static GeneratedIntervalQuestion CreateQuestion(
        IntervalExerciseDefinition exercise,
        MusicalNote source,
        int index)
    {
        var interval = exercise.Intervals[index % exercise.Intervals.Count];
        var direction = SelectDirection(exercise.Direction, index);
        return new GeneratedIntervalQuestion(interval, direction, GetTargetNote(source, interval, direction));
    }

    private static MusicalNote ParseNaturalStartingNote(string exerciseId, string value)
    {
        if (!MusicalNote.TryParse(value, out var note) || note.IsBlackKey)
        {
            throw new InvalidOperationException(
                $"Interval exercise '{exerciseId}' must define a natural starting note for interactive practice.");
        }

        return note;
    }

    private static ExerciseDirection SelectDirection(ExerciseDirection configuredDirection, int index) =>
        configuredDirection == ExerciseDirection.Both
            ? index % 2 == 0 ? ExerciseDirection.Ascending : ExerciseDirection.Descending
            : configuredDirection;

    /// <summary>Resolves the per-question direction, alternating when the exercise allows both.</summary>
    public static ExerciseDirection ResolveDirection(ExerciseDirection configuredDirection, int index) =>
        SelectDirection(configuredDirection, index);

    /// <summary>Returns whether the interval abbreviation is supported by the interactive runtimes.</summary>
    public static bool IsSupportedInterval(string? interval) => TryGetSemitones(interval, out _);

    private static int GetSemitones(string interval) =>
        TryGetSemitones(interval, out var semitones)
            ? semitones
            : throw new InvalidOperationException($"Interval '{interval}' is not supported by the interactive runtime.");

    private static bool TryGetSemitones(string? interval, out int semitones)
    {
        // Interval abbreviations are case-sensitive: lowercase means minor,
        // uppercase means major or perfect (m3 vs M3).
        semitones = interval?.Trim() switch
        {
            "P1" => 0,
            "m2" => 1,
            "M2" => 2,
            "m3" => 3,
            "M3" => 4,
            "P4" => 5,
            "P5" => 7,
            "m6" => 8,
            "M6" => 9,
            "m7" => 10,
            "M7" => 11,
            "P8" => 12,
            _ => -1
        };
        return semitones >= 0;
    }

    private sealed record GeneratedIntervalQuestion(
        string Interval,
        ExerciseDirection Direction,
        MusicalNote Target);
}
