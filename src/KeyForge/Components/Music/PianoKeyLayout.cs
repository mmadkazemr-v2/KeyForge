namespace KeyForge.Components.Music;

public sealed class PianoKeyLayout
{
    public required MusicalNote Note { get; init; }
    public required int WhiteKeyIndex { get; init; }
    public required double PositionPercent { get; init; }
}

public static class PianoKeyboardLayout
{
    public static IReadOnlyList<PianoKeyLayout> Create(MusicalNote first, MusicalNote last)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(last);

        if (first.MidiNumber > last.MidiNumber)
        {
            throw new ArgumentException("The first piano note must not be higher than the last note.");
        }

        var notes = Enumerable.Range(first.MidiNumber, last.MidiNumber - first.MidiNumber + 1)
            .Select(MusicalNote.FromMidiNumber)
            .ToArray();
        var whiteKeyCount = notes.Count(note => !note.IsBlackKey);
        var whiteIndex = 0;
        var result = new List<PianoKeyLayout>(notes.Length);

        foreach (var note in notes)
        {
            result.Add(new PianoKeyLayout
            {
                Note = note,
                WhiteKeyIndex = note.IsBlackKey ? whiteIndex - 1 : whiteIndex,
                PositionPercent = note.IsBlackKey ? whiteIndex * 100d / whiteKeyCount : 0
            });

            if (!note.IsBlackKey)
            {
                whiteIndex++;
            }
        }

        return result;
    }
}
