namespace KeyForge.Features.Practice.Models;

/// <summary>A note attack that had no corresponding expected sequence position.</summary>
public sealed class UnexpectedSequenceNote
{
    public required string ActualNote { get; init; }
    public required TimeSpan ActualOffset { get; init; }
}
