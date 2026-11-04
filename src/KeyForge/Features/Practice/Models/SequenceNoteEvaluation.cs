namespace KeyForge.Features.Practice.Models;

/// <summary>Pitch and optional onset comparison for one sequence note attack.</summary>
public sealed class SequenceNoteEvaluation
{
    public required int NoteNumber { get; init; }
    public required string ExpectedNote { get; init; }
    public string? ActualNote { get; init; }
    public TimeSpan? ExpectedOffset { get; init; }
    public TimeSpan? ActualOffset { get; init; }
    /// <summary>Actual onset minus expected onset; positive values are late.</summary>
    public TimeSpan? TimingError { get; init; }
    public required bool IsPitchCorrect { get; init; }
    /// <summary>Null means timing is not part of this exercise.</summary>
    public bool? IsTimingCorrect { get; init; }
}
