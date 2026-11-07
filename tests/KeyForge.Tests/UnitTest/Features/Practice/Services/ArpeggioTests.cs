namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class ArpeggioTests
{
    [Theory]
    [InlineData(TriadQuality.Major, 0, "C4,E4,G4,C5")]
    [InlineData(TriadQuality.Minor, 0, "C4,D#4,G4,C5")]
    [InlineData(TriadQuality.Major, 1, "E4,G4,C5,E5")]
    [InlineData(TriadQuality.Minor, 2, "G4,C5,D#5,G5")]
    public void TriadsAndInversionsGenerateExplicitOctaves(TriadQuality quality, int inversion, string expected) =>
        Assert.Equal(expected.Split(','), PitchSequenceBuilder.BuildArpeggio(new ArpeggioConfiguration { Root = "C4", Quality = quality, Inversion = inversion }, "test"));

    [Fact]
    public void MultiOctaveDirectionAndBoundsAreValidated()
    {
        var config = new ArpeggioConfiguration { Root = "C4", Octaves = 2, Direction = ScaleDirection.AscendingDescending };
        var pitches = PitchSequenceBuilder.BuildArpeggio(config, "test");
        Assert.Equal(13, pitches.Count);
        Assert.Equal("C6", pitches[6]);
        Assert.Equal("C4", pitches[^1]);
        config.Root = "G9";
        Assert.Throws<InvalidOperationException>(() => PitchSequenceBuilder.BuildArpeggio(config, "test"));
        config.Inversion = 3;
        Assert.Throws<InvalidOperationException>(() => PitchSequenceBuilder.BuildArpeggio(config, "test"));
    }
}
