using KeyForge.Components.Practice;

namespace KeyForge.Tests.UnitTest.Components.Practice;

public sealed class RhythmKeyboardInputAdapterTests
{
    private static readonly DateTime TimelineStart =
        new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ActiveKPress_ProducesGenericBeatInput()
    {
        var input = RhythmKeyboardInputAdapter.Create(
            "KeyK",
            isRepeat: false,
            isActive: true,
            elapsedMilliseconds: 245.5,
            TimelineStart);

        Assert.NotNull(input);
        Assert.Equal(InputSource.ComputerKeyboard, input.Source);
        Assert.Equal(InputEventType.Beat, input.Type);
        Assert.Equal("beat", input.Value);
        Assert.Equal(245.5, input.Offset.TotalMilliseconds, precision: 6);
        Assert.Equal(TimelineStart.AddMilliseconds(245.5), input.Timestamp);
    }

    [Fact]
    public void Create_KeyRepeat_IsIgnored()
    {
        Assert.Null(RhythmKeyboardInputAdapter.Create(
            "KeyK", true, true, 100, TimelineStart));
    }

    [Theory]
    [InlineData("KeyJ")]
    [InlineData("Space")]
    [InlineData("k")]
    public void Create_UnrelatedKey_IsIgnored(string code)
    {
        Assert.Null(RhythmKeyboardInputAdapter.Create(
            code, false, true, 100, TimelineStart));
    }

    [Fact]
    public void Create_DuringCountdown_IsIgnored()
    {
        Assert.Null(RhythmKeyboardInputAdapter.Create(
            "KeyK", false, false, 100, TimelineStart));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Create_InvalidElapsedTime_IsIgnored(double elapsedMilliseconds)
    {
        Assert.Null(RhythmKeyboardInputAdapter.Create(
            "KeyK", false, true, elapsedMilliseconds, TimelineStart));
    }
}
