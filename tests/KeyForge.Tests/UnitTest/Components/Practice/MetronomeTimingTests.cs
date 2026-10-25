using KeyForge.Components.Practice;

namespace KeyForge.Tests.UnitTest.Components.Practice;

public sealed class MetronomeTimingTests
{
    [Theory]
    [InlineData(60, 1000)]
    [InlineData(80, 750)]
    [InlineData(120, 500)]
    [InlineData(240, 250)]
    public void GetBeatInterval_UsesSelectedBpm(int bpm, int expectedMilliseconds)
    {
        var interval = MetronomeTiming.GetBeatInterval(bpm);

        Assert.Equal(expectedMilliseconds, interval.TotalMilliseconds);
    }

    [Fact]
    public void GetBeatInterval_InvalidBpm_FailsClearly()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MetronomeTiming.GetBeatInterval(0));
    }

    [Fact]
    public void GetCountInBeats_ReturnsConfiguredCount()
    {
        Assert.Equal([1, 2, 3, 4], MetronomeTiming.GetCountInBeats(4));
        Assert.Empty(MetronomeTiming.GetCountInBeats(0));
    }

    [Fact]
    public void GetCountInBeats_NegativeCount_FailsClearly()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MetronomeTiming.GetCountInBeats(-1));
    }
}
