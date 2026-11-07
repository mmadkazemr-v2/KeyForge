namespace KeyForge.Components.Audio;

/// <summary>UI-layer audio abstraction backed by browser interop.</summary>
public interface IAudioPlaybackService
{
    public event Action? StateChanged;

    public AudioPlaybackState State { get; }

    /// <summary>Why audio is currently unusable; <see cref="AudioFailureReason.None"/> when usable.</summary>
    public AudioFailureReason FailureReason { get; }
    public string? LastError { get; }
    /// <summary>Preloads the audio library and binds unlock to this button's browser click. Does not start audio.</summary>
    public Task PrepareActivationAsync(Microsoft.AspNetCore.Components.ElementReference button);
    public Task<bool> InitializeAsync();
    public Task<bool> SetInstrumentPresetAsync(InstrumentPreset preset);
    public Task<bool> PlayNoteAsync(MusicalNote note);

    /// <summary>Plays a simultaneous chord as an explicitly requested learning stimulus.</summary>
    public Task<bool> PlayChordAsync(IReadOnlyList<MusicalNote> notes);

    /// <summary>
    /// Plays the given notes sequentially at their configured delays. Starting a
    /// new sequence cancels any previously scheduled sequence before it sounds.
    /// </summary>
    public Task<bool> PlaySequenceAsync(IReadOnlyList<AudioSequenceNote> sequence);

    public Task<bool> PlayMetronomeClickAsync();
    public Task StopActiveNotesAsync();
}
