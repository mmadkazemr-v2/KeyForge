namespace KeyForge.Components.Audio;

/// <summary>One note within a scheduled playback sequence, played after its delay elapses.</summary>
public sealed class AudioSequenceNote
{
    public required string Note { get; init; }
    public double DelayMilliseconds { get; init; }

    public static AudioSequenceNote From(MusicalNote note, TimeSpan delay)
    {
        ArgumentNullException.ThrowIfNull(note);
        if (delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), "Delays cannot be negative.");
        }

        return new AudioSequenceNote { Note = note.Value, DelayMilliseconds = delay.TotalMilliseconds };
    }
}
