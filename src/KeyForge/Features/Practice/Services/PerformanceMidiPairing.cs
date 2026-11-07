namespace KeyForge.Features.Practice.Services;

/// <summary>Pairs keys FIFO per device/channel/pitch and tracks CC64-latched MIDI release endpoints.</summary>
internal static class PerformanceMidiPairing
{
    internal static (Dictionary<ExerciseInputEvent, TimeSpan> Releases, Dictionary<ExerciseInputEvent, TimeSpan> SustainedReleases, int UnexpectedReleases) Pair(IReadOnlyList<ExerciseInputEvent> inputs)
    {
        var active = new Dictionary<(string?, int?, int), Queue<ExerciseInputEvent>>();
        var releases = new Dictionary<ExerciseInputEvent, TimeSpan>();
        var sustained = new Dictionary<ExerciseInputEvent, TimeSpan>();
        var pedalDown = new HashSet<(string?, int?)>();
        var latched = new Dictionary<(string?, int?), List<ExerciseInputEvent>>();
        var unexpected = 0;
        foreach (var input in inputs.OrderBy(e => e.Offset).ThenBy(e => e.Sequence))
        {
            var port = (input.DeviceId, input.MidiChannel);
            if (input.MidiKind == MidiEventKind.ControlChange && input.MidiData1 == 64 && input.MidiData2 is { } value)
            {
                if (value >= 64) pedalDown.Add(port);
                else
                {
                    pedalDown.Remove(port);
                    if (latched.Remove(port, out var notes)) foreach (var note in notes) sustained.Add(note, input.Offset);
                }
                continue;
            }
            if (input.MidiKind is not (MidiEventKind.NoteOn or MidiEventKind.NoteOff) || !MusicalNote.TryParse(input.Value, out var pitch)) continue;
            var key = (input.DeviceId, input.MidiChannel, pitch.MidiNumber);
            if (input.MidiKind == MidiEventKind.NoteOn)
            {
                if (!active.TryGetValue(key, out var queue)) active[key] = queue = new Queue<ExerciseInputEvent>();
                queue.Enqueue(input);
            }
            else if (active.TryGetValue(key, out var queue) && queue.TryDequeue(out var attack))
            {
                releases.Add(attack, input.Offset);
                if (pedalDown.Contains(port))
                {
                    if (!latched.TryGetValue(port, out var notes)) latched[port] = notes = [];
                    notes.Add(attack);
                }
                else sustained.Add(attack, input.Offset);
            }
            else unexpected++;
        }
        return (releases, sustained, unexpected);
    }
}
