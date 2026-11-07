namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Evaluates note order and optional beat timing for the two sequence drills.
/// </summary>
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
        var noteEvaluations = new List<SequenceNoteEvaluation>();
        for (var index = 0; index < expected.Length; index++)
        {
            var actualInput = index < actual.Length ? actual[index] : null;
            var noteMatches = actualInput is not null &&
                              string.Equals(expected[index].Value, actualInput.Value, StringComparison.OrdinalIgnoreCase);
            var timingError = actualInput is not null && expected[index].ExpectedOffset is { } expectedOffset
                ? actualInput.Offset - expectedOffset
                : (TimeSpan?)null;
            if (timingError.HasValue)
                timingErrors.Add(timingError.Value.TotalMilliseconds);
            var timingMatches = actualInput is not null &&
                                (!requireTiming || !timingError.HasValue ||
                                 Math.Abs(timingError.Value.TotalMilliseconds) <= RhythmExerciseEvaluator.AcceptableToleranceMilliseconds);
            if (noteMatches && timingMatches)
            {
                correct++;
            }
            else
            {
                incorrect++;
            }

            noteEvaluations.Add(new SequenceNoteEvaluation
            {
                NoteNumber = index + 1,
                ExpectedNote = expected[index].Value,
                ActualNote = actualInput?.Value,
                ExpectedOffset = expected[index].ExpectedOffset,
                ActualOffset = timingError.HasValue ? actualInput!.Offset : null,
                TimingError = timingError,
                IsPitchCorrect = noteMatches,
                IsTimingCorrect = timingError.HasValue
                    ? Math.Abs(timingError.Value.TotalMilliseconds) <= RhythmExerciseEvaluator.AcceptableToleranceMilliseconds
                    : null
            });
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
            TimingTendency = GetTimingTendency(timingErrors),
            Notes = noteEvaluations,
            UnexpectedNotes = actual.Skip(expected.Length).Select(input => new UnexpectedSequenceNote
            {
                ActualNote = input.Value,
                ActualOffset = input.Offset
            }).ToArray()
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
            FingerIndependence = exerciseType == ExerciseType.FingerIndependence ? details : null,
            Sequence = exerciseType == ExerciseType.Sequence ? details : null
        };
    }

    private static RhythmTimingTendency GetTimingTendency(IReadOnlyList<double> errors)
    {
        if (errors.Count == 0) return RhythmTimingTendency.None;
        var average = errors.Average();
        return average < -OnTimeTendencyToleranceMilliseconds ? RhythmTimingTendency.Early :
            average > OnTimeTendencyToleranceMilliseconds ? RhythmTimingTendency.Late : RhythmTimingTendency.OnTime;
    }
}
