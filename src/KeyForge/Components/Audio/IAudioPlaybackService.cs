namespace KeyForge.Components.Audio;

/// <summary>UI-layer audio abstraction backed by browser interop.</summary>
public interface IAudioPlaybackService
{
    AudioPlaybackState State { get; }
    string? LastError { get; }
    Task<bool> InitializeAsync();
    Task<bool> PlayNoteAsync(MusicalNote note);
    Task<bool> PlayMetronomeClickAsync();
    Task StopActiveNotesAsync();
}
