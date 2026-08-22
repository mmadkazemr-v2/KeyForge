namespace KeyForge.Features.Practice.Services;

/// <summary>Evaluates note-reading answers without depending on their input source.</summary>
public sealed class NoteReadingExerciseEvaluator
{
    public const int SuccessScore = 60;

    public ExerciseEvaluationResult Evaluate(
        ExerciseChallenge challenge,
        IReadOnlyList<ExerciseInputEvent> inputEvents)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        ArgumentNullException.ThrowIfNull(inputEvents);

        if (challenge.ExpectedInputs.Count == 0)
        {
            return new ExerciseEvaluationResult
            {
                IsSuccessful = false,
                Score = 0,
                Feedback = ExerciseEvaluationFeedback.UnsupportedChallenge
            };
        }

        var questions = challenge.ExpectedInputs
            .Select((expected, index) => CreateQuestionEvaluation(
                index,
                expected,
                index < inputEvents.Count ? inputEvents[index] : null))
            .ToArray();
        var details = new NoteReadingEvaluationDetails { Questions = questions };
        var score = (int)Math.Round(
            details.CorrectAnswerCount * 100d / details.QuestionCount,
            MidpointRounding.AwayFromZero);

        return new ExerciseEvaluationResult
        {
            IsSuccessful = score >= SuccessScore,
            Score = score,
            Feedback = inputEvents.Count == 0
                ? ExerciseEvaluationFeedback.NoInput
                : score >= SuccessScore
                    ? ExerciseEvaluationFeedback.Correct
                    : ExerciseEvaluationFeedback.Incorrect,
            ExpectedValue = challenge.ExpectedInputs[0].Value,
            ActualValue = inputEvents.FirstOrDefault()?.Value,
            NoteReading = details
        };
    }

    public static bool IsCorrect(ExpectedInput expected, ExerciseInputEvent actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        return expected.Type == InputEventType.Note &&
            actual.Type == InputEventType.Note &&
            MusicalNote.TryParse(expected.Value, out var expectedNote) &&
            MusicalNote.TryParse(actual.Value, out var actualNote) &&
            expectedNote == actualNote;
    }

    private static NoteReadingQuestionEvaluation CreateQuestionEvaluation(
        int index,
        ExpectedInput expected,
        ExerciseInputEvent? actual) => new()
        {
            QuestionNumber = index + 1,
            ExpectedNote = expected.Value,
            ActualNote = actual?.Value,
            IsCorrect = actual is not null && IsCorrect(expected, actual)
        };
}
