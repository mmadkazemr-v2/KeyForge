namespace KeyForge.Features.Practice.Models;

/// <summary>Presentation-neutral sequence information for a speed drill.</summary>
public sealed class SpeedChallengeDetails
{
    public required SpeedPattern Pattern { get; init; }
    public required IReadOnlyList<string> ExpectedNotes { get; init; }
}
