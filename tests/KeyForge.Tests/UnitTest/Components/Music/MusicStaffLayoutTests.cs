using KeyForge.Components.Music;

namespace KeyForge.Tests.UnitTest.Components.Music;

public sealed class MusicStaffLayoutTests
{
    [Fact]
    public void Create_TrebleBottomLine_MapsE4ToBottomLine()
    {
        var layout = MusicStaffLayout.Create(MusicalNote.Parse("E4"), ClefType.Treble);

        Assert.Equal(0, layout.StaffOffset);
        Assert.Equal(MusicStaffLayout.BottomLineY, layout.NoteY);
        Assert.Empty(layout.LedgerLineYs);
    }

    [Fact]
    public void Create_BassBottomLine_MapsG2ToBottomLine()
    {
        var layout = MusicStaffLayout.Create(MusicalNote.Parse("G2"), ClefType.Bass);

        Assert.Equal(ClefType.Bass, layout.Clef);
        Assert.Equal(0, layout.StaffOffset);
        Assert.Equal(MusicStaffLayout.BottomLineY, layout.NoteY);
    }

    [Theory]
    [InlineData("C4", ClefType.Treble, 1)]
    [InlineData("C6", ClefType.Treble, 2)]
    [InlineData("E2", ClefType.Bass, 1)]
    public void Create_NoteOutsideStaff_AddsRequiredLedgerLines(
        string value,
        ClefType clef,
        int expectedLines)
    {
        var layout = MusicStaffLayout.Create(MusicalNote.Parse(value), clef);

        Assert.Equal(expectedLines, layout.LedgerLineYs.Count);
    }
}
