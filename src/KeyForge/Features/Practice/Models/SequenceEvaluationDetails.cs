namespace KeyForge.Features.Practice.Models;

/// <summary>Compact automatic feedback for a note sequence attempt.</summary>
public sealed class SequenceEvaluationDetails
{
    public required int ExpectedInputCount { get; init; }
    public required int CorrectInputCount { get; init; }
    public required int MissedInputCount { get; init; }
    public required int IncorrectInputCount { get; init; }
    public required int ExtraInputCount { get; init; }
    public double? AverageAbsoluteTimingErrorMilliseconds { get; init; }
    public required RhythmTimingTendency TimingTendency { get; init; }
}
