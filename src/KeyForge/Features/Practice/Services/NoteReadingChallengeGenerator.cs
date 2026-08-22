namespace KeyForge.Features.Practice.Services;

/// <summary>Creates a small deterministic set of notes from a configured reading range.</summary>
public sealed class NoteReadingChallengeGenerator
{
    /// <summary>
    /// The definition currently has no question-count property, so the first
    /// interactive runtime uses five questions per attempt.
    /// </summary>
    public const int DefaultQuestionCount = 5;

    public ExerciseChallenge Create(NoteReadingExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        var notes = ParseRange(exercise);
        var expectedInputs = Enumerable.Range(0, DefaultQuestionCount)
            .Select(index => new ExpectedInput
            {
                Type = InputEventType.Note,
                Value = SelectNote(notes, index).Value
            })
            .ToArray();

        return new ExerciseChallenge
        {
            ExerciseId = exercise.Id,
            ExerciseType = exercise.Type,
            ExpectedInputs = expectedInputs
        };
    }

    public static IReadOnlyList<MusicalNote> ParseRange(NoteReadingExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        var parts = exercise.Range.Split(
            '-',
            2,
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !MusicalNote.TryParse(parts[0], out var start) ||
            !MusicalNote.TryParse(parts[1], out var end) ||
            start.IsBlackKey ||
            end.IsBlackKey ||
            start.DiatonicIndex > end.DiatonicIndex)
        {
            throw new InvalidOperationException(
                $"Note-reading exercise '{exercise.Id}' does not define a valid natural-note range.");
        }

        return Enumerable.Range(start.DiatonicIndex, end.DiatonicIndex - start.DiatonicIndex + 1)
            .Select(MusicalNote.FromDiatonicIndex)
            .ToArray();
    }

    private static MusicalNote SelectNote(IReadOnlyList<MusicalNote> notes, int questionIndex)
    {
        if (notes.Count >= DefaultQuestionCount)
        {
            var noteIndex = (int)Math.Round(
                questionIndex * (notes.Count - 1d) / (DefaultQuestionCount - 1d),
                MidpointRounding.AwayFromZero);
            return notes[noteIndex];
        }

        return notes[questionIndex % notes.Count];
    }
}
