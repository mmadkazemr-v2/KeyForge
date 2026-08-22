namespace KeyForge.Features.Practice.Services;

/// <summary>Creates deterministic, octave-aware target-note questions.</summary>
public sealed class OctaveChallengeGenerator
{
    public ExerciseChallenge Create(OctaveExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        var source = ParseNaturalStartingNote(exercise.Id, exercise.StartingNote);
        if (exercise.Octaves <= 0)
        {
            throw new InvalidOperationException($"Octave exercise '{exercise.Id}' must span at least one octave.");
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
            Octave = new OctaveChallengeDetails
            {
                Questions = questions.Select(question => new OctaveQuestion
                {
                    SourceNote = source.Value,
                    Octaves = exercise.Octaves,
                    Direction = question.Direction
                }).ToArray()
            }
        };
    }

    public static MusicalNote GetTargetNote(MusicalNote source, int octaves, ExerciseDirection direction)
    {
        ArgumentNullException.ThrowIfNull(source);
        var targetMidi = source.MidiNumber + (direction == ExerciseDirection.Descending ? -1 : 1) * octaves * 12;
        if (targetMidi < 0 || targetMidi > 127)
        {
            throw new InvalidOperationException("The configured octave span is outside the supported piano range.");
        }

        return MusicalNote.FromMidiNumber(targetMidi);
    }

    private static GeneratedOctaveQuestion CreateQuestion(
        OctaveExerciseDefinition exercise,
        MusicalNote source,
        int index)
    {
        var direction = exercise.Direction == ExerciseDirection.Both
            ? index % 2 == 0 ? ExerciseDirection.Ascending : ExerciseDirection.Descending
            : exercise.Direction;
        return new GeneratedOctaveQuestion(direction, GetTargetNote(source, exercise.Octaves, direction));
    }

    private static MusicalNote ParseNaturalStartingNote(string exerciseId, string value)
    {
        if (!MusicalNote.TryParse(value, out var note) || note.IsBlackKey)
        {
            throw new InvalidOperationException(
                $"Octave exercise '{exerciseId}' must define a natural starting note for interactive practice.");
        }

        return note;
    }

    private sealed record GeneratedOctaveQuestion(ExerciseDirection Direction, MusicalNote Target);
}
