namespace KeyForge.Features.Practice.Services;

/// <summary>Evaluates ear-training answers independently of input source.</summary>
public sealed class EarTrainingExerciseEvaluator
{
    public const int SuccessScore = NoteReadingExerciseEvaluator.SuccessScore;

    public ExerciseEvaluationResult Evaluate(
        ExerciseChallenge challenge,
        IReadOnlyList<ExerciseInputEvent> inputEvents)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        ArgumentNullException.ThrowIfNull(inputEvents);

        var taskType = challenge.EarTraining?.TaskType;
        var isIntervalTask = taskType == EarTrainingTask.IntervalRecognition;
        if ((taskType != EarTrainingTask.PitchMatching && !isIntervalTask) ||
            challenge.ExpectedInputs.Count == 0)
        {
            return new ExerciseEvaluationResult
            {
                IsSuccessful = false,
                Score = 0,
                Feedback = ExerciseEvaluationFeedback.UnsupportedChallenge
            };
        }

        var questions = challenge.ExpectedInputs.Select((expected, index) =>
        {
            var actual = index < inputEvents.Count ? inputEvents[index] : null;
            return new NoteTargetQuestionEvaluation
            {
                QuestionNumber = index + 1,
                ExpectedNote = expected.Value,
                ActualNote = actual?.Value,
                IsCorrect = actual is not null &&
                    (isIntervalTask ? IsCorrectInterval(expected, actual) : IsCorrect(expected, actual))
            };
        }).ToArray();
        var details = new EarTrainingEvaluationDetails { Questions = questions };
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
            EarTraining = details
        };
    }

    public static bool IsCorrect(ExpectedInput expected, ExerciseInputEvent actual) =>
        NoteReadingExerciseEvaluator.IsCorrect(expected, actual);

    /// <summary>Interval answers are compared as exact choice values; minor and major differ only by case.</summary>
    public static bool IsCorrectInterval(ExpectedInput expected, ExerciseInputEvent actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        return expected.Type == InputEventType.Choice &&
            actual.Type == InputEventType.Choice &&
            string.Equals(expected.Value.Trim(), actual.Value.Trim(), StringComparison.Ordinal);
    }
}
