namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// An exercise that practises timing, subdivisions and reading rhythm patterns.
/// </summary>
public sealed class RhythmExerciseDefinition : ExerciseDefinition
{
    /// <summary>
    /// Meter the exercise is performed in, e.g. "4/4" or "6/8".
    /// </summary>
    public string TimeSignature { get; set; } = "4/4";

    /// <summary>
    /// Note values included in the exercise, e.g. ["quarter", "eighth", "sixteenth"].
    /// </summary>
    public List<string> NoteValues { get; set; } = [];

    /// <summary>
    /// One explicitly authored measure. Use this or <see cref="Measures"/>.
    /// </summary>
    public List<RhythmPatternEvent>? Pattern { get; set; }

    /// <summary>
    /// Explicit authored measures. Use this or <see cref="Pattern"/>.
    /// </summary>
    public List<RhythmMeasureDefinition>? Measures { get; set; }

    /// <summary>
    /// Number of rhythm patterns (bars/measures) to practise.
    /// </summary>
    public int PatternCount { get; set; } = 1;

    /// <summary>
    /// Default constructor that pins the polymorphic discriminator.
    /// </summary>
    public RhythmExerciseDefinition() => Type = ExerciseType.Rhythm;

    /// <summary>
    /// Validates complete authored measures, preserving the legacy pulse fallback.
    /// </summary>
    public void Validate()
    {
        RhythmMeter meter;
        try
        {
            meter = RhythmMeter.Parse(TimeSignature);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            throw new InvalidOperationException($"field 'timeSignature': {ex.Message}", ex);
        }

        if (Pattern is not null && Measures is not null)
            throw new InvalidOperationException("define either 'pattern' or 'measures', not both.");

        if (Pattern is not null || Measures is not null)
        {
            if (PatternCount != 1)
                throw new InvalidOperationException("field 'patternCount' applies only to legacy pulses; use 'measures' for authored multiple bars.");
            if (Pattern is not null)
            {
                ValidateMeasure(Pattern, meter, "pattern");
            }
            else
            {
                if (Measures!.Count is < 1 or > 100)
                    throw new InvalidOperationException("field 'measures' must contain between 1 and 100 measures.");
                for (var index = 0; index < Measures.Count; index++)
                    ValidateMeasure(Measures[index].Pattern, meter, $"measures[{index}].pattern");
            }
        }
        else if (PatternCount is < 1 or > 100)
        {
            throw new InvalidOperationException("field 'patternCount' must be between 1 and 100.");
        }
    }

    private static void ValidateMeasure(IReadOnlyCollection<RhythmPatternEvent> pattern, RhythmMeter meter, string field)
    {
        if (pattern.Count == 0)
            throw new InvalidOperationException($"field '{field}' must not be empty.");
        var units = 0;
        foreach (var item in pattern)
        {
            if (!Enum.IsDefined(item.Duration))
                throw new InvalidOperationException($"field '{field}.duration' is unsupported.");
            units += RhythmDurationUnits.GetSixteenthUnits(item.Duration);
        }

        if (units != meter.BarSixteenthUnits)
            throw new InvalidOperationException($"field '{field}' does not fill one {meter.Numerator}/{meter.Denominator} measure.");
    }
}