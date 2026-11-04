namespace KeyForge.Features.Practice.Models;

/// <summary>One duration-bearing note or rest at a compiled timeline position.</summary>
public sealed class RhythmTimelineEvent
{
    public required RhythmDuration Duration { get; init; }
    public required bool IsRest { get; init; }
    public required TimeSpan StartOffset { get; init; }
    public required TimeSpan Length { get; init; }
    public required int MeasureNumber { get; init; }
    public required int StartSixteenthUnits { get; init; }
    public int DurationSixteenthUnits => RhythmDurationUnits.GetSixteenthUnits(Duration);
}
