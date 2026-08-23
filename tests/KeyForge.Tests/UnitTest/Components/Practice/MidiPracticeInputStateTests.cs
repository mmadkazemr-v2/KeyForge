using KeyForge.Components.Practice;

namespace KeyForge.Tests.UnitTest.Components.Practice;

public sealed class MidiPracticeInputStateTests
{
    [Fact]
    public void NoteOnThenNoteOff_UpdatesActiveNotePresentation()
    {
        var state = new MidiPracticeInputState();
        var note = MusicalNote.Parse("C4");

        state.Apply(new MidiPresentationEvent { Type = MidiPresentationEventType.NoteOn, Note = note, Velocity = 90 });
        Assert.Contains(note, state.ActiveNotes);
        Assert.Equal(90, state.LastVelocity);

        state.Apply(new MidiPresentationEvent { Type = MidiPresentationEventType.NoteOff, Note = note });
        Assert.DoesNotContain(note, state.ActiveNotes);
    }

    [Fact]
    public void SustainDown_KeepsReleasedNoteActiveUntilPedalUp()
    {
        var state = new MidiPracticeInputState();
        var note = MusicalNote.Parse("E4");
        state.Apply(new MidiPresentationEvent { Type = MidiPresentationEventType.NoteOn, Note = note, Velocity = 60 });
        state.Apply(new MidiPresentationEvent { Type = MidiPresentationEventType.SustainChanged, IsSustainDown = true });
        state.Apply(new MidiPresentationEvent { Type = MidiPresentationEventType.NoteOff, Note = note });

        Assert.True(state.IsSustainDown);
        Assert.Contains(note, state.ActiveNotes);

        state.Apply(new MidiPresentationEvent { Type = MidiPresentationEventType.SustainChanged, IsSustainDown = false });
        Assert.False(state.IsSustainDown);
        Assert.DoesNotContain(note, state.ActiveNotes);
    }

    [Theory]
    [InlineData(1, MidiVelocityLevel.Soft)]
    [InlineData(42, MidiVelocityLevel.Soft)]
    [InlineData(43, MidiVelocityLevel.Medium)]
    [InlineData(84, MidiVelocityLevel.Medium)]
    [InlineData(85, MidiVelocityLevel.Strong)]
    public void VelocityLevel_IsClassifiedForPresentationOnly(int velocity, MidiVelocityLevel level)
    {
        Assert.Equal(level, MidiPracticeInputState.GetVelocityLevel(velocity));
    }

    [Fact]
    public void Reset_ClearsDeviceDisconnectOrNavigationPresentationState()
    {
        var state = new MidiPracticeInputState();
        state.Apply(new MidiPresentationEvent { Type = MidiPresentationEventType.NoteOn, Note = MusicalNote.Parse("G4"), Velocity = 100 });
        state.Apply(new MidiPresentationEvent { Type = MidiPresentationEventType.SustainChanged, IsSustainDown = true });

        state.Reset();

        Assert.Empty(state.ActiveNotes);
        Assert.False(state.IsSustainDown);
        Assert.Null(state.LastNote);
        Assert.Null(state.LastVelocity);
    }
}
