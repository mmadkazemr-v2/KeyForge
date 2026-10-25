using KeyForge.Components.Practice;

namespace KeyForge.Tests.UnitTest.Components.Practice;

public sealed class MidiInputAdapterTests
{
    private static readonly DateTime ReceivedAt = new(2026, 1, 1, 12, 0, 1, DateTimeKind.Utc);

    [Theory]
    [InlineData(0x90, 60, 96, 1, "C4")]
    [InlineData(0x9F, 71, 127, 16, "B4")]
    public void NoteOn_PreservesNoteVelocityChannelAndBrowserMetadata(
        int status, int note, int velocity, int channel, string expectedNote)
    {
        var input = MidiInputAdapter.TryCreate(Message(status, note, velocity, 12_530, 5), ReceivedAt, 12_000);

        Assert.NotNull(input);
        Assert.Equal(InputEventType.Note, input.Type);
        Assert.Equal(MidiEventKind.NoteOn, input.MidiKind);
        Assert.Equal(expectedNote, input.Value);
        Assert.Equal(velocity, input.Velocity);
        Assert.Equal(channel, input.MidiChannel);
        Assert.Equal(TimeSpan.FromMilliseconds(530), input.Offset);
        Assert.Equal(12_530, input.BrowserTimestampMilliseconds);
        Assert.Equal(5, input.Sequence);
        Assert.Equal("controller-1", input.DeviceId);
        Assert.Equal("Alesis V61 MKII", input.DeviceName);
    }

    [Theory]
    [InlineData(0x80, 64, 64)]
    [InlineData(0x90, 0, 0)]
    public void BothNoteOffForms_NormalizeToNoteOff(int status, int velocity, int expectedReleaseVelocity)
    {
        var input = MidiInputAdapter.TryCreate(Message(status, 60, velocity, 12_100, 1), ReceivedAt, 12_000);

        Assert.NotNull(input);
        Assert.Equal(InputEventType.NoteOff, input.Type);
        Assert.Equal(MidiEventKind.NoteOff, input.MidiKind);
        Assert.Equal("C4", input.Value);
        Assert.Equal(expectedReleaseVelocity, input.MidiData2);
        Assert.Null(input.Velocity);
    }

    [Theory]
    [InlineData(0xB2, 64, 127, MidiEventKind.ControlChange, InputEventType.ControlChange, 3)]
    [InlineData(0xE0, 0, 64, MidiEventKind.PitchBend, InputEventType.PitchBend, 1)]
    public void ControlChangeAndPitchBend_PreserveKindChannelAndData(
        int status, int data1, int data2, MidiEventKind kind, InputEventType eventType, int channel)
    {
        var input = MidiInputAdapter.TryCreate(Message(status, data1, data2, 12_050, 2), ReceivedAt, 12_000);

        Assert.NotNull(input);
        Assert.Equal(kind, input.MidiKind);
        Assert.Equal(eventType, input.Type);
        Assert.Equal(channel, input.MidiChannel);
        Assert.Equal(data1, input.MidiData1);
        Assert.Equal(data2, input.MidiData2);
    }

    [Theory]
    [InlineData(0x90, 128, 80)]
    [InlineData(0x90, 60, 128)]
    [InlineData(0xF0, 1, 2)]
    public void InvalidMidiData_IsRejected(int status, int data1, int data2) =>
        Assert.Null(MidiInputAdapter.TryCreate(Message(status, data1, data2, 12_100, 1), ReceivedAt, 12_000));

    [Fact]
    public void RuntimeOffset_UsesBrowserTimestampRegardlessOfServerDeliveryDelay()
    {
        var message = Message(0x90, 60, 90, 12_500, 1);
        var immediateDelivery = MidiInputAdapter.TryCreate(message, ReceivedAt, 12_000);
        var delayedDelivery = MidiInputAdapter.TryCreate(message, ReceivedAt.AddMinutes(3), 12_000);

        Assert.NotNull(immediateDelivery);
        Assert.NotNull(delayedDelivery);
        Assert.Equal(TimeSpan.FromMilliseconds(500), immediateDelivery.Offset);
        Assert.Equal(immediateDelivery.Offset, delayedDelivery.Offset);
        Assert.NotEqual(immediateDelivery.Timestamp, delayedDelivery.Timestamp);
    }

