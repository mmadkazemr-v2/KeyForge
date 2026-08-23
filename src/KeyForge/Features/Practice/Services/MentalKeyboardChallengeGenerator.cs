namespace KeyForge.Features.Practice.Services;

/// <summary>Creates deterministic chromatic keyboard-navigation questions.</summary>
public sealed class MentalKeyboardChallengeGenerator
{
    public ExerciseChallenge Create(MentalKeyboardExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        if (!MusicalNote.TryParse(exercise.StartingNote, out var startingNote))
        {
            throw new InvalidOperationException(
                $"Mental keyboard exercise '{exercise.Id}' must define a valid starting note.");
        }

        if (exercise.Steps <= 0)
        {
            throw new InvalidOperationException(
                $"Mental keyboard exercise '{exercise.Id}' must define at least one step.");
        }

        var questions = Enumerable.Range(0, NoteReadingChallengeGenerator.DefaultQuestionCount)
            .Select(index => CreateQuestion(exercise, startingNote, index))
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
            MentalKeyboard = new MentalKeyboardChallengeDetails
            {
                Questions = questions.Select(question => new MentalKeyboardQuestion
                {
                    SourceNote = question.Source.Value,
                    Steps = exercise.Steps,
                    Direction = question.Direction
                }).ToArray()
            }
        };
    }

    public static MusicalNote GetTargetNote(MusicalNote source, int steps, ExerciseDirection direction)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (steps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(steps));
        }

        var targetMidi = source.MidiNumber + (direction == ExerciseDirection.Descending ? -steps : steps);
        if (targetMidi is < 0 or > 127)
        {
            throw new InvalidOperationException("The configured mental keyboard navigation is outside the supported piano range.");
        }

        return MusicalNote.FromMidiNumber(targetMidi);
    }

    private static GeneratedQuestion CreateQuestion(
        MentalKeyboardExerciseDefinition exercise,
        MusicalNote startingNote,
        int index)
    {
        var direction = exercise.Direction == ExerciseDirection.Both
            ? index % 2 == 0 ? ExerciseDirection.Ascending : ExerciseDirection.Descending
            : exercise.Direction;
        var sourceOffset = index * exercise.Steps;
        var sourceMidi = startingNote.MidiNumber +
            (direction == ExerciseDirection.Descending ? -sourceOffset : sourceOffset);
        if (sourceMidi is < 0 or > 127)
        {
            throw new InvalidOperationException("The generated mental keyboard source is outside the supported piano range.");
        }

        var source = MusicalNote.FromMidiNumber(sourceMidi);
        return new GeneratedQuestion(source, direction, GetTargetNote(source, exercise.Steps, direction));
    }

    private sealed record GeneratedQuestion(
        MusicalNote Source,
        ExerciseDirection Direction,
        MusicalNote Target);
}
