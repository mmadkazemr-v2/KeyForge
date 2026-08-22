namespace KeyForge.Components.Music;

/// <summary>Maps a natural note to compact SVG staff geometry.</summary>
public static class MusicStaffLayout
{
    public const double BottomLineY = 140;
    public const double StaffStep = 7.5;

    public static StaffNoteLayout Create(MusicalNote note, ClefType clef)
    {
        ArgumentNullException.ThrowIfNull(note);

        if (note.IsBlackKey)
        {
            throw new ArgumentException(
                "The current staff renderer supports natural notes only.",
                nameof(note));
        }

        var normalizedClef = clef == ClefType.Bass ? ClefType.Bass : ClefType.Treble;
        var bottomLineNote = MusicalNote.Parse(normalizedClef == ClefType.Bass ? "G2" : "E4");
        var staffOffset = note.DiatonicIndex - bottomLineNote.DiatonicIndex;
        var ledgerLines = CreateLedgerLines(staffOffset);

        return new StaffNoteLayout
        {
            Clef = normalizedClef,
            StaffOffset = staffOffset,
            NoteY = BottomLineY - (staffOffset * StaffStep),
            LedgerLineYs = ledgerLines
        };
    }

    private static IReadOnlyList<double> CreateLedgerLines(int staffOffset)
    {
        if (staffOffset <= -2)
        {
            return Enumerable.Range(1, Math.Abs(staffOffset) / 2)
                .Select(index => BottomLineY + (index * StaffStep * 2))
                .ToArray();
        }

        if (staffOffset >= 10)
        {
            return Enumerable.Range(1, (staffOffset - 8) / 2)
                .Select(index => BottomLineY - ((8 + (index * 2)) * StaffStep))
                .ToArray();
        }

        return [];
    }
}

public sealed class StaffNoteLayout
{
    public required ClefType Clef { get; init; }
    public required int StaffOffset { get; init; }
    public required double NoteY { get; init; }
    public required IReadOnlyList<double> LedgerLineYs { get; init; }
}
