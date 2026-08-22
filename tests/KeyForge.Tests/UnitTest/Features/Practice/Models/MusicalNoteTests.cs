namespace KeyForge.Tests.UnitTest.Features.Practice.Models;

public sealed class MusicalNoteTests
{
    [Theory]
    [InlineData("C4", "C", 4, 60, false)]
    [InlineData("f#3", "F#", 3, 54, true)]
    [InlineData(" A5 ", "A", 5, 81, false)]
    public void Parse_PreservesPitchAndOctave(
        string value,
        string name,
        int octave,
        int midiNumber,
        bool isBlackKey)
    {
        var note = MusicalNote.Parse(value);

        Assert.Equal(name, note.Name);
        Assert.Equal(octave, note.Octave);
        Assert.Equal(midiNumber, note.MidiNumber);
        Assert.Equal(isBlackKey, note.IsBlackKey);
    }

    [Fact]
    public void FromMidiNumber_RoundTripsOctave()
    {
        var note = MusicalNote.FromMidiNumber(72);

        Assert.Equal("C5", note.Value);
        Assert.Equal(72, note.MidiNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("H4")]
    [InlineData("C#")]
    public void TryParse_InvalidValue_ReturnsFalse(string value)
    {
        Assert.False(MusicalNote.TryParse(value, out _));
    }
}
