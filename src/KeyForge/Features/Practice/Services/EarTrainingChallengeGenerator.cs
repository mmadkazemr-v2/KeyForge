namespace KeyForge.Features.Practice.Services;

/// <summary>Generates deterministic pitch-identification questions from ear-training keys.</summary>
public sealed class EarTrainingChallengeGenerator
{
    private static readonly int[] MajorScaleOffsets = [0, 2, 4, 5, 7, 9, 11];

    public ExerciseChallenge Create(EarTrainingExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        if (exercise.TaskType != EarTrainingTask.PitchMatching)
        {
            return new ExerciseChallenge
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

        if (exercise.Rounds <= 0)
        {
            throw new InvalidOperationException("Ear-training rounds must be greater than zero.");
        }

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
}
