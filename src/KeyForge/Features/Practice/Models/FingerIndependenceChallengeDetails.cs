namespace KeyForge.Features.Practice.Models;

/// <summary>Presentation-neutral sequence and instruction data for a finger drill.</summary>
public sealed class FingerIndependenceChallengeDetails
{
    public required PracticeHand Hand { get; init; }
    public required IReadOnlyList<int> Fingers { get; init; }
    public required IReadOnlyList<string> ExpectedNotes { get; init; }
}
