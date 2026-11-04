namespace KeyForge.Components.Music;

/// <summary>Maps KeyForge music concepts to the small VexFlow interop contract.</summary>
public static class VexFlowNotationMapper
{
    public static VexFlowStaffRenderRequest MapRhythm(RhythmChallengeDetails rhythm, string timeSignature)
    {
        ArgumentNullException.ThrowIfNull(rhythm);
        var meter = RhythmMeter.Parse(timeSignature);
        return new VexFlowStaffRenderRequest
        {
            Clef = "percussion",
            NoteKey = "b/4",
            TimeSignature = $"{meter.Numerator}/{meter.Denominator}",
            RhythmMeasures = rhythm.Events.GroupBy(item => item.MeasureNumber)
                .Select(measure => (IReadOnlyList<VexFlowRhythmEvent>)measure.Select(MapRhythmEvent).ToArray())
                .ToArray()
        };
    }

    private static VexFlowRhythmEvent MapRhythmEvent(RhythmTimelineEvent item)
    {
        var duration = item.Duration switch
        {
            RhythmDuration.Whole => "w",
            RhythmDuration.Half or RhythmDuration.DottedHalf => "h",
            RhythmDuration.Quarter or RhythmDuration.DottedQuarter => "q",
            RhythmDuration.Eighth or RhythmDuration.DottedEighth => "8",
            RhythmDuration.Sixteenth => "16",
            _ => throw new ArgumentOutOfRangeException(nameof(item), "Unsupported notation duration.")
        };
        var dotted = item.Duration is RhythmDuration.DottedHalf or RhythmDuration.DottedQuarter or RhythmDuration.DottedEighth;
        return new VexFlowRhythmEvent($"{duration}{(dotted ? "d" : string.Empty)}{(item.IsRest ? "r" : string.Empty)}", dotted);
    }

    public static bool TryMap(
        ClefType clef,
        MusicalNote? note,
        out VexFlowStaffRenderRequest? request)
    {
        request = null;
        if (note is null || note.IsBlackKey)
        {
            return false;
        }

        var clefName = clef switch
        {
            ClefType.Treble => "treble",
            ClefType.Bass => "bass",
            _ => null
        };

        if (clefName is null)
        {
            return false;
        }

        request = new VexFlowStaffRenderRequest
        {
            Clef = clefName,
            NoteKey = $"{note.Name.ToLowerInvariant()}/{note.Octave.ToString(CultureInfo.InvariantCulture)}"
        };
        return true;
    }
}
