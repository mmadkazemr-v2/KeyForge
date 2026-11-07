using System.Xml;
using System.Xml.Linq;

namespace KeyForge.Infrastructure.Content.Import;

/// <summary>Imports a deliberately limited, explicit-part MusicXML performance subset into validated lesson YAML.</summary>
public sealed class MusicXmlPerformanceImporter
{
    public const int MaximumCharacters = 1_000_000;
    public IReadOnlyList<(string Id, string Name)> GetParts(string xml) => Read(xml).Elements("part-list").Elements("score-part")
        .Select(p => ((string?)p.Attribute("id") ?? throw new InvalidDataException("Part ID is missing."), (string?)p.Element("part-name") ?? "Unnamed part")).ToArray();

    public string Import(string xml, string partId, string lessonId, string exerciseId, int fallbackBpm)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partId);
        ArgumentException.ThrowIfNullOrWhiteSpace(lessonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(exerciseId);
        var root = Read(xml);
        var part = root.Elements("part").SingleOrDefault(p => (string?)p.Attribute("id") == partId)
            ?? throw new InvalidDataException("Select an existing learner part explicitly.");
        var unsupported = new HashSet<string> { "tie", "tied", "grace", "repeat", "ending", "segno", "coda", "transpose", "unpitched", "cue", "pedal" };
        if (part.Descendants().Any(e => unsupported.Contains(e.Name.LocalName)))
            throw new InvalidDataException("Ties, grace/cue/unpitched notes, repeats/navigation, transposing parts and pedal instructions are not supported by this importer.");
        if (part.Descendants("note").Any(n => n.Attribute("attack") is not null || n.Attribute("release") is not null))
            throw new InvalidDataException("MusicXML performance attack/release adjustments are not supported.");
        var tempos = part.Descendants("sound").Attributes("tempo").Select(a => Number(a.Value)).Distinct().ToArray();
        if (tempos.Length > 1) throw new InvalidDataException("Tempo changes require separate exercises.");
        var bpm = tempos.Length == 1 ? tempos[0] : fallbackBpm;
        if (bpm is < 10 or > 400 || bpm != Math.Truncate(bpm)) throw new InvalidDataException("Choose a whole quarter-note BPM between 10 and 400.");
        var events = new List<MusicalPerformanceEvent>();
        double divisions = 0, measureStart = 0;
        double? meterBeats = null;
        foreach (var measure in part.Elements("measure"))
        {
            if (measure.Element("attributes")?.Element("time") is { } time)
            {
                if (time.Element("senza-misura") is not null) meterBeats = null;
                else
                {
                    var beats = Number(time.Element("beats")?.Value ?? "");
                    var beatType = Number(time.Element("beat-type")?.Value ?? "");
                    if (beats <= 0 || beatType is not (1 or 2 or 4 or 8 or 16)) throw new InvalidDataException("Unsupported time signature.");
                    meterBeats = beats * 4 / beatType;
                }
            }
            double cursor = 0, end = 0;
            double? lastNoteOnset = null;
            foreach (var item in measure.Elements())
            {
                if (item.Name.LocalName == "attributes" && item.Element("divisions") is { } division)
                {
                    if (cursor != 0) throw new InvalidDataException("Mid-measure divisions changes are unsupported.");
                    divisions = Number(division.Value);
                    if (divisions <= 0) throw new InvalidDataException("Divisions must be positive.");
                }
                if (item.Name.LocalName is not ("note" or "backup" or "forward")) continue;
                if (divisions <= 0 || item.Element("duration") is not { } duration) throw new InvalidDataException("Explicit divisions and duration are required.");
                var beats = Number(duration.Value) / divisions;
                if (beats <= 0) throw new InvalidDataException("Durations must be positive.");
                if (item.Name.LocalName is "backup" or "forward")
                {
                    cursor += item.Name.LocalName == "backup" ? -beats : beats;
                    if (cursor < -0.0000001) throw new InvalidDataException("Backup crosses the start of a measure.");
                    lastNoteOnset = null;
                    end = Math.Max(end, cursor);
                    continue;
                }
                var chord = item.Element("chord") is not null;
                if (chord && lastNoteOnset is null) throw new InvalidDataException("Chord note must follow a pitched note.");
                var onset = chord ? lastNoteOnset!.Value : cursor;
                var pitch = item.Element("rest") is not null ? null : Pitch(item.Element("pitch"));
                if (chord && pitch is null) throw new InvalidDataException("Chord rests are unsupported.");
                events.Add(new MusicalPerformanceEvent { Pitch = pitch, OnsetBeats = measureStart + onset, DurationBeats = beats, Voice = (string?)item.Element("voice"), Hand = PracticeHand.Either });
                if (!chord) { cursor += beats; lastNoteOnset = pitch is null ? null : onset; }
                end = Math.Max(end, onset + beats);
            }
            if (end <= 0) throw new InvalidDataException("Empty measures have no reliable duration in this import subset.");
            if (meterBeats is { } expectedLength && (string?)measure.Attribute("implicit") != "yes" && Math.Abs(end - expectedLength) > 0.0000001)
                throw new InvalidDataException("Measure does not fill its declared meter; use an explicitly implicit pickup or correct the source.");
            measureStart += end;
        }
        var performance = new PerformanceDefinition { Events = events };
        performance.Validate();
        var lesson = new LessonDefinition
        {
            Id = lessonId,
            Title = "Imported learner part: " + partId,
            Order = 1001,
            Description = "MusicXML performance import. Original spelling/notation, hand and fingering are not inferred.",
            Unlock = new UnlockRule { Mode = UnlockMode.Immediate },
            Completion = new CompletionRule { RequireAllExercises = true },
            Exercises = [new SequenceExerciseDefinition { Id = exerciseId, Title = "Imported performance", Tempo = new TempoConfiguration { DefaultBpm = (int)bpm }, Performance = performance }]
        };
        var yaml = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithEnumNamingConvention(CamelCaseNamingConvention.Instance).ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build().Serialize(lesson);
        _ = new YamlLessonParser().Parse(yaml);
        return yaml;
    }

    private static XElement Read(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);
        if (xml.Length > MaximumCharacters) throw new InvalidDataException("MusicXML exceeds the import size limit.");
        using var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null, MaxCharactersInDocument = MaximumCharacters });
        XElement root;
        try { root = XElement.Load(reader); }
        catch (XmlException ex) { throw new InvalidDataException("Invalid MusicXML document.", ex); }
        if (root.Name.LocalName != "score-partwise") throw new InvalidDataException("Only uncompressed score-partwise MusicXML is supported.");
        foreach (var element in root.DescendantsAndSelf()) element.Name = element.Name.LocalName;
        return root;
    }
    private static double Number(string value) => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) && double.IsFinite(result)
        ? result : throw new InvalidDataException("Invalid numeric MusicXML value.");
    private static string Pitch(XElement? pitch)
    {
        if (pitch is null || !MusicalNote.TryParse((string?)pitch.Element("step") + (string?)pitch.Element("octave"), out var note))
            throw new InvalidDataException("Explicit pitched octave is required.");
        var alteration = pitch.Element("alter") is { } alter ? Number(alter.Value) : 0;
        var midi = note.MidiNumber + alteration;
        if (midi != Math.Truncate(midi) || midi is < 0 or > 127) throw new InvalidDataException("Microtonal or out-of-range pitch is unsupported.");
        return MusicalNote.FromMidiNumber((int)midi).Value;
    }
}
