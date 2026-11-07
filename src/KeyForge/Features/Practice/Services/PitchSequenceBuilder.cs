namespace KeyForge.Features.Practice.Services;

/// <summary>Builds pitch lists from authored notes or beginner scale formulas.</summary>
public static class PitchSequenceBuilder
{
    public static IReadOnlyList<string> Transpose(IReadOnlyList<string> notes, int semitones)
    {
        ArgumentNullException.ThrowIfNull(notes);
        if (semitones is < -48 or > 48) throw new InvalidOperationException("Transposition must be between -48 and 48 semitones.");
        var midi = notes.Select(n => MusicalNote.Parse(n).MidiNumber + semitones).ToArray();
        if (midi.Any(n => n is < 0 or > 127)) throw new InvalidOperationException("Transposition exceeds MIDI pitch range.");
        return midi.Select(MusicalNote.FromMidiNumber).Select(n => n.Value).ToArray();
    }

    public static IReadOnlyList<string> ParsePitches(IEnumerable<string> pitches, string exerciseId, string field)
    {
        ArgumentNullException.ThrowIfNull(pitches);
        var notes = pitches.Select(pitch => MusicalNote.TryParse(pitch, out var note)
                ? note.Value
                : throw new InvalidOperationException(
                    $"Exercise '{exerciseId}', field '{field}' contains invalid MIDI note '{pitch}'."))
            .ToArray();
        if (notes.Length == 0)
        {
            throw new InvalidOperationException($"Exercise '{exerciseId}', field '{field}' must not be empty.");
        }
        return notes;
    }

    public static IReadOnlyList<string> BuildArpeggio(ArpeggioConfiguration arpeggio, string exerciseId)
    {
        ArgumentNullException.ThrowIfNull(arpeggio);
        if (!MusicalNote.TryParse(arpeggio.Root, out var root) || !Enum.IsDefined(arpeggio.Quality) ||
            !Enum.IsDefined(arpeggio.Direction) || arpeggio.Octaves is < 1 or > 4 || arpeggio.Inversion is < 0 or > 2)
            throw new InvalidOperationException($"Exercise '{exerciseId}' has invalid arpeggio root, quality, direction, octaves or inversion.");
        var triad = new[] { 0, arpeggio.Quality == TriadQuality.Major ? 4 : 3, 7 };
        var inverted = triad.Skip(arpeggio.Inversion).Concat(triad.Take(arpeggio.Inversion).Select(x => x + 12)).ToArray();
        var ascending = Enumerable.Range(0, arpeggio.Octaves)
            .SelectMany(o => inverted.Select(x => root.MidiNumber + 12 * o + x))
            .Append(root.MidiNumber + inverted[0] + arpeggio.Octaves * 12).ToArray();
        if (ascending.Any(x => x > 127)) throw new InvalidOperationException("Arpeggio exceeds MIDI pitch range.");
        var directed = arpeggio.Direction switch
        {
            ScaleDirection.Descending => ascending.Reverse(),
            ScaleDirection.AscendingDescending => ascending.Concat(ascending.Take(ascending.Length - 1).Reverse()),
            _ => ascending.AsEnumerable()
        };
        return directed.Select(MusicalNote.FromMidiNumber).Select(n => n.Value).ToArray();
    }

    public static IReadOnlyList<string> BuildScale(ScaleConfiguration scale, string exerciseId)
    {
        ArgumentNullException.ThrowIfNull(scale);
        if (!MusicalNote.TryParse(scale.Root, out var root))
        {
            throw new InvalidOperationException(
                $"Exercise '{exerciseId}', field 'scale.root' must be a MIDI note including its octave.");
        }
        if (scale.Octaves is < 1 or > 10)
        {
            throw new InvalidOperationException($"Exercise '{exerciseId}', field 'scale.octaves' must be between 1 and 10.");
        }

        var degrees = scale.Type switch
        {
            ScaleType.Major => new[] { 0, 2, 4, 5, 7, 9, 11, 12 },
            ScaleType.NaturalMinor => new[] { 0, 2, 3, 5, 7, 8, 10, 12 },
            _ => throw new InvalidOperationException($"Exercise '{exerciseId}', field 'scale.type' is unsupported.")
        };
        var upperMidi = root.MidiNumber + (12 * scale.Octaves);
        if (upperMidi > 127)
        {
            throw new InvalidOperationException(
                $"Exercise '{exerciseId}', field 'scale.octaves' generates pitches above MIDI note 127.");
        }

        var ascendingMidi = Enumerable.Range(0, scale.Octaves)
            .SelectMany(octave => degrees.Take(7).Select(degree => root.MidiNumber + octave * 12 + degree))
            .Append(upperMidi)
            .ToArray();
        var directedMidi = scale.Direction switch
        {
            ScaleDirection.Ascending => ascendingMidi,
            ScaleDirection.Descending => ascendingMidi.Reverse(),
            ScaleDirection.AscendingDescending => ascendingMidi.Concat(ascendingMidi
                .Take(ascendingMidi.Length - 1).Reverse()),
            _ => throw new InvalidOperationException($"Exercise '{exerciseId}', field 'scale.direction' is unsupported.")
        };

        return directedMidi.Select(MusicalNote.FromMidiNumber).Select(note => note.Value).ToArray();
    }
}
