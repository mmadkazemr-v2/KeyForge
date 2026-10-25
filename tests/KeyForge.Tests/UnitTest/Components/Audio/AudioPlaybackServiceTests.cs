using KeyForge.Components.Audio;
using Microsoft.JSInterop;

namespace KeyForge.Tests.UnitTest.Components.Audio;

public sealed class AudioPlaybackServiceTests
{
    [Fact]
    public void AudioNoteRequest_From_PreservesNoteAndOctave()
    {
        var request = AudioNoteRequest.From(MusicalNote.Parse("C5"));

        Assert.Equal("C5", request.Note);
    }

    [Fact]
    public void InstrumentPresetCatalog_HasUniqueIdsAndBasicPianoDefault()
    {
        Assert.Equal(8, InstrumentPresetCatalog.All.Count);
        Assert.Equal(InstrumentPresetCatalog.All.Count,
            InstrumentPresetCatalog.All.Select(preset => preset.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal("basic-piano", InstrumentPresetCatalog.Default.Id);
        Assert.Equal(InstrumentPresetCatalog.Default, InstrumentPresetCatalog.Get("basic-piano"));
    }

    [Fact]
    public void InstrumentPresetCatalog_RejectsUnknownPreset()
    {
        Assert.Throws<ArgumentException>(() => InstrumentPresetCatalog.Get("unknown"));
    }

    [Fact]
    public async Task SetInstrumentPresetAsync_ConfiguresExistingAudioModuleWithoutStartingAudio()
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);

        Assert.True(await service.SetInstrumentPresetAsync(InstrumentPresetCatalog.Get("synth-lead")));

        Assert.Equal(AudioPlaybackState.Uninitialized, service.State);
        Assert.Single(runtime.Imports);
        Assert.Equal("setInstrumentPreset", Assert.Single(runtime.Module.Calls).Identifier);
        Assert.Equal("synth-lead", Assert.IsType<InstrumentPreset>(Assert.Single(runtime.Module.Calls[0].Arguments)).Id);
    }

