namespace KeyForge.Components.Audio;

/// <summary>A small Tone.js practice-sound configuration, not a sampled instrument.</summary>
public sealed record InstrumentPreset(
    string Id,
    string Name,
    string Description,
    string Engine,
    string Oscillator,
    double Volume,
    double Attack,
    double Decay,
    double Sustain,
    double Release,
    int OctaveOffset = 0);

public static class InstrumentPresetCatalog
{
    public static IReadOnlyList<InstrumentPreset> All { get; } =
    [
        new("basic-piano", "Basic Piano", "Clean and neutral for everyday practice", "Synth", "triangle", -12, 0.005, 0.28, 0.12, 0.18),
        new("soft-keys", "Soft Keys", "Gentle attack and a mellow tone", "Synth", "sine", -14, 0.04, 0.22, 0.2, 0.35),
        new("electric-keys", "Electric Keys", "Bell-like tones with a quick response", "FMSynth", "sine", -16, 0.006, 0.25, 0.08, 0.2),
        new("organ", "Organ", "Sustained tone for chord and harmony practice", "AMSynth", "square", -18, 0.006, 0.04, 0.9, 0.08),
        new("synth-lead", "Synth Lead", "Fast and bright for melodic lead practice", "Synth", "sawtooth", -18, 0.003, 0.12, 0.4, 0.1),
        new("soft-pad", "Soft Pad", "Slow attack and long release for smooth lines", "Synth", "triangle", -20, 0.35, 0.3, 0.65, 0.8),
        new("bright-synth", "Bright Synth", "Bright, crisp tone for rhythmic parts", "FMSynth", "square", -20, 0.002, 0.12, 0.2, 0.12),
        new("bass", "Bass", "Focused low-register practice sound", "Synth", "triangle", -12, 0.004, 0.18, 0.25, 0.12, -1)
    ];

    public static InstrumentPreset Default => All[0];

    public static InstrumentPreset Get(string id) =>
        All.FirstOrDefault(preset => string.Equals(preset.Id, id, StringComparison.Ordinal))
        ?? throw new ArgumentException($"Unknown practice sound '{id}'.", nameof(id));
}
