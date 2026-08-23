namespace KeyForge.Components.Audio;

/// <summary>A presentation-layer request to play one KeyForge musical note.</summary>
public sealed class AudioNoteRequest
{
    public required string Note { get; init; }

    public static AudioNoteRequest From(MusicalNote note)
    {
        ArgumentNullException.ThrowIfNull(note);
        return new AudioNoteRequest { Note = note.Value };
    }
}
