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

    private static int GetSemitones(string interval) => interval.Trim().ToUpperInvariant() switch
    {
        "P1" => 0,
        "M2" => 2,
        "M3" => 4,
        "P4" => 5,
        "P5" => 7,
        "M6" => 9,
        "M7" => 11,
        "P8" => 12,
        _ => throw new InvalidOperationException($"Interval '{interval}' is not supported by the interactive runtime.")
    };

    private sealed record GeneratedIntervalQuestion(
        string Interval,
        ExerciseDirection Direction,
        MusicalNote Target);
}
