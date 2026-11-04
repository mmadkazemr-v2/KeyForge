namespace KeyForge.Features.Exercises.Models;

/// <summary>A complete measure of authored beginner rhythm events.</summary>
public sealed class RhythmMeasureDefinition
{
    public List<RhythmPatternEvent> Pattern { get; set; } = [];
}
