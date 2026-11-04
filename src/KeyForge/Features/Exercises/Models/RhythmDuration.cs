namespace KeyForge.Features.Exercises.Models;

/// <summary>Basic note values expressed in sixteenth-note units.</summary>
public enum RhythmDuration
{
    Whole,
    Half,
    Quarter,
    Eighth,
    Sixteenth,
    DottedHalf,
    DottedQuarter,
    DottedEighth
}

/// <summary>Musical duration lengths in sixteenth-note units.</summary>
public static class RhythmDurationUnits
{
    public static int GetSixteenthUnits(RhythmDuration duration) => duration switch
    {
        RhythmDuration.Whole => 16,
        RhythmDuration.Half => 8,
        RhythmDuration.Quarter => 4,
        RhythmDuration.Eighth => 2,
        RhythmDuration.Sixteenth => 1,
        RhythmDuration.DottedHalf => 12,
        RhythmDuration.DottedQuarter => 6,
        RhythmDuration.DottedEighth => 3,
        _ => throw new InvalidOperationException($"Unsupported rhythm duration '{duration}'.")
    };
}
