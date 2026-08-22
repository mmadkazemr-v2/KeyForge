namespace KeyForge.Features.Practice.Models;

/// <summary>
/// Identifies one chromatic piano note independently of any input device.
/// </summary>
public sealed record MusicalNote
{
    private static readonly string[] ChromaticNames =
        ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];

    private static readonly string[] NaturalNames = ["C", "D", "E", "F", "G", "A", "B"];

    private MusicalNote(string name, int octave)
    {
        Name = name;
        Octave = octave;
    }

    public string Name { get; }
    public int Octave { get; }
    public string Value => $"{Name}{Octave}";
    public bool IsBlackKey => Name.EndsWith('#');
    public int MidiNumber => ((Octave + 1) * 12) + Array.IndexOf(ChromaticNames, Name);
    public int DiatonicIndex =>
        (Octave * NaturalNames.Length) + Array.IndexOf(NaturalNames, Name[..1]);

    public static MusicalNote Parse(string value)
    {
        if (!TryParse(value, out var note))
        {
            throw new FormatException($"'{value}' is not a supported musical note.");
        }

        return note;
    }

    public static bool TryParse(string? value, out MusicalNote note)
    {
        note = null!;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().ToUpperInvariant();
        var nameLength = normalized.Length > 1 && normalized[1] == '#' ? 2 : 1;
        if (normalized.Length <= nameLength ||
            !int.TryParse(normalized[nameLength..], CultureInfo.InvariantCulture, out var octave))
        {
            return false;
        }

        var name = normalized[..nameLength];
        var pitchIndex = Array.IndexOf(ChromaticNames, name);
        var midiNumber = ((octave + 1) * 12) + pitchIndex;
        if (pitchIndex < 0 || midiNumber is < 0 or > 127)
        {
            return false;
        }

        note = new MusicalNote(name, octave);
        return true;
    }

    public static MusicalNote FromMidiNumber(int midiNumber)
    {
        if (midiNumber is < 0 or > 127)
        {
            throw new ArgumentOutOfRangeException(nameof(midiNumber));
        }

        return new MusicalNote(
            ChromaticNames[midiNumber % ChromaticNames.Length],
            (midiNumber / ChromaticNames.Length) - 1);
    }

    public static MusicalNote FromDiatonicIndex(int diatonicIndex)
    {
        if (diatonicIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(diatonicIndex));
        }

        return new MusicalNote(
            NaturalNames[diatonicIndex % NaturalNames.Length],
            diatonicIndex / NaturalNames.Length);
    }

    public override string ToString() => Value;
}
