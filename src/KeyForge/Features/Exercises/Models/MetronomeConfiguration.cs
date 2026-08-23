namespace KeyForge.Features.Exercises.Models;

/// <summary>Optional reusable metronome capabilities for a tempo-based exercise.</summary>
public sealed class MetronomeConfiguration
{
    public bool Enabled { get; set; }
    public bool Visual { get; set; }
    public bool Audio { get; set; }
    public int CountInBeats { get; set; }

    public void Validate()
    {
        if (CountInBeats < 0)
        {
            throw new InvalidOperationException("Metronome count-in beats cannot be negative.");
        }
    }
}
