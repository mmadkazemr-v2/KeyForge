namespace KeyForge.Components.Audio;

/// <summary>The browser-audio state exposed to UI consumers.</summary>
public enum AudioPlaybackState
{
    Uninitialized,
    Initializing,
    Ready,
    Failed
}
