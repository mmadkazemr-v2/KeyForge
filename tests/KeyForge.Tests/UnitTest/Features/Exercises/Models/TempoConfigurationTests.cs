namespace KeyForge.Tests.UnitTest.Features.Exercises.Models;

public sealed class TempoConfigurationTests
{
    [Fact]
    public void Validate_ValidConfiguration_Succeeds()
    {
        var configuration = new TempoConfiguration
        {
            DefaultBpm = 80,
            AllowUserChange = true,
            MinBpm = 50,
            MaxBpm = 120,
            Step = 5,
            SuggestedBpms = [60, 80, 100]
        };

        configuration.Validate();

        Assert.True(configuration.Supports(100));
        Assert.False(configuration.Supports(125));
    }

    [Theory]
    [InlineData(0, 50, 120, 5)]
    [InlineData(80, 90, 70, 5)]
    [InlineData(80, 50, 120, 0)]
    public void Validate_InvalidRangeOrStep_Throws(int defaultBpm, int minBpm, int maxBpm, int step)
    {
        var configuration = new TempoConfiguration
        {
            DefaultBpm = defaultBpm,
            MinBpm = minBpm,
            MaxBpm = maxBpm,
            Step = step
        };

        Assert.Throws<InvalidOperationException>(configuration.Validate);
    }

    [Fact]
    public void Validate_SuggestedBpmOutsideRange_Throws()
    {
        var configuration = new TempoConfiguration
        {
            DefaultBpm = 80,
            MinBpm = 50,
            MaxBpm = 120,
            SuggestedBpms = [40]
        };

        Assert.Throws<InvalidOperationException>(configuration.Validate);
    }
}
