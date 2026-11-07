namespace KeyForge.Features.Exercises.Models;

/// <summary>A broken major/minor triad, including inversions, authored independently of lesson identity.</summary>
public sealed class ArpeggioConfiguration
{
    public string Root { get; set; } = string.Empty;
    public TriadQuality Quality { get; set; }
    public int Inversion { get; set; }
    public int Octaves { get; set; } = 1;
    public ScaleDirection Direction { get; set; }
}

public enum TriadQuality { Major, Minor }
