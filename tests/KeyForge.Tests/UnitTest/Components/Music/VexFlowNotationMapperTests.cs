using KeyForge.Components.Music;

namespace KeyForge.Tests.UnitTest.Components.Music;

public sealed class VexFlowNotationMapperTests
{
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
    public void TryMap_GrandStaff_FailsSafely()
    {
        var mapped = VexFlowNotationMapper.TryMap(
            ClefType.Grand,
            MusicalNote.Parse("C4"),
            out var request);

        Assert.False(mapped);
        Assert.Null(request);
    }

    [Fact]
    public void TryMap_AccidentalOutsideFoundationScope_FailsSafely()
    {
        var mapped = VexFlowNotationMapper.TryMap(
            ClefType.Treble,
            MusicalNote.Parse("F#4"),
            out var request);

        Assert.False(mapped);
        Assert.Null(request);
    }

    [Fact]
    public void TryMap_MissingNote_FailsSafely()
    {
        Assert.False(VexFlowNotationMapper.TryMap(ClefType.Treble, null, out var request));
        Assert.Null(request);
    }
}
