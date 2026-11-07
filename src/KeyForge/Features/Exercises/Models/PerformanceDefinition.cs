namespace KeyForge.Features.Exercises.Models;

/// <summary>An optional, quarter-note-based performance timeline. Hand and voice are instructions, not sensed identity.</summary>
public sealed class PerformanceDefinition
{
    public List<MusicalPerformanceEvent> Events { get; set; } = [];
    public PerformanceTimingReference TimingReference { get; set; } = PerformanceTimingReference.FirstAttack;
    public PerformanceScoringMode ScoringMode { get; set; }
    public int ReleaseToleranceMilliseconds { get; set; } = 150;
    public int OnsetToleranceMilliseconds { get; set; } = 150;

    public void Validate()
    {
        if (!Enum.IsDefined(ScoringMode) || ReleaseToleranceMilliseconds is < 20 or > 1000 || !Enum.IsDefined(TimingReference) || OnsetToleranceMilliseconds is < 20 or > 500)
            throw new InvalidOperationException("Invalid performance timing reference or onset tolerance (20�500ms).");
        if (Events is null || Events.Count is < 1 or > 4096 || Events.All(e => e?.Pitch is null))
            throw new InvalidOperationException("Performance requires 1�4096 events including at least one pitched note.");
        foreach (var item in Events)
        {
            if (item is null || !double.IsFinite(item.OnsetBeats) || !double.IsFinite(item.DurationBeats) ||
                item.OnsetBeats < 0 || item.DurationBeats <= 0 || item.OnsetBeats + item.DurationBeats > 10000 ||
                !Enum.IsDefined(item.Hand) || item.Pitch is not null && !MusicalNote.TryParse(item.Pitch, out _))
                throw new InvalidOperationException("Invalid performance pitch, onset, duration or hand instruction.");
        }
        if (Events.Where(e => e.Pitch is not null).GroupBy(e => (MusicalNote.Parse(e.Pitch!).MidiNumber, e.OnsetBeats)).Any(g => g.Count() > 1))
            throw new InvalidOperationException("Duplicate pitch at the same onset is ambiguous on a piano.");
    }
}

public sealed class MusicalPerformanceEvent
{
    /// <summary>Null represents a rest. Explicit octave required for pitched notes.</summary>
    public string? Pitch { get; set; }
    public double OnsetBeats { get; set; }
    public double DurationBeats { get; set; }
    public string? Voice { get; set; }
    public PracticeHand Hand { get; set; } = PracticeHand.Either;
}

public enum PerformanceTimingReference
{
    FirstAttack,
    WorkspaceStart
}

public enum PerformanceScoringMode
{
    OnsetOnly,
    KeyHold,
    PedalSustain
}
