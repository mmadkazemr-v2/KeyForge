namespace KeyForge.Features.Practice.Services;

/// <summary>Evaluates note order and optional beat timing for the two sequence drills.</summary>
public sealed class SequenceExerciseEvaluator
{
    private const int ExtraInputPenalty = 5;
    private const double OnTimeTendencyToleranceMilliseconds = 10;

    public ExerciseEvaluationResult Evaluate(
        ExerciseChallenge challenge,
        IReadOnlyList<ExerciseInputEvent> inputEvents,
        ExerciseType exerciseType,
        bool requireTiming)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        ArgumentNullException.ThrowIfNull(inputEvents);

        var expected = challenge.ExpectedInputs.Where(input => input.Type == InputEventType.Note).ToArray();
        var actual = inputEvents.Where(input => input.Type == InputEventType.Note).ToArray();
        if (expected.Length == 0)
        {
            return new ExerciseEvaluationResult { IsSuccessful = false, Score = 0, Feedback = ExerciseEvaluationFeedback.UnsupportedChallenge };
        }

        var correct = 0;
        var incorrect = 0;
        var timingErrors = new List<double>();
        for (var index = 0; index < Math.Min(expected.Length, actual.Length); index++)
        {
            var noteMatches = string.Equals(expected[index].Value, actual[index].Value, StringComparison.OrdinalIgnoreCase);
            var timingMatches = !requireTiming || IsWithinTimingTolerance(expected[index], actual[index], timingErrors);
            if (noteMatches && timingMatches)
            {
                correct++;
            }
            else
            {
                incorrect++;
            }
        }

        var missed = expected.Length - Math.Min(expected.Length, actual.Length);
        var extra = Math.Max(0, actual.Length - expected.Length);
        var score = Math.Clamp((int)Math.Round(correct * 100d / expected.Length) - extra * ExtraInputPenalty, 0, 100);
        var details = new SequenceEvaluationDetails
        {
            ExpectedInputCount = expected.Length,
            CorrectInputCount = correct,
            MissedInputCount = missed,
            IncorrectInputCount = incorrect,
            ExtraInputCount = extra,
            AverageAbsoluteTimingErrorMilliseconds = timingErrors.Count == 0 ? null : timingErrors.Average(Math.Abs),
            TimingTendency = GetTimingTendency(timingErrors)
        };

        return new ExerciseEvaluationResult
        {
            IsSuccessful = score >= RhythmExerciseEvaluator.SuccessScore,
            Score = score,
            Feedback = inputEvents.Count == 0 ? ExerciseEvaluationFeedback.NoInput :
                score >= RhythmExerciseEvaluator.SuccessScore ? ExerciseEvaluationFeedback.Correct : ExerciseEvaluationFeedback.Incorrect,
            ExpectedValue = expected[0].Value,
            ActualValue = actual.FirstOrDefault()?.Value,
            Speed = exerciseType == ExerciseType.Speed ? details : null,
            FingerIndependence = exerciseType == ExerciseType.FingerIndependence ? details : null
        };
    }

    private static bool IsWithinTimingTolerance(ExpectedInput expected, ExerciseInputEvent actual, List<double> errors)
    {
        if (!expected.ExpectedOffset.HasValue)
        {
            return true;
        }

        var error = (actual.Offset - expected.ExpectedOffset.Value).TotalMilliseconds;
        errors.Add(error);
        return Math.Abs(error) <= RhythmExerciseEvaluator.AcceptableToleranceMilliseconds;
    }

    private static RhythmTimingTendency GetTimingTendency(IReadOnlyList<double> errors)
    {
        if (errors.Count == 0) return RhythmTimingTendency.None;
        var average = errors.Average();
        return average < -OnTimeTendencyToleranceMilliseconds ? RhythmTimingTendency.Early :
            average > OnTimeTendencyToleranceMilliseconds ? RhythmTimingTendency.Late : RhythmTimingTendency.OnTime;
    }
}
