namespace KeyForge.Features.Exercises.Models;

/// <summary>One playable hit or rest in a rhythm measure.</summary>
public sealed class RhythmPatternEvent
{
    public RhythmDuration Duration { get; set; }
    public bool Rest { get; set; }
}