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
    public async Task InitializeAsync_WhenBrowserRejectsAudio_SetsFailureState()
    {
        var runtime = new FakeJavaScriptRuntime { InitializeResult = false };
        await using var service = new AudioPlaybackService(runtime);

        Assert.False(await service.InitializeAsync());
        Assert.Equal(AudioPlaybackState.Failed, service.State);
        Assert.NotNull(service.LastError);
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

    private sealed class FakeJavaScriptRuntime : IJSRuntime
    {
        public FakeModule Module { get; } = new();
        public bool InitializeResult { get; init; } = true;
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
                Module.InitializeResult = InitializeResult;
                return ValueTask.FromResult((TValue)(object)Module);
            }

            throw new InvalidOperationException($"Unexpected JS runtime call '{identifier}'.");
        }
    }

    private sealed class FakeModule : IJSObjectReference
    {
        public bool InitializeResult { get; set; }
        public List<InteropCall> Calls { get; } = [];

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            CancellationToken cancellationToken,
            object?[]? args)
        {
            Calls.Add(new InteropCall(identifier, args ?? []));
            if (identifier == "initialize")
            {
                return ValueTask.FromResult((TValue)(object)InitializeResult);
            }

            return ValueTask.FromResult(default(TValue)!);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed record InteropCall(string Identifier, IReadOnlyList<object?> Arguments);
}
