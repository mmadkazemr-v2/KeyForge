namespace KeyForge.Features.Practice.Services;

/// <summary>Matches captured beat inputs to expected pulses and scores timing accuracy.</summary>
public sealed class RhythmExerciseEvaluator
{
    public const double ExcellentToleranceMilliseconds = 40;
    public const double GoodToleranceMilliseconds = 80;
    public const double AcceptableToleranceMilliseconds = 120;
    public const int SuccessScore = 60;

    private const int ExcellentPoints = 100;
    private const int GoodPoints = 80;
    private const int AcceptablePoints = 60;
    private const int ExtraHitPenalty = 5;
    private const double OnTimeTendencyToleranceMilliseconds = 10;

    public ExerciseEvaluationResult Evaluate(
        ExerciseChallenge challenge,
        IReadOnlyList<ExerciseInputEvent> inputEvents)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        ArgumentNullException.ThrowIfNull(inputEvents);

        var expectedBeats = challenge.ExpectedInputs
            .Where(input => input.Type == InputEventType.Beat && input.ExpectedOffset.HasValue)
            .OrderBy(input => input.ExpectedOffset)
            .ToList();

        if (expectedBeats.Count == 0)
        {
            return new ExerciseEvaluationResult
            {
                IsSuccessful = false,
                Score = 0,
                Feedback = ExerciseEvaluationFeedback.UnsupportedChallenge
            };
        }

        var actualHits = inputEvents
            .Where(input => input.Type == InputEventType.Beat)
            .OrderBy(input => input.Offset)
            .ToList();
        var matches = new ExerciseInputEvent?[expectedBeats.Count];
        var timingErrors = new TimeSpan?[expectedBeats.Count];
        var extraHits = 0;

        foreach (var hit in actualHits)
        {
            var nearestIndex = FindNearestUnmatchedBeat(expectedBeats, matches, hit.Offset);
            if (nearestIndex < 0)
            {
                extraHits++;
                continue;
            }

            var error = hit.Offset - expectedBeats[nearestIndex].ExpectedOffset!.Value;
            if (Math.Abs(error.TotalMilliseconds) > AcceptableToleranceMilliseconds)
            {
                extraHits++;
                continue;
            }

            matches[nearestIndex] = hit;
            timingErrors[nearestIndex] = error;
        }

        var beatResults = expectedBeats.Select((expected, index) =>
        {
            var error = timingErrors[index];
            return new RhythmBeatEvaluation
            {
                BeatNumber = index + 1,
                ExpectedOffset = expected.ExpectedOffset!.Value,
                ActualOffset = matches[index]?.Offset,
                TimingError = error,
                Accuracy = GetAccuracy(error)
            };
        }).ToList();

        var matchedErrors = timingErrors
            .Where(error => error.HasValue)
            .Select(error => error!.Value.TotalMilliseconds)
            .ToList();
        var score = CalculateScore(beatResults, extraHits);
        double? averageError = matchedErrors.Count == 0 ? null : matchedErrors.Average();

        return new ExerciseEvaluationResult
        {
            IsSuccessful = score >= SuccessScore,
            Score = score,
            Feedback = score >= SuccessScore
                ? ExerciseEvaluationFeedback.RhythmAccurate
                : ExerciseEvaluationFeedback.RhythmNeedsPractice,
            Rhythm = new RhythmEvaluationDetails
            {
                Beats = beatResults,
                ExpectedBeatCount = expectedBeats.Count,
                SuccessfulHitCount = matchedErrors.Count,
                MissedBeatCount = beatResults.Count(beat => beat.Accuracy == RhythmHitAccuracy.Miss),
                ExtraHitCount = extraHits,
                AverageTimingErrorMilliseconds = averageError,
                AverageAbsoluteTimingErrorMilliseconds = matchedErrors.Count == 0
                    ? null
                    : matchedErrors.Average(error => Math.Abs(error)),
                TimingTendency = GetTimingTendency(averageError)
            }
        };
    }

    private static int FindNearestUnmatchedBeat(
        IReadOnlyList<ExpectedInput> expectedBeats,
        IReadOnlyList<ExerciseInputEvent?> matches,
        TimeSpan actualOffset)
    {
        var nearestIndex = -1;
        var nearestError = double.MaxValue;

        for (var index = 0; index < expectedBeats.Count; index++)
        {
            if (matches[index] is not null)
            {
                continue;
            }

            var error = Math.Abs(
                (actualOffset - expectedBeats[index].ExpectedOffset!.Value).TotalMilliseconds);
            if (error < nearestError)
            {
                nearestError = error;
                nearestIndex = index;
            }
        }

        return nearestIndex;
    }

    private static RhythmHitAccuracy GetAccuracy(TimeSpan? error)
    {
        if (!error.HasValue)
        {
            return RhythmHitAccuracy.Miss;
        }

        var absoluteError = Math.Abs(error.Value.TotalMilliseconds);
        if (absoluteError <= ExcellentToleranceMilliseconds)
        {
            return RhythmHitAccuracy.Excellent;
        }

        if (absoluteError <= GoodToleranceMilliseconds)
        {
            return RhythmHitAccuracy.Good;
        }

        return absoluteError <= AcceptableToleranceMilliseconds
            ? RhythmHitAccuracy.Acceptable
            : RhythmHitAccuracy.Miss;
    }

    private static int CalculateScore(
        IReadOnlyList<RhythmBeatEvaluation> beats,
        int extraHits)
    {
        var earnedPoints = beats.Sum(beat => beat.Accuracy switch
        {
            RhythmHitAccuracy.Excellent => ExcellentPoints,
            RhythmHitAccuracy.Good => GoodPoints,
            RhythmHitAccuracy.Acceptable => AcceptablePoints,
            _ => 0
        });
        var baseScore = (int)Math.Round(earnedPoints / (double)beats.Count);

        return Math.Clamp(baseScore - extraHits * ExtraHitPenalty, 0, 100);
    }

    private static RhythmTimingTendency GetTimingTendency(double? averageError)
    {
        if (!averageError.HasValue)
        {
            return RhythmTimingTendency.None;
        }

        if (averageError < -OnTimeTendencyToleranceMilliseconds)
        {
            return RhythmTimingTendency.Early;
        }

        return averageError > OnTimeTendencyToleranceMilliseconds
            ? RhythmTimingTendency.Late
            : RhythmTimingTendency.OnTime;
    }
}
