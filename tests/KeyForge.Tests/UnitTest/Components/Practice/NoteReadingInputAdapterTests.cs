using KeyForge.Components.Practice;

namespace KeyForge.Tests.UnitTest.Components.Practice;

public sealed class NoteReadingInputAdapterTests
{
    private static readonly DateTime StartedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void VirtualPianoSelection_CreatesNoteEventWithOctave()
    {
        var input = VirtualPianoInputAdapter.Create(
            MusicalNote.Parse("F#5"),
            StartedAt,
            StartedAt.AddMilliseconds(240));

        Assert.Equal(InputSource.VirtualPiano, input.Source);
        Assert.Equal(InputEventType.Note, input.Type);
        Assert.Equal("F#5", input.Value);
        Assert.Equal(TimeSpan.FromMilliseconds(240), input.Offset);
    }

    [Fact]
    public void KeyboardMapping_CreatesGenericComputerKeyboardEvent()
    {
        var input = NoteReadingKeyboardInputAdapter.TryCreate(
            "KeyK",
            repeat: false,
            isActive: true,
            StartedAt,
            StartedAt.AddMilliseconds(100));

        Assert.NotNull(input);
        Assert.Equal(InputSource.ComputerKeyboard, input.Source);
        Assert.Equal("C5", input.Value);
    }

    [Theory]
    [InlineData("KeyK", true, true)]
    [InlineData("KeyZ", false, true)]
    [InlineData("KeyK", false, false)]
    public void KeyboardMapping_RepeatInvalidOrInactiveInput_IsIgnored(
        string code,
        bool repeat,
        bool isActive)
    {
        var input = NoteReadingKeyboardInputAdapter.TryCreate(
            code,
            repeat,
            isActive,
            StartedAt,
            StartedAt.AddMilliseconds(100));

        Assert.Null(input);
    }
}
