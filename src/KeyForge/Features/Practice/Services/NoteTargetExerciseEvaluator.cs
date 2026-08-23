namespace KeyForge.Features.Practice.Services;

/// <summary>Shared exact-note evaluation for interval and octave runtime challenges.</summary>
public sealed class NoteTargetExerciseEvaluator
{
    public const int SuccessScore = NoteReadingExerciseEvaluator.SuccessScore;

    public ExerciseEvaluationResult Evaluate(
        ExerciseChallenge challenge,
        IReadOnlyList<ExerciseInputEvent> inputEvents,
        ExerciseType exerciseType)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        ArgumentNullException.ThrowIfNull(inputEvents);

        var questions = challenge.ExpectedInputs
            .Select((expected, index) => CreateQuestionEvaluation(index, expected,
                index < inputEvents.Count ? inputEvents[index] : null))
            .ToArray();
        if (questions.Length == 0)
        {
            return new ExerciseEvaluationResult
            {
                IsSuccessful = false,
                Score = 0,
                Feedback = ExerciseEvaluationFeedback.UnsupportedChallenge
            };
        }

        var details = new NoteTargetEvaluationDetails { Questions = questions };
        var score = (int)Math.Round(details.CorrectAnswerCount * 100d / details.QuestionCount,
            MidpointRounding.AwayFromZero);

        return new ExerciseEvaluationResult
        {
            IsSuccessful = score >= SuccessScore,
            Score = score,
            Feedback = inputEvents.Count == 0 ? ExerciseEvaluationFeedback.NoInput :
                score >= SuccessScore ? ExerciseEvaluationFeedback.Correct : ExerciseEvaluationFeedback.Incorrect,
            ExpectedValue = challenge.ExpectedInputs[0].Value,
            ActualValue = inputEvents.FirstOrDefault()?.Value,
            Interval = exerciseType == ExerciseType.Interval ? details : null,
            Octave = exerciseType == ExerciseType.Octave ? details : null,
            MentalKeyboard = exerciseType == ExerciseType.MentalKeyboard ? details : null
        };
    }

    public static bool IsCorrect(ExpectedInput expected, ExerciseInputEvent actual) =>
        NoteReadingExerciseEvaluator.IsCorrect(expected, actual);

    private static NoteTargetQuestionEvaluation CreateQuestionEvaluation(
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
