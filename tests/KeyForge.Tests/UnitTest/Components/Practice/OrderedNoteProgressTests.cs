using KeyForge.Components.Practice;

namespace KeyForge.Tests.UnitTest.Components.Practice;

public sealed class OrderedNoteProgressTests
{
    private static readonly string[] Ascending = ["C4", "D4", "E4", "F4", "G4", "A4", "B4", "C5"];
    private static readonly string[] Descending = ["C5", "B4", "A4", "G4", "F4", "E4", "D4", "C4"];

    [Theory]
    [MemberData(nameof(ScaleDirections))]
    public void CorrectCmajorSequence_AdvancesOneExpectedNoteAtATime(string[] notes)
    {
        var progress = new OrderedNoteProgress(notes);

        foreach (var note in notes)
        {
            Assert.Equal(note, progress.CurrentExpectedNote);
            Assert.True(progress.TryAccept(note));
        }

        Assert.True(progress.IsComplete);
        Assert.Null(progress.CurrentExpectedNote);
        Assert.False(progress.TryAccept(notes[^1]));
    }

    [Theory]
    [InlineData("D4")]
    [InlineData("C5")]
    public void IncorrectPitchOrOctave_DoesNotAdvance(string incorrectNote)
    {
        var progress = new OrderedNoteProgress(Ascending);

        Assert.False(progress.TryAccept(incorrectNote));
        Assert.Equal(0, progress.CompletedCount);
        Assert.Equal("C4", progress.CurrentExpectedNote);
        Assert.True(progress.TryAccept("C4"));
        Assert.Equal("D4", progress.CurrentExpectedNote);
    }

    [Theory]
    [MemberData(nameof(ScaleDirections))]
    public void Reset_RestartsAtFirstExpectedNote(string[] notes)
    {
        var progress = new OrderedNoteProgress(notes);
        Assert.True(progress.TryAccept(notes[0]));

        progress.Reset();

        Assert.Equal(0, progress.CompletedCount);
        Assert.False(progress.IsComplete);
        Assert.Equal(notes[0], progress.CurrentExpectedNote);
    }

    public static IEnumerable<object[]> ScaleDirections()
    {
        yield return [Ascending];
        yield return [Descending];
    }
}
