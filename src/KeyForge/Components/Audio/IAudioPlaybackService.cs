namespace KeyForge.Components.Audio;

/// <summary>UI-layer audio abstraction backed by browser interop.</summary>
public interface IAudioPlaybackService
{
    event Action? StateChanged;

    AudioPlaybackState State { get; }

    /// <summary>Why audio is currently unusable; <see cref="AudioFailureReason.None"/> when usable.</summary>
    AudioFailureReason FailureReason { get; }
    string? LastError { get; }
    /// <summary>Preloads the audio library and binds unlock to this button's browser click. Does not start audio.</summary>
    Task PrepareActivationAsync(Microsoft.AspNetCore.Components.ElementReference button);
    Task<bool> InitializeAsync();
    Task<bool> SetInstrumentPresetAsync(InstrumentPreset preset);
    Task<bool> PlayNoteAsync(MusicalNote note);

    /// <summary>
    /// Plays the given notes sequentially at their configured delays. Starting a
    /// new sequence cancels any previously scheduled sequence before it sounds.
    /// </summary>
    Task<bool> PlaySequenceAsync(IReadOnlyList<AudioSequenceNote> sequence);

    Task<bool> PlayMetronomeClickAsync();
    Task StopActiveNotesAsync();
}
