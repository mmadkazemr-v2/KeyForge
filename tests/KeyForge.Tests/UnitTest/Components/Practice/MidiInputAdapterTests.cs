using KeyForge.Components.Practice;

namespace KeyForge.Tests.UnitTest.Components.Practice;

public sealed class MidiInputAdapterTests
{
    private static readonly DateTime StartedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(60, "C4")]
    [InlineData(71, "B4")]
    [InlineData(72, "C5")]
    public void NoteOn_MapsMidiNumberToOctaveAwareMusicalNote(int midiNote, string expectedNote)
    {
        var input = MidiInputAdapter.TryCreate(Message(0x90, midiNote, 96), StartedAt, StartedAt.AddMilliseconds(80));

        Assert.NotNull(input);
        Assert.Equal(InputSource.Midi, input.Source);
        Assert.Equal(InputEventType.Note, input.Type);
        Assert.Equal(expectedNote, input.Value);
        Assert.Equal(96, input.Velocity);
    }

    [Theory]
    [InlineData(0x80, 60, 64)]
    [InlineData(0x90, 60, 0)]
    [InlineData(0xB0, 64, 127)]
    [InlineData(0x90, 128, 80)]
    public void NoteOffOrUnsupportedMessage_IsIgnored(int status, int data1, int data2)
    {
        var input = MidiInputAdapter.TryCreate(Message(status, data1, data2), StartedAt, StartedAt.AddMilliseconds(80));

        Assert.Null(input);
    }

    [Fact]
    public void BeatInput_OnlyMapsWhilePracticeIsActive()
    {
        var message = Message(0x90, 60, 100);

        Assert.Null(MidiInputAdapter.TryCreateBeat(message, false, StartedAt, StartedAt.AddMilliseconds(100)));

        var input = MidiInputAdapter.TryCreateBeat(message, true, StartedAt, StartedAt.AddMilliseconds(100));
        Assert.NotNull(input);
        Assert.Equal(InputEventType.Beat, input.Type);
        Assert.Equal(InputSource.Midi, input.Source);
        Assert.Equal(TimeSpan.FromMilliseconds(100), input.Offset);
    }

    [Fact]
    public async Task Router_OnlyDispatchesToLatestActiveWorkspaceAndStopsAfterDisposal()
    {
        var router = new MidiInputRouter();
        var firstCount = 0;
        var secondCount = 0;
        using var first = router.Register(_ => { firstCount++; return Task.CompletedTask; });
        var second = router.Register(_ => { secondCount++; return Task.CompletedTask; });

        await router.DispatchAsync(Message(0x90, 60, 80));
        Assert.Equal(0, firstCount);
        Assert.Equal(1, secondCount);

        second.Dispose();
        await router.DispatchAsync(Message(0x90, 60, 80));
        Assert.Equal(1, secondCount);
    }

    private static MidiInputMessage Message(int status, int data1, int data2) => new()
    {
        DeviceId = "device-1",
        Status = status,
        Data1 = data1,
        Data2 = data2,
        TimestampMilliseconds = 100
    };
}
