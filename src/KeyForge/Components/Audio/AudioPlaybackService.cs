namespace KeyForge.Components.Audio;

/// <summary>Loads the small Tone.js interop module and keeps browser-audio concerns in the UI layer.</summary>
public sealed class AudioPlaybackService(IJSRuntime javascript) : IAudioPlaybackService, IAsyncDisposable
{
    private const string RunningContextState = "running";

    private IJSObjectReference? _module;
    private Task<bool>? _initialization;
    private bool _disposed;

    public event Action? StateChanged;

    public AudioPlaybackState State { get; private set; } = AudioPlaybackState.Uninitialized;
    public AudioFailureReason FailureReason { get; private set; } = AudioFailureReason.None;
    public string? LastError { get; private set; }

    public async Task PrepareActivationAsync(Microsoft.AspNetCore.Components.ElementReference button)
    {
        if (_disposed) return;
        try
        {
            _module ??= await javascript.InvokeAsync<IJSObjectReference>(
                "import", "./Components/Audio/AudioPlaybackService.js");
            if (_disposed)
            {
                await _module.DisposeAsync();
                _module = null;
                return;
            }
            await _module.InvokeVoidAsync("prepareActivation", button);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException)
        {
            Fail(AudioFailureReason.Unavailable, $"Browser audio preparation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Enables browser audio from an explicit user action. Concurrent callers share one
    /// initialization, repeated calls are safe, and a failed attempt can be retried.
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        if (_disposed)
        {
            return Fail(AudioFailureReason.Unavailable);
        }

        if (_initialization is not null)
        {
            return await _initialization;
        }

        _initialization ??= InitializeCoreAsync();
        var initialized = await _initialization;

        // Share only the in-flight attempt; recheck browser state on later calls.
        _initialization = null;

        return initialized;
    }

    private async Task<bool> InitializeCoreAsync()
    {
        var wasReady = State == AudioPlaybackState.Ready;
        LastError = null;
        FailureReason = AudioFailureReason.None;
        if (!wasReady)
        {
            State = AudioPlaybackState.Initializing;
            StateChanged?.Invoke();
        }

        try
        {
            _module ??= await javascript.InvokeAsync<IJSObjectReference>(
                "import", "./Components/Audio/AudioPlaybackService.js");
            if (_disposed)
            {
                await _module.DisposeAsync();
                _module = null;
                return Fail(AudioFailureReason.Unavailable);
            }

            if (wasReady)
            {
                var existingState = await _module.InvokeAsync<string>("getState");
                if (_disposed) return Fail(AudioFailureReason.Unavailable);
                if (existingState == RunningContextState) return true;
            }

            if (wasReady)
            {
                State = AudioPlaybackState.Initializing;
                StateChanged?.Invoke();
            }

            // The module reports the resulting AudioContext state so that autoplay
            // blocking is detected instead of being silently reported as success.
            var contextState = await _module.InvokeAsync<string>("initialize");
            if (_disposed) return Fail(AudioFailureReason.Unavailable);
            if (string.Equals(contextState, RunningContextState, StringComparison.OrdinalIgnoreCase))
            {
                State = AudioPlaybackState.Ready;
                StateChanged?.Invoke();
                return true;
            }

            return Fail(contextState switch
            {
                "suspended" or "blocked" => AudioFailureReason.BrowserBlocked,
                "closed" => AudioFailureReason.ContextClosed,
                _ => AudioFailureReason.Unavailable
            }, $"Audio initialization returned context state '{contextState}'.");
        }
        catch (JSException ex)
        {
            return Fail(AudioFailureReason.Unavailable, $"Browser audio interop failed: {ex.Message}");
        }
        catch (JSDisconnectedException)
        {
            return Fail(AudioFailureReason.Unavailable);
        }
    }

    public async Task<bool> PlayNoteAsync(MusicalNote note)
    {
        ArgumentNullException.ThrowIfNull(note);
        if (!IsReady())
        {
            return false;
        }

        try
        {
            await _module!.InvokeVoidAsync("playNote", AudioNoteRequest.From(note).Note);
            return true;
        }
        catch (JSException ex)
        {
            return Fail(AudioFailureReason.Unavailable, $"Note playback failed: {ex.Message}");
        }
        catch (JSDisconnectedException)
        {
            return Fail(AudioFailureReason.Unavailable);
        }
    }

    public async Task<bool> SetInstrumentPresetAsync(InstrumentPreset preset)
    {
        ArgumentNullException.ThrowIfNull(preset);
        if (_disposed) return false;

        try
        {
            _module ??= await javascript.InvokeAsync<IJSObjectReference>(
                "import", "./Components/Audio/AudioPlaybackService.js");
            if (_disposed) return false;
            await _module.InvokeVoidAsync("setInstrumentPreset", preset);
            return true;
        }
        catch (JSException ex)
        {
            return Fail(AudioFailureReason.Unavailable, $"Practice sound selection failed: {ex.Message}");
        }
        catch (JSDisconnectedException)
        {
            return Fail(AudioFailureReason.Unavailable);
        }
    }

    public async Task<bool> PlaySequenceAsync(IReadOnlyList<AudioSequenceNote> sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        if (!IsReady())
        {
            return false;
        }

        try
        {
            // Wrap explicitly: a bare typed array would be unwrapped by the
            // params binder and reach JavaScript as multiple arguments.
            await _module!.InvokeVoidAsync("playSequence", new object?[] { sequence.ToArray() });
            return true;
        }
        catch (JSException ex)
        {
            return Fail(AudioFailureReason.Unavailable, $"Sequence playback failed: {ex.Message}");
        }
        catch (JSDisconnectedException)
        {
            return Fail(AudioFailureReason.Unavailable);
        }
    }

    public async Task<bool> PlayMetronomeClickAsync()
    {
        if (!IsReady())
        {
            return false;
        }

        try
        {
            await _module!.InvokeVoidAsync("playMetronomeClick");
            return true;
        }
        catch (JSException ex)
        {
            return Fail(AudioFailureReason.Unavailable, $"Metronome playback failed: {ex.Message}");
        }
        catch (JSDisconnectedException)
        {
            return Fail(AudioFailureReason.Unavailable);
        }
    }

    public async Task StopActiveNotesAsync()
    {
        if (_module is null || _disposed)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("stopActiveNotes");
        }
        catch (JSDisconnectedException)
        {
            // The browser circuit has already closed.
        }
        catch (JSException)
        {
            // Cleanup must not fail navigation or component disposal.
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("dispose");
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException)
        {
            // Browser cleanup must not break navigation.
        }
        try
        {
            await _module.DisposeAsync();
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException)
        {
            // Release the interop reference even when browser cleanup failed.
        }
        _module = null;
        State = AudioPlaybackState.Failed;
        FailureReason = AudioFailureReason.Unavailable;
    }

    private bool IsReady()
    {
        if (!_disposed && State == AudioPlaybackState.Ready && _module is not null)
        {
            return true;
        }

        if (State != AudioPlaybackState.Failed)
        {
            FailureReason = AudioFailureReason.NotInitialized;
            LastError = "Audio must be enabled before playback.";
        }
        return false;
    }

    private bool Fail(AudioFailureReason reason, string? diagnostic = null)
    {
        State = AudioPlaybackState.Failed;
        FailureReason = reason;
        LastError = diagnostic ?? (reason switch
        {
            AudioFailureReason.BrowserBlocked => "The AudioContext could not reach the running state.",
            AudioFailureReason.NotInitialized => "Audio must be enabled before playback.",
            AudioFailureReason.ContextClosed => "The AudioContext is closed. Reload the page to restore audio.",
            _ => "Browser audio is unavailable."
        });
        StateChanged?.Invoke();
        return false;
    }
}
