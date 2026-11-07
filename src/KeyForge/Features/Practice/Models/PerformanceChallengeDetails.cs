namespace KeyForge.Features.Practice.Models;

public sealed record PerformanceEvent(string? Pitch, TimeSpan Onset, TimeSpan Duration, string? Voice, PracticeHand Hand);

public sealed class PerformanceChallengeDetails
{
    public required IReadOnlyList<PerformanceEvent> Events { get; init; }
    public required PerformanceTimingReference TimingReference { get; init; }
    public PerformanceScoringMode ScoringMode { get; init; }
    public int ReleaseToleranceMilliseconds { get; init; }
    public required int OnsetToleranceMilliseconds { get; init; }
}

public sealed record PerformanceNoteEvaluation(string Pitch, TimeSpan ExpectedOnset, TimeSpan? ActualOnset, double? OnsetErrorMilliseconds, bool IsCorrect)
{
    public string? Voice { get; init; }
    public PracticeHand Hand { get; init; }
    public double? DurationErrorMilliseconds { get; init; }
    public PerformanceReleaseAccuracy ReleaseAccuracy { get; init; }
}

public enum PerformanceReleaseAccuracy { NotScored, Correct, Early, Late, Missing }

public sealed class PerformanceEvaluationDetails
{
    public required IReadOnlyList<PerformanceNoteEvaluation> Notes { get; init; }
    public int UnexpectedReleases { get; init; }
    public int ExtraNotes { get; init; }
    public required PerformanceTimingReference TimingReference { get; init; }
}
