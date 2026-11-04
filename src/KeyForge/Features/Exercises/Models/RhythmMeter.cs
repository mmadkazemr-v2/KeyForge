namespace KeyForge.Features.Exercises.Models;

/// <summary>A supported meter with bar and denominator-beat lengths in sixteenth units.</summary>
public readonly record struct RhythmMeter(int Numerator, int Denominator)
{
    public int BarSixteenthUnits => Numerator * (16 / Denominator);
    public int BeatSixteenthUnits => 16 / Denominator;

    public static RhythmMeter Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim() switch
        {
            "2/4" => new RhythmMeter(2, 4),
            "3/4" => new RhythmMeter(3, 4),
            "4/4" => new RhythmMeter(4, 4),
            "6/8" => new RhythmMeter(6, 8),
            _ => throw new InvalidOperationException(
                $"Time signature '{value}' is unsupported. Supported meters are 2/4, 3/4, 4/4, and 6/8.")
        };
    }
}
