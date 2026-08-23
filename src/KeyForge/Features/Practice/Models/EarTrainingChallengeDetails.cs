namespace KeyForge.Features.Practice.Models;

/// <summary>Presentation-neutral data for an ear-training challenge.</summary>
public sealed class EarTrainingChallengeDetails
{
    public required EarTrainingTask TaskType { get; init; }
    public required IReadOnlyList<string> AvailableNotes { get; init; }
}
