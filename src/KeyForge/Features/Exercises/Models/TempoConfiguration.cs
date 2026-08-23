namespace KeyForge.Features.Exercises.Models;

/// <summary>Optional, data-driven tempo settings for an exercise.</summary>
public sealed class TempoConfiguration
{
    public int DefaultBpm { get; set; }
    public bool AllowUserChange { get; set; }
    public int? MinBpm { get; set; }
    public int? MaxBpm { get; set; }
    public int Step { get; set; } = 1;
    public List<int> SuggestedBpms { get; set; } = [];

    /// <summary>Validates the configuration without mutating the lesson definition.</summary>
    public void Validate()
    {
        if (DefaultBpm <= 0)
        {
            throw new InvalidOperationException("Tempo default BPM must be greater than zero.");
        }

        if (Step <= 0)
        {
            throw new InvalidOperationException("Tempo adjustment step must be greater than zero.");
        }

        if (MinBpm is { } min && min <= 0)
        {
            throw new InvalidOperationException("Tempo minimum BPM must be greater than zero.");
        }

        if (MaxBpm is { } max && max <= 0)
        {
            throw new InvalidOperationException("Tempo maximum BPM must be greater than zero.");
        }

        if (MinBpm is { } minimum && MaxBpm is { } maximum && minimum > maximum)
        {
            throw new InvalidOperationException("Tempo minimum BPM cannot exceed maximum BPM.");
        }

        if (MinBpm is { } lower && DefaultBpm < lower || MaxBpm is { } upper && DefaultBpm > upper)
        {
            throw new InvalidOperationException("Tempo default BPM must be inside the configured range.");
        }

        if (SuggestedBpms.Any(bpm => bpm <= 0))
        {
            throw new InvalidOperationException("Suggested BPM values must be greater than zero.");
        }

        if (SuggestedBpms.Any(bpm => MinBpm is { } lower && bpm < lower || MaxBpm is { } upper && bpm > upper))
        {
            throw new InvalidOperationException("Suggested BPM values must be inside the configured range.");
        }
    }

    public bool Supports(int bpm) =>
        bpm > 0 &&
        (!MinBpm.HasValue || bpm >= MinBpm.Value) &&
        (!MaxBpm.HasValue || bpm <= MaxBpm.Value);
}
