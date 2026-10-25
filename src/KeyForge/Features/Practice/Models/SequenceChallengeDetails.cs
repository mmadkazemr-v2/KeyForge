namespace KeyForge.Features.Practice.Models;

/// <summary>Pitch order and instructional metadata for an authored sequence.</summary>
public sealed class SequenceChallengeDetails
{
    public required IReadOnlyList<string> ExpectedNotes { get; init; }
    public required PracticeHand Hand { get; init; }
    public required IReadOnlyList<int> Fingers { get; init; }
}
