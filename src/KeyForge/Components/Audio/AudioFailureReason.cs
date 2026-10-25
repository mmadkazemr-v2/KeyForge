namespace KeyForge.Components.Audio;

/// <summary>Why browser audio is not usable right now.</summary>
public enum AudioFailureReason
{
    None,

    /// <summary>Playback was requested before audio was enabled.</summary>
    NotInitialized,

    /// <summary>The browser refused activation or left the context suspended.</summary>
    BrowserBlocked,

    /// <summary>The audio context has been closed and cannot be resumed.</summary>
    ContextClosed,

    /// <summary>The browser or circuit cannot provide audio at all.</summary>
    Unavailable
}
