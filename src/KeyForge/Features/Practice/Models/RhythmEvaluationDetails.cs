namespace KeyForge.Features.Practice.Models;

/// <summary>Learner-facing summary produced by rhythm timing evaluation.</summary>
public sealed class RhythmEvaluationDetails
{
    public required IReadOnlyList<RhythmBeatEvaluation> Beats { get; init; }
    public required int ExpectedBeatCount { get; init; }
    public required int SuccessfulHitCount { get; init; }
    public required int MissedBeatCount { get; init; }
    public required int ExtraHitCount { get; init; }
    public double? AverageTimingErrorMilliseconds { get; init; }
    public double? AverageAbsoluteTimingErrorMilliseconds { get; init; }
    public required RhythmTimingTendency TimingTendency { get; init; }
}
