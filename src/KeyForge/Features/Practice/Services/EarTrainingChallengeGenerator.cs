namespace KeyForge.Features.Practice.Services;

/// <summary>Generates deterministic pitch-identification and interval-recognition questions from ear-training keys.</summary>
public sealed class EarTrainingChallengeGenerator
{
    /// <summary>Intervals used when an interval-recognition exercise does not configure any.</summary>
    public static readonly IReadOnlyList<string> DefaultIntervals =
        ["P1", "m2", "M2", "m3", "M3", "P4", "P5", "m6", "M6", "m7", "M7", "P8"];

    private static readonly int[] MajorScaleOffsets = [0, 2, 4, 5, 7, 9, 11];

    public ExerciseChallenge Create(EarTrainingExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        if (exercise.Rounds <= 0)
        {
            throw new InvalidOperationException("Ear-training rounds must be greater than zero.");
        }

        return exercise.TaskType switch
        {
            EarTrainingTask.PitchMatching => CreatePitchMatching(exercise),
            EarTrainingTask.IntervalRecognition => CreateIntervalRecognition(exercise),
            _ => CreateUnsupported(exercise)
        };
    }

    /// <summary>Returns the configured major-scale notes in octave four, or the unconstrained chromatic octave.</summary>
    public static IReadOnlyList<MusicalNote> GetAvailableNotes(EarTrainingExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        if (exercise.Keys.Count == 0)
        {
            return Enumerable.Range(MusicalNote.Parse("C4").MidiNumber, 12)
                .Select(MusicalNote.FromMidiNumber)
                .ToArray();
        }

        var notes = new List<MusicalNote>();
        foreach (var key in exercise.Keys)
        {
            var root = key.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (root is null || !MusicalNote.TryParse($"{root}4", out var rootNote))
            {
                throw new InvalidOperationException($"Ear-training key '{key}' has an unsupported root note.");
            }

            notes.AddRange(MajorScaleOffsets.Select(offset =>
                MusicalNote.FromMidiNumber(rootNote.MidiNumber + offset)));
        }

        return notes.Distinct().OrderBy(note => note.MidiNumber).ToArray();
    }

    private ExerciseChallenge CreatePitchMatching(EarTrainingExerciseDefinition exercise)
    {
        var notes = GetAvailableNotes(exercise);
        return new ExerciseChallenge
        {
            ExerciseId = exercise.Id,
            ExerciseType = exercise.Type,
            ExpectedInputs = Enumerable.Range(0, exercise.Rounds)
                .Select(index => new ExpectedInput
                {
                    Type = InputEventType.Note,
                    Value = notes[index % notes.Count].Value
                })
                .ToArray(),
            EarTraining = new EarTrainingChallengeDetails
            {
                TaskType = exercise.TaskType,
                AvailableNotes = notes.Select(note => note.Value).ToArray()
            }
        };
    }

    private ExerciseChallenge CreateIntervalRecognition(EarTrainingExerciseDefinition exercise)
    {
        var intervals = ResolveIntervals(exercise);
        var notes = GetAvailableNotes(exercise);
        var questions = Enumerable.Range(0, exercise.Rounds)
            .Select(index => CreateIntervalQuestion(intervals, notes, exercise.Direction, index))
            .ToArray();

        return new ExerciseChallenge
        {
            ExerciseId = exercise.Id,
            ExerciseType = exercise.Type,
            ExpectedInputs = questions.Select(question => new ExpectedInput
            {
                Type = InputEventType.Choice,
                Value = question.Interval
            }).ToArray(),
            EarTraining = new EarTrainingChallengeDetails
            {
                TaskType = exercise.TaskType,
                AvailableNotes = questions.SelectMany(question => new[] { question.SourceNote, question.TargetNote })
                    .Distinct()
                    .ToArray(),
                IntervalChoices = intervals,
                IntervalQuestions = questions
            }
        };
    }

    private static EarTrainingIntervalQuestion CreateIntervalQuestion(
        IReadOnlyList<string> intervals,
        IReadOnlyList<MusicalNote> sources,
        ExerciseDirection configuredDirection,
        int index)
    {
        var interval = intervals[index % intervals.Count];
        var direction = IntervalChallengeGenerator.ResolveDirection(configuredDirection, index);
        var source = sources[index % sources.Count];
        var target = IntervalChallengeGenerator.GetTargetNote(source, interval, direction);

        return new EarTrainingIntervalQuestion
        {
            SourceNote = source.Value,
            TargetNote = target.Value,
            Interval = interval,
            Direction = direction
        };
    }

    private static IReadOnlyList<string> ResolveIntervals(EarTrainingExerciseDefinition exercise)
    {
        if (exercise.Intervals.Count == 0)
        {
            return DefaultIntervals;
        }

        var unsupported = exercise.Intervals.FirstOrDefault(interval =>
            !IntervalChallengeGenerator.IsSupportedInterval(interval));
        if (unsupported is not null)
        {
            throw new InvalidOperationException(
                $"Interval '{unsupported}' is not supported by the interactive runtime.");
        }

        return exercise.Intervals.Distinct().ToArray();
    }

    private ExerciseChallenge CreateUnsupported(EarTrainingExerciseDefinition exercise) => new()
    {
        ExerciseId = exercise.Id,
        ExerciseType = exercise.Type,
        ExpectedInputs = [],
        EarTraining = new EarTrainingChallengeDetails
        {
            TaskType = exercise.TaskType,
            AvailableNotes = []
        }
    };
}