    [Fact]
    public async Task InitializeAsync_SetsReadyState_AndIsIdempotent()
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);

        Assert.Equal(AudioPlaybackState.Uninitialized, service.State);
        Assert.True(await service.InitializeAsync());
        Assert.True(await service.InitializeAsync());

        Assert.Equal(AudioPlaybackState.Ready, service.State);
        Assert.Single(runtime.Imports);
        Assert.Single(runtime.Module.Calls, call => call.Identifier == "initialize");
    }

    [Fact]
    public async Task InitializeAsync_ImportsModuleFromServedWebRootPath()
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);

        // The module must live under wwwroot so the dynamic import resolves;
        // colocated non-razor .js files are NOT served as static web assets.
        await service.InitializeAsync();

        Assert.Equal("./Components/Audio/AudioPlaybackService.js", Assert.Single(runtime.Imports));
    }

    [Fact]
    public async Task InitializeAsync_WhenInteropFails_RecordsDiagnosticDetail()
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);
        runtime.Module.ThrowOnInitialize = true;

        Assert.False(await service.InitializeAsync());
        Assert.StartsWith("Browser audio interop failed:", service.LastError);
    }

    [Fact]
    public async Task PlayNoteAsync_MapsMusicalNoteAndPreservesOctave()
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);
        await service.InitializeAsync();

        Assert.True(await service.PlayNoteAsync(MusicalNote.Parse("G5")));

        var call = Assert.Single(runtime.Module.Calls, call => call.Identifier == "playNote");
        Assert.Equal("G5", Assert.Single(call.Arguments));
    }

    [Fact]
    public async Task PlayNoteAsync_BeforeInitialization_ReturnsMeaningfulFailure()
    {
        await using var service = new AudioPlaybackService(new FakeJavaScriptRuntime());

        Assert.False(await service.PlayNoteAsync(MusicalNote.Parse("C4")));
        Assert.Equal(AudioPlaybackState.Uninitialized, service.State);
        Assert.NotNull(service.LastError);
    }

    [Fact]
    public async Task PlaySequenceAsync_MapsNotesAndDelaysInOrder()
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);
        await service.InitializeAsync();
        var sequence = new[]
        {
            AudioSequenceNote.From(MusicalNote.Parse("C4"), TimeSpan.Zero),
            AudioSequenceNote.From(MusicalNote.Parse("E4"), TimeSpan.FromMilliseconds(700))
        };

        Assert.True(await service.PlaySequenceAsync(sequence));

        var call = Assert.Single(runtime.Module.Calls, call => call.Identifier == "playSequence");
        var payload = Assert.IsType<AudioSequenceNote[]>(Assert.Single(call.Arguments));
        Assert.Equal(2, payload.Length);
        Assert.Equal("C4", payload[0].Note);
        Assert.Equal(0, payload[0].DelayMilliseconds);
        Assert.Equal("E4", payload[1].Note);
        Assert.Equal(700, payload[1].DelayMilliseconds);
    }

    [Fact]
    public async Task PlaySequenceAsync_BeforeInitialization_ReturnsMeaningfulFailure()
    {
        await using var service = new AudioPlaybackService(new FakeJavaScriptRuntime());

        Assert.False(await service.PlaySequenceAsync(
        [AudioSequenceNote.From(MusicalNote.Parse("C4"), TimeSpan.Zero)]));
        Assert.Equal(AudioPlaybackState.Uninitialized, service.State);
        Assert.NotNull(service.LastError);
    }

    [Fact]
    public async Task PlaySequenceAsync_NullSequence_ThrowsArgumentNullException()
    {
        await using var service = new AudioPlaybackService(new FakeJavaScriptRuntime());

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.PlaySequenceAsync(null!));
    }

    [Fact]
    public async Task InitializeAsync_WhenBrowserBlocksAudio_ReportsBlockedAndAllowsRetry()
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);
        runtime.Module.ContextState = "suspended";

        Assert.False(await service.InitializeAsync());
        Assert.Equal(AudioPlaybackState.Failed, service.State);
        Assert.Equal(AudioFailureReason.BrowserBlocked, service.FailureReason);
        Assert.NotNull(service.LastError);

        // A later attempt may succeed once the browser allows playback.
        runtime.Module.ContextState = "running";
        Assert.True(await service.InitializeAsync());
        Assert.Equal(AudioPlaybackState.Ready, service.State);
        Assert.Equal(2, runtime.Module.Calls.Count(call => call.Identifier == "initialize"));
    }

    [Fact]
    public async Task InitializeAsync_WhenInteropFails_ReportsUnavailableFailure()
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);
        runtime.Module.ThrowOnInitialize = true;

        Assert.False(await service.InitializeAsync());
        Assert.Equal(AudioPlaybackState.Failed, service.State);
        Assert.Equal(AudioFailureReason.Unavailable, service.FailureReason);

        runtime.Module.ThrowOnInitialize = false;
        Assert.True(await service.InitializeAsync());
    }

    [Fact]
    public async Task InitializeAsync_ConcurrentCalls_ShareSingleInitialization()
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);

        var first = service.InitializeAsync();
        var second = service.InitializeAsync();

        Assert.True(await first);
        Assert.True(await second);
        Assert.Single(runtime.Module.Calls, call => call.Identifier == "initialize");
    }

    [Fact]
    public async Task PlayNoteAsync_BeforeInitialization_ReportsNotInitializedReason()
    {
        await using var service = new AudioPlaybackService(new FakeJavaScriptRuntime());

        Assert.False(await service.PlayNoteAsync(MusicalNote.Parse("C4")));
        Assert.Equal(AudioPlaybackState.Uninitialized, service.State);
        Assert.Equal(AudioFailureReason.NotInitialized, service.FailureReason);
    }

    [Fact]
    public async Task PlayMetronomeClickAsync_MapsToInteropRequest()
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);
        await service.InitializeAsync();

        Assert.True(await service.PlayMetronomeClickAsync());

        Assert.Single(runtime.Module.Calls, call => call.Identifier == "playMetronomeClick");
    }

    [Fact]
    public async Task PlayNoteAsync_NullNote_ThrowsArgumentNullException()
    {
        await using var service = new AudioPlaybackService(new FakeJavaScriptRuntime());

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.PlayNoteAsync(null!));
    }

    [Theory]
    [InlineData("closed", AudioFailureReason.ContextClosed)]
    [InlineData("interrupted", AudioFailureReason.Unavailable)]
    [InlineData("unknown", AudioFailureReason.Unavailable)]
    [InlineData("blocked", AudioFailureReason.BrowserBlocked)]
    public async Task InitializeAsync_ClassifiesContextStateWithoutInventingPermissionDenial(string state, AudioFailureReason reason)
    {
        var runtime = new FakeJavaScriptRuntime();
        runtime.Module.ContextState = state;
        await using var service = new AudioPlaybackService(runtime);

        Assert.False(await service.InitializeAsync());
        Assert.Equal(reason, service.FailureReason);
        Assert.Contains(state, service.LastError);
        Assert.False(await service.PlayMetronomeClickAsync());
        Assert.Equal(reason, service.FailureReason);
    }

    [Fact]
    public async Task InitializeAsync_ExposesTransitionsAndSharesAnInFlightAttempt()
    {
        var runtime = new FakeJavaScriptRuntime();
        runtime.Module.InitializationGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var service = new AudioPlaybackService(runtime);
        var states = new List<AudioPlaybackState>();
        service.StateChanged += () => states.Add(service.State);

        var first = service.InitializeAsync();
        var second = service.InitializeAsync();
        Assert.Equal(AudioPlaybackState.Initializing, service.State);
        runtime.Module.InitializationGate.SetResult("running");

        Assert.True(await first);
        Assert.True(await second);
        Assert.Equal([AudioPlaybackState.Initializing, AudioPlaybackState.Ready], states);
        Assert.Single(runtime.Module.Calls, call => call.Identifier == "initialize");
    }

    [Fact]
    public async Task InitializeAsync_ReadyContextThatWasSuspendedIsResumedAgain()
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);
        await service.InitializeAsync();
        runtime.Module.ContextState = "suspended";
        Assert.False(await service.InitializeAsync());
        runtime.Module.ContextState = "running";
        Assert.True(await service.InitializeAsync());
        Assert.Equal(AudioFailureReason.None, service.FailureReason);
        Assert.Null(service.LastError);
    }

    [Theory]
    [InlineData("C4")]
    [InlineData("E4")]
    [InlineData("G4")]
    [InlineData("C5")]
    public async Task PlayNoteAsync_VerificationPitchesShareTheMetronomeService(string pitch)
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);
        await service.InitializeAsync();
        Assert.True(await service.PlayMetronomeClickAsync());
        Assert.True(await service.PlayNoteAsync(MusicalNote.Parse(pitch)));
        Assert.True(await service.PlayMetronomeClickAsync());
        Assert.Equal(pitch, Assert.Single(runtime.Module.Calls, call => call.Identifier == "playNote").Arguments[0]);
        Assert.Single(runtime.Imports);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cleanup_IsSafeWhenBrowserFailsAndDoesNotReuseDisposedModule(bool disconnected)
    {
        var runtime = new FakeJavaScriptRuntime();
        var service = new AudioPlaybackService(runtime);
        await service.InitializeAsync();
        runtime.Module.PlaybackError = disconnected ? new JSDisconnectedException("Circuit closed") : new JSException("Cleanup failed");

        await service.StopActiveNotesAsync();
        await service.StopActiveNotesAsync();
        await service.DisposeAsync();
        await service.DisposeAsync();
        Assert.True(runtime.Module.Disposed);
        Assert.False(await service.PlayNoteAsync(MusicalNote.Parse("C4")));
        Assert.False(await service.InitializeAsync());
    }

    [Fact]
    public async Task PlaybackError_PreservesDiagnosticAndAllowsReinitialization()
    {
        var runtime = new FakeJavaScriptRuntime();
        await using var service = new AudioPlaybackService(runtime);
        await service.InitializeAsync();
        runtime.Module.PlaybackError = new JSException("Audio output disconnected");
        Assert.False(await service.PlayMetronomeClickAsync());
        Assert.Contains("Audio output disconnected", service.LastError);
        Assert.Equal(AudioFailureReason.Unavailable, service.FailureReason);
        runtime.Module.PlaybackError = null;
        Assert.True(await service.InitializeAsync());
        Assert.True(await service.PlayMetronomeClickAsync());
    }

    [Fact]
    public async Task DisposeAsync_DuringInitialization_DoesNotBecomeReady()
    {
        var runtime = new FakeJavaScriptRuntime();
        runtime.Module.InitializationGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var service = new AudioPlaybackService(runtime);
        var initialization = service.InitializeAsync();
        await service.DisposeAsync();
        runtime.Module.InitializationGate.SetResult("running");
        Assert.False(await initialization);
        Assert.Equal(AudioPlaybackState.Failed, service.State);
        Assert.False(await service.PlayMetronomeClickAsync());
    }

    private sealed class FakeJavaScriptRuntime : IJSRuntime
    {
        public FakeModule Module { get; } = new();
        public List<string> Imports { get; } = [];

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            CancellationToken cancellationToken,
            object?[]? args)
        {
            if (identifier == "import")
            {
                Imports.Add((string)args![0]!);
                return ValueTask.FromResult((TValue)(object)Module);
            }

            throw new InvalidOperationException($"Unexpected JS runtime call '{identifier}'.");
        }
    }

    private sealed class FakeModule : IJSObjectReference
    {
        public string ContextState { get; set; } = "running";
        public bool ThrowOnInitialize { get; set; }
        public TaskCompletionSource<string>? InitializationGate { get; set; }
        public Exception? PlaybackError { get; set; }
        public bool Disposed { get; private set; }
        public List<InteropCall> Calls { get; } = [];

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            CancellationToken cancellationToken,
            object?[]? args)
        {
            Calls.Add(new InteropCall(identifier, args ?? []));
            if (identifier == "initialize" && ThrowOnInitialize)
            {
                throw new JSException("Tone.js could not be loaded.");
            }

            if (identifier == "initialize" && InitializationGate is not null)
                return new ValueTask<TValue>(AwaitInitialization<TValue>());

            if (identifier is "initialize" or "getState")
            {
                return ValueTask.FromResult((TValue)(object)ContextState);
            }

            if (PlaybackError is not null) throw PlaybackError;

            return ValueTask.FromResult(default(TValue)!);
        }

        private async Task<TValue> AwaitInitialization<TValue>() => (TValue)(object)await InitializationGate!.Task;

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed record InteropCall(string Identifier, IReadOnlyList<object?> Arguments);
}
