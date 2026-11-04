namespace KeyForge.Components.Practice;

/// <summary>
/// Tracks exact-pitch progress for an ordered note exercise.
/// </summary>
public sealed class OrderedNoteProgress
{
    private readonly IReadOnlyList<string> _expectedNotes;

    public OrderedNoteProgress(IEnumerable<string> expectedNotes)
    {
        ArgumentNullException.ThrowIfNull(expectedNotes);
        var notes = expectedNotes.ToArray();
        if (notes.Length == 0 || notes.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("An ordered note exercise must contain valid expected notes.", nameof(expectedNotes));
        }

        _expectedNotes = notes;
    }

    public int CompletedCount { get; private set; }
    public bool IsComplete => CompletedCount == _expectedNotes.Count;
    public string? CurrentExpectedNote => IsComplete ? null : _expectedNotes[CompletedCount];

    public bool TryAccept(string note)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(note);
        if (IsComplete || !string.Equals(CurrentExpectedNote, note, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        CompletedCount++;
        return true;
    }

    public void Reset() => CompletedCount = 0;
}