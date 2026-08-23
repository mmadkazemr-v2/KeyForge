namespace KeyForge.Features.Practice.Services;

/// <summary>Evaluates pitch-identification answers independently of input source.</summary>
public sealed class EarTrainingExerciseEvaluator
{
    public const int SuccessScore = NoteReadingExerciseEvaluator.SuccessScore;

    public ExerciseEvaluationResult Evaluate(
        ExerciseChallenge challenge,
        IReadOnlyList<ExerciseInputEvent> inputEvents)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        ArgumentNullException.ThrowIfNull(inputEvents);

        if (challenge.EarTraining?.TaskType != EarTrainingTask.PitchMatching ||
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
                IsCorrect = actual is not null && IsCorrect(expected, actual)
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
}
