namespace KeyForge.Features.Practice.Services;

/// <summary>Creates a small deterministic set of notes from a configured reading range.</summary>
public sealed class NoteReadingChallengeGenerator
{
    /// <summary>
    /// Legacy question count used when a note-reading definition omits count.
    /// </summary>
    public const int DefaultQuestionCount = 5;

    private readonly Func<int, int> _nextRandomIndex;

    public NoteReadingChallengeGenerator(Func<int, int>? nextRandomIndex = null) =>
        _nextRandomIndex = nextRandomIndex ?? Random.Shared.Next;

    public ExerciseChallenge Create(NoteReadingExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        ValidateCount(exercise);
        var notes = ParseRange(exercise);
        var expectedInputs = Enumerable.Range(0, exercise.Count)
            .Select(index => new ExpectedInput
            {
                Type = InputEventType.Note,
                Value = SelectNote(exercise, notes, index).Value
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

        if (exercise.Notes is not null)
        {
            if (exercise.Notes.Count == 0)
            {
                throw new InvalidOperationException($"Note-reading exercise '{exercise.Id}' has an empty note pool.");
            }
            return exercise.Notes.Select(value => ParseNote(exercise.Id, value, "notes")).ToArray();
        }

        if (exercise.NoteRange is { } structuredRange)
        {
            var rangeStart = ParseNote(exercise.Id, structuredRange.From, "noteRange.from");
            var rangeEnd = ParseNote(exercise.Id, structuredRange.To, "noteRange.to");
            if (rangeStart.MidiNumber > rangeEnd.MidiNumber)
            {
                throw new InvalidOperationException($"Note-reading exercise '{exercise.Id}' has a reversed noteRange.");
            }
            return Enumerable.Range(rangeStart.MidiNumber, rangeEnd.MidiNumber - rangeStart.MidiNumber + 1)
                .Select(MusicalNote.FromMidiNumber).ToArray();
        }

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

    private MusicalNote SelectNote(NoteReadingExerciseDefinition exercise, IReadOnlyList<MusicalNote> notes, int questionIndex)
    {
        if (exercise.Randomized == true)
        {
            var randomIndex = _nextRandomIndex(notes.Count);
            if (randomIndex < 0 || randomIndex >= notes.Count)
            {
                throw new InvalidOperationException("The random note selector returned an out-of-range index.");
            }
            return notes[randomIndex];
        }

        if (exercise.Randomized == null && exercise.Notes is null && exercise.NoteRange is null)
        {
            var noteIndex = notes.Count >= exercise.Count
                ? (int)Math.Round(questionIndex * (notes.Count - 1d) / Math.Max(1, exercise.Count - 1), MidpointRounding.AwayFromZero)
                : questionIndex % notes.Count;
            return notes[noteIndex];
        }

        return notes[questionIndex % notes.Count];
    }

    private static void ValidateCount(NoteReadingExerciseDefinition exercise)
    {
        if (exercise.Count is < 1 or > 100)
        {
            throw new InvalidOperationException($"Note-reading exercise '{exercise.Id}' count must be between 1 and 100.");
        }
    }

    private static MusicalNote ParseNote(string exerciseId, string value, string field) =>
        MusicalNote.TryParse(value, out var note)
            ? note
            : throw new InvalidOperationException($"Note-reading exercise '{exerciseId}', field '{field}' has invalid note '{value}'.");
}
