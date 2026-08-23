using Microsoft.JSInterop;

namespace KeyForge.Components.Audio;

/// <summary>Loads the small Tone.js interop module and keeps browser-audio concerns in the UI layer.</summary>
public sealed class AudioPlaybackService(IJSRuntime javascript) : IAudioPlaybackService, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private bool _disposed;

    public AudioPlaybackState State { get; private set; } = AudioPlaybackState.Uninitialized;
    public string? LastError { get; private set; }

    public async Task<bool> InitializeAsync()
    {
        if (_disposed)
        {
            return Fail("Audio playback is no longer available.");
        }

        if (State == AudioPlaybackState.Ready)
        {
            return true;
        }

        try
        {
            _module ??= await javascript.InvokeAsync<IJSObjectReference>(
                "import", "./Components/Audio/AudioPlaybackService.js");
            var initialized = await _module.InvokeAsync<bool>("initialize");
            if (!initialized)
            {
                return Fail("Browser audio could not be initialized.");
            }

            State = AudioPlaybackState.Ready;
            LastError = null;
            return true;
        }
        catch (JSException)
        {
            return Fail("Browser audio could not be initialized.");
        }
        catch (JSDisconnectedException)
        {
            return Fail("The audio connection is no longer available.");
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
        catch (JSException)
        {
            return Fail("The note could not be played.");
        }
        catch (JSDisconnectedException)
        {
            return Fail("The audio connection is no longer available.");
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
        catch (JSException)
        {
            return Fail("The metronome click could not be played.");
        }
        catch (JSDisconnectedException)
        {
            return Fail("The audio connection is no longer available.");
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
        _disposed = true;
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("dispose");
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The circuit may close before DI disposes the scoped service.
        }
    }

    private bool IsReady()
    {
        if (State == AudioPlaybackState.Ready && _module is not null)
        {
            return true;
        }

        LastError = "Audio must be enabled before playback.";
        return false;
    }

    private bool Fail(string error)
    {
        State = AudioPlaybackState.Failed;
        LastError = error;
        return false;
    }
}