    [Fact]
    public void RuntimeMidiStream_PreservesRapidBrowserSequenceOrder()
    {
        var definition = new NoteReadingExerciseDefinition { Id = "reading", Clef = ClefType.Treble, Range = "C4-C4" };
        var runtime = new RuntimeExercise(definition,
            new ExerciseChallenge { ExerciseId = definition.Id, ExerciseType = ExerciseType.NoteReading, ExpectedInputs = [] }, ReceivedAt);
        var notes = new[] { 60, 62, 64, 65, 67 };
        foreach (var (note, index) in notes.Select((value, index) => (value, index)))
        {
            var input = MidiInputAdapter.TryCreate(Message(0x90, note, 90, 12_010 + index, index + 1),
                ReceivedAt.AddMilliseconds(10), 12_000)!;
            runtime.CaptureMidiEvent(input);
        }

        Assert.Equal(new long[] { 1, 2, 3, 4, 5 }, runtime.MidiEvents.Select(input => input.Sequence!.Value));
        Assert.Equal(new[] { "C4", "D4", "E4", "F4", "G4" }, runtime.MidiEvents.Select(input => input.Value));
    }

    [Theory]
    [InlineData(63, false)]
    [InlineData(64, true)]
    [InlineData(127, true)]
    public void SustainControlChange_MapsPedalThreshold(int value, bool expectedDown)
    {
        var midiEvent = MidiInputAdapter.TryCreatePresentationEvent(Message(0xB0, 64, value, 12_100, 1));

        Assert.NotNull(midiEvent);
        Assert.Equal(MidiPresentationEventType.SustainChanged, midiEvent.Type);
        Assert.Equal(expectedDown, midiEvent.IsSustainDown);
    }

    [Fact]
    public void NoteOnAndNoteOff_CreatePresentationAndRuntimeEvents()
    {
        var noteOn = Message(0x90, 60, 100, 12_100, 1);
        var noteOff = Message(0x80, 60, 64, 12_200, 2);

        Assert.Equal(MidiPresentationEventType.NoteOn, MidiInputAdapter.TryCreatePresentationEvent(noteOn)!.Type);
        Assert.Equal(MidiPresentationEventType.NoteOff, MidiInputAdapter.TryCreatePresentationEvent(noteOff)!.Type);
        Assert.NotNull(MidiInputAdapter.TryCreate(noteOn, ReceivedAt, 12_000));
        Assert.NotNull(MidiInputAdapter.TryCreate(noteOff, ReceivedAt, 12_000));
    }

    [Fact]
    public async Task Router_OnlyDispatchesToLatestActiveWorkspaceAndStopsAfterDisposal()
    {
        var router = new MidiInputRouter();
        router.SetSelectedDevice("controller-1", true);
        var firstCount = 0;
        var secondCount = 0;
        using var first = router.Register(_ => { firstCount++; return Task.CompletedTask; });
        var second = router.Register(_ => { secondCount++; return Task.CompletedTask; });

        await router.DispatchAsync(Message(0x90, 60, 80, 12_100, 1));
        Assert.Equal(0, firstCount);
        Assert.Equal(1, secondCount);

        router.SetSelectedDevice("controller-1", false);
        await router.DispatchAsync(Message(0x90, 60, 80, 12_100, 2));
        Assert.Equal(1, secondCount);

        router.SetSelectedDevice("controller-1", true);
        second.Dispose();
        await router.DispatchAsync(Message(0x90, 60, 80, 12_100, 3));
        Assert.Equal(1, secondCount);
    }

    [Fact]
    public async Task Router_DeviceStateAndClock_KeepSelectedIdentityAndBrowserOrigin()
    {
        var router = new MidiInputRouter();
        using var clock = router.RegisterBrowserClock(() => ValueTask.FromResult(1_200d));
        var transitions = new List<(string? DeviceId, bool Connected)>();
        router.SelectedDeviceStateChanged += (deviceId, connected) => transitions.Add((deviceId, connected));

        Assert.Equal(1_200d, await router.CaptureBrowserTimestampAsync());
        router.SetTimelineOrigin(1_200);
        Assert.Equal(1_200, router.TimelineOriginMilliseconds);
        Assert.True(router.Accepts(Message(0x90, 60, 80, 1_250, 1)) == false);

        router.SetSelectedDevice("controller-1", true);
        Assert.True(router.Accepts(Message(0x90, 60, 80, 1_250, 1)));
        Assert.False(router.Accepts(new MidiInputMessage
        {
            DeviceId = "unrelated", Status = 0x90, Data1 = 60, Data2 = 80,
            TimestampMilliseconds = 1_250, Sequence = 2
        }));
        router.SetSelectedDevice("controller-1", false);
        Assert.False(router.Accepts(Message(0x90, 60, 80, 1_250, 3)));
        Assert.Equal(new (string? DeviceId, bool Connected)[]
        {
            ("controller-1", true), ("controller-1", false)
        }, transitions);
    }

    private static MidiInputMessage Message(int status, int data1, int data2, double timestamp, long sequence) => new()
    {
        DeviceId = "controller-1",
        DeviceName = "Alesis V61 MKII",
        Status = status,
        Data1 = data1,
        Data2 = data2,
        TimestampMilliseconds = timestamp,
        Sequence = sequence
    };
}
