using KeyForge.Components.Music;

namespace KeyForge.Tests.UnitTest.Components.Music;

public sealed class PianoKeyboardLayoutTests
{
    [Fact]
    public void Create_ChromaticOctave_PreservesNotesAndOctaves()
    {
        var keys = PianoKeyboardLayout.Create(MusicalNote.Parse("C4"), MusicalNote.Parse("C5"));

        Assert.Equal(13, keys.Count);
        Assert.Equal("C4", keys[0].Note.Value);
        Assert.Equal("C#4", keys[1].Note.Value);
        Assert.Equal("C5", keys[^1].Note.Value);
        Assert.Equal(8, keys.Count(key => !key.Note.IsBlackKey));
    }

    [Fact]
    public void Create_BlackKeys_HavePositionsBetweenWhiteKeys()
    {
        var keys = PianoKeyboardLayout.Create(MusicalNote.Parse("C4"), MusicalNote.Parse("E4"));

        var cSharp = Assert.Single(keys, key => key.Note.Value == "C#4");
        Assert.Equal(0, cSharp.WhiteKeyIndex);
        Assert.InRange(cSharp.PositionPercent, 33.3, 33.4);
    }
}
