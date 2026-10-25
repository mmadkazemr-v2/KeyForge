namespace KeyForge.Features.Exercises.Models;

/// <summary>Beginner scale configuration with an explicit physical starting pitch.</summary>
public sealed class ScaleConfiguration
{
    /// <summary>Root pitch including octave, e.g. C4 or D4.</summary>
    public string Root { get; set; } = string.Empty;
    public ScaleType Type { get; set; } = ScaleType.Major;
    public ScaleDirection Direction { get; set; } = ScaleDirection.Ascending;
    public int Octaves { get; set; } = 1;
}

public enum ScaleType
{
    Major,
    NaturalMinor
}

public enum ScaleDirection
{
    Ascending,
    Descending,
    AscendingDescending
}
