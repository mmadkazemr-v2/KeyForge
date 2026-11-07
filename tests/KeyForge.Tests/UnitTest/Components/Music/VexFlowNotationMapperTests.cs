using KeyForge.Components.Music;

namespace KeyForge.Tests.UnitTest.Components.Music;

public sealed class VexFlowNotationMapperTests
{
    [Theory]
    [InlineData("C3", "bass")]
    [InlineData("C5", "treble")]
    public void GrandStaffChoosesTheWrittenStaffWithoutHandInference(string pitch, string expectedClef)
    {
        Assert.True(VexFlowNotationMapper.TryMap(ClefType.Grand, MusicalNote.Parse(pitch), out var request));
        Assert.True(request!.GrandStaff);
        Assert.Equal(expectedClef, request.Clef);
    }

    [Theory]
    [InlineData(RhythmDuration.Whole, false, "w", false)]
    [InlineData(RhythmDuration.Half, false, "h", false)]
    [InlineData(RhythmDuration.Quarter, false, "q", false)]
    [InlineData(RhythmDuration.Eighth, false, "8", false)]
    [InlineData(RhythmDuration.Sixteenth, false, "16", false)]
    [InlineData(RhythmDuration.DottedHalf, false, "hd", true)]
    [InlineData(RhythmDuration.DottedQuarter, false, "qd", true)]
    [InlineData(RhythmDuration.DottedEighth, false, "8d", true)]
    [InlineData(RhythmDuration.Quarter, true, "qr", false)]
    [InlineData(RhythmDuration.DottedQuarter, true, "qdr", true)]
    public void MapRhythm_UsesVexFlowDurationsRestsAndDots(
        RhythmDuration duration, bool rest, string token, bool dotted)
    {
        var rhythm = new RhythmChallengeDetails
        {
            Duration = TimeSpan.FromSeconds(4),
            PulseOffsets = [],
            Events = [new RhythmTimelineEvent
            {
                Duration = duration, IsRest = rest, MeasureNumber = 1,
                StartOffset = TimeSpan.Zero, Length = TimeSpan.FromSeconds(1), StartSixteenthUnits = 0
            }]
        };

        var request = VexFlowNotationMapper.MapRhythm(rhythm, "4/4");

        var item = Assert.Single(Assert.Single(request.RhythmMeasures!));
        Assert.Equal(token, item.Duration);
        Assert.Equal(dotted, item.Dotted);
        Assert.Equal("4/4", request.TimeSignature);
        Assert.Equal("percussion", request.Clef);
    }

    [Fact]
    public void TryMap_TrebleClef_MapsComponentParametersToInteropInput()
    {
        var mapped = VexFlowNotationMapper.TryMap(
            ClefType.Treble,
            MusicalNote.Parse("C4"),
            out var request);

        Assert.True(mapped);
        Assert.NotNull(request);
        Assert.Equal("treble", request.Clef);
        Assert.Equal("c/4", request.NoteKey);
    }

    [Fact]
    public void TryMap_BassClef_MapsComponentParametersToInteropInput()
    {
        var mapped = VexFlowNotationMapper.TryMap(
            ClefType.Bass,
            MusicalNote.Parse("A2"),
            out var request);

        Assert.True(mapped);
        Assert.NotNull(request);
        Assert.Equal("bass", request.Clef);
        Assert.Equal("a/2", request.NoteKey);
    }

    [Theory]
    [InlineData("C2", "c/2")]
    [InlineData("G5", "g/5")]
    public void TryMap_NoteAndOctave_ArePreserved(string value, string expectedKey)
    {
        var mapped = VexFlowNotationMapper.TryMap(
            ClefType.Treble,
            MusicalNote.Parse(value),
            out var request);

        Assert.True(mapped);
        Assert.Equal(expectedKey, request!.NoteKey);
    }

    [Fact]
    public void TryMap_GrandStaffShowsBothClefs()
    {
        var mapped = VexFlowNotationMapper.TryMap(
            ClefType.Grand,
            MusicalNote.Parse("C4"),
            out var request);

        Assert.True(mapped);
        Assert.True(request!.GrandStaff);
    }

    [Fact]
    public void TryMap_SharpIncludesExplicitAccidental()
    {
        var mapped = VexFlowNotationMapper.TryMap(
            ClefType.Treble,
            MusicalNote.Parse("F#4"),
            out var request);

        Assert.True(mapped);
        Assert.Equal("#", request!.Accidental);
        Assert.Equal("f#/4", request.NoteKey);
    }

    [Fact]
    public void TryMap_MissingNote_FailsSafely()
    {
        Assert.False(VexFlowNotationMapper.TryMap(ClefType.Treble, null, out var request));
        Assert.Null(request);
    }
}
