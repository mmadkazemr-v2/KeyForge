namespace KeyForge.Features.Exercises.Models;

/// <summary>An inclusive chromatic pitch range authored with explicit octave labels.</summary>
public sealed class PitchRangeDefinition
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}
