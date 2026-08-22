using KeyForge.Components.Music;

namespace KeyForge.Tests.UnitTest.Components.Music;

/// <summary>
/// MusicStaff preserves its KeyForge-facing note/clef contract by delegating
/// those values to the shared VexFlow mapping path.
/// </summary>
public sealed class MusicStaffVexFlowMappingTests
{
    [Theory]
    [InlineData(ClefType.Treble, "E4", "treble", "e/4")]
    [InlineData(ClefType.Bass, "G2", "bass", "g/2")]
    public void MusicStaffContract_MapsTrebleAndBassToVexFlow(
        ClefType clef,
        string noteValue,
        string expectedClef,
        string expectedKey)
    {
        var mapped = VexFlowNotationMapper.TryMap(
            clef,
            MusicalNote.Parse(noteValue),
            out var request);

        Assert.True(mapped);
        Assert.NotNull(request);
        Assert.Equal(expectedClef, request.Clef);
        Assert.Equal(expectedKey, request.NoteKey);
    }

    [Theory]
    [InlineData(ClefType.Treble, "C4")]
    [InlineData(ClefType.Bass, "C2")]
    public void MusicStaffContract_LedgerLineCapableNotesRemainValid(
        ClefType clef,
        string noteValue)
    {
        var mapped = VexFlowNotationMapper.TryMap(
            clef,
            MusicalNote.Parse(noteValue),
            out var request);

        Assert.True(mapped);
        Assert.NotNull(request);
    }

    [Fact]
    public void MusicStaffContract_UnsupportedGrandStaffFailsGracefully()
    {
        var mapped = VexFlowNotationMapper.TryMap(
            ClefType.Grand,
            MusicalNote.Parse("C4"),
            out var request);

        Assert.False(mapped);
        Assert.Null(request);
    }
}
