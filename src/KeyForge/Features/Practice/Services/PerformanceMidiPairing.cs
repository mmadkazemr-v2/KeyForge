namespace KeyForge.Features.Practice.Services;

/// <summary>Pairs repeated and overlapping MIDI keys by device, channel and pitch, FIFO within a key.</summary>
internal static class PerformanceMidiPairing
{
    internal static (Dictionary<ExerciseInputEvent, TimeSpan> Releases, int UnexpectedReleases) Pair(IReadOnlyList<ExerciseInputEvent> inputs)
    {
        var active = new Dictionary<(string?, int?, int), Queue<ExerciseInputEvent>>();
        var releases = new Dictionary<ExerciseInputEvent, TimeSpan>();
        var unexpected = 0;
        foreach (var input in inputs.OrderBy(e => e.Offset).ThenBy(e => e.Sequence))
        {
            if (input.MidiKind is not (MidiEventKind.NoteOn or MidiEventKind.NoteOff) || !MusicalNote.TryParse(input.Value, out var note)) continue;
            var key = (input.DeviceId, input.MidiChannel, note.MidiNumber);
            if (input.MidiKind == MidiEventKind.NoteOn)
            {
                if (!active.TryGetValue(key, out var queue)) active[key] = queue = new Queue<ExerciseInputEvent>();
                queue.Enqueue(input);
            }
            else if (active.TryGetValue(key, out var queue) && queue.TryDequeue(out var attack)) releases.Add(attack, input.Offset);
            else unexpected++;
        }
        return (releases, unexpected);
    }
}
