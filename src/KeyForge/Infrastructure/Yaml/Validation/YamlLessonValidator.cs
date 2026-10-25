namespace KeyForge.Infrastructure.Yaml.Validation;

/// <summary>
/// Validates lesson YAML structure and exercise type discriminators
/// before deserialization.
/// </summary>
internal static class YamlLessonValidator
{
    /// <summary>
    /// Parses the YAML stream, verifies the root is a mapping,
    /// and validates every exercise has a known type discriminator.
    /// </summary>
    /// <exception cref="YamlLessonParseException">
    /// The YAML is malformed, not a mapping, or has structural issues.
    /// </exception>
    /// <exception cref="UnknownExerciseTypeException">
    /// An exercise declares an unsupported 'type' discriminator.
    /// </exception>
    internal static void Validate(string yaml)
    {
        var root = LoadRootMapping(yaml);

        var exercisesNode = root.Children
            .Where(pair => pair.Key is YamlScalarNode { Value: "exercises" })
            .Select(pair => pair.Value)
            .FirstOrDefault();

        if (exercisesNode is not YamlSequenceNode exercises)
        {
            return;
        }

        var lessonId = root.Children
            .Where(pair => pair.Key is YamlScalarNode { Value: "id" })
            .Select(pair => pair.Value as YamlScalarNode)
            .FirstOrDefault()?.Value ?? "unknown";

        foreach (var exerciseNode in exercises.Children)
        {
            ValidateExerciseNode(exerciseNode, lessonId);
        }
    }

    /// <summary>Validates optional, typed exercise configuration after deserialization.</summary>
    internal static void ValidateExerciseConfigurations(LessonDefinition lesson)
    {
        ArgumentNullException.ThrowIfNull(lesson);

        foreach (var exercise in lesson.Exercises)
        {
            try
            {
                exercise.Tempo?.Validate();
                exercise.Metronome?.Validate();
                ValidateExerciseConfiguration(exercise);
            }
            catch (InvalidOperationException ex)
            {
                throw new YamlLessonParseException($"Lesson '{lesson.Id}', exercise '{exercise.Id}': {ex.Message}", ex);
            }
        }
    }

    private static YamlMappingNode LoadRootMapping(string yaml)
    {
        YamlStream stream;
        try
        {
            stream = [];
            stream.Load(new StringReader(yaml));
        }
        catch (YamlException ex)
        {
            throw new YamlLessonParseException("Failed to parse lesson from YAML.", ex);
        }

        if (stream.Documents.Count == 0 || stream.Documents[0].RootNode is not YamlMappingNode root)
        {
            throw new YamlLessonParseException("Lesson must be a YAML mapping.");
        }

        return root;
    }

    private static void ValidateExerciseNode(YamlNode node, string lessonId)
    {
        if (node is not YamlMappingNode mapping)
        {
            throw new YamlLessonParseException(
                $"Each exercise must be a YAML mapping, but found a {node.NodeType} node.");
        }

        var typeScalar = mapping.Children
            .Where(pair => pair.Key is YamlScalarNode { Value: "type" })
            .Select(pair => pair.Value as YamlScalarNode)
            .FirstOrDefault();

        if (typeScalar is null || string.IsNullOrWhiteSpace(typeScalar.Value))
        {
            throw new YamlLessonParseException("Exercise is missing the required 'type' discriminator.");
        }

        if (!ExerciseTypeMap.Types.ContainsKey(typeScalar.Value))
        {
            throw new UnknownExerciseTypeException(typeScalar.Value);
        }

        var exerciseId = mapping.Children
            .Where(pair => pair.Key is YamlScalarNode { Value: "id" })
            .Select(pair => pair.Value as YamlScalarNode)
            .FirstOrDefault()?.Value ?? "unknown";
        // Scale enum values are checked here so an unsupported value gets a useful field path
        // instead of a generic enum-deserialization error.
        foreach (var pair in mapping.Children.Where(pair => pair.Key is YamlScalarNode { Value: "scale" }))
        {
            if (pair.Value is not YamlMappingNode scale)
            {
                continue;
            }

            var scaleType = scale.Children
                .Where(child => child.Key is YamlScalarNode { Value: "type" })
                .Select(child => child.Value as YamlScalarNode)
                .FirstOrDefault()?.Value;
            if (scaleType is not null && !new[] { "major", "naturalMinor" }.Contains(scaleType, StringComparer.Ordinal))
            {
                throw new YamlLessonParseException(
                    $"Lesson '{lessonId}', exercise '{exerciseId}', field 'scale.type' has unsupported value '{scaleType}'.");
            }

            var direction = scale.Children
                .Where(child => child.Key is YamlScalarNode { Value: "direction" })
                .Select(child => child.Value as YamlScalarNode)
                .FirstOrDefault()?.Value;
            if (direction is not null && !new[] { "ascending", "descending", "ascendingDescending" }.Contains(direction, StringComparer.Ordinal))
                throw new YamlLessonParseException(
                    $"Lesson '{lessonId}', exercise '{exerciseId}', field 'scale.direction' has unsupported value '{direction}'.");
        }

        var hand = mapping.Children
            .Where(pair => pair.Key is YamlScalarNode { Value: "hand" })
            .Select(pair => pair.Value as YamlScalarNode)
            .FirstOrDefault()?.Value;
        if (hand is not null && !new[] { "left", "right", "both", "either" }.Contains(hand, StringComparer.Ordinal))
            throw new YamlLessonParseException(
                $"Lesson '{lessonId}', exercise '{exerciseId}', field 'hand' has unsupported value '{hand}'.");
    }

    private static void ValidateExerciseConfiguration(ExerciseDefinition exercise)
    {
        switch (exercise)
        {
            case NoteReadingExerciseDefinition note:
                ValidateCount(note.Count, "count");
                if (note.Notes is not null && (note.Notes.Count == 0 || note.NoteRange is not null || !string.IsNullOrWhiteSpace(note.Range)))
                {
                    throw new InvalidOperationException("define a non-empty 'notes' pool or a range, not both.");
                }
                if (note.NoteRange is not null && !string.IsNullOrWhiteSpace(note.Range))
                {
                    throw new InvalidOperationException("define either 'noteRange' or legacy 'range', not both.");
                }
                if (note.Notes is not null)
                {
                    ValidatePitches(note.Notes, "notes");
                }
                if (note.NoteRange is not null)
                {
                    ValidateRange(note.NoteRange, "noteRange");
                }
                if (note.Notes is null && note.NoteRange is null && !string.IsNullOrWhiteSpace(note.Range))
                {
                    var parts = note.Range.Split('-', 2, StringSplitOptions.TrimEntries);
                    if (parts.Length != 2 || !MusicalNote.TryParse(parts[0], out var from) ||
                        !MusicalNote.TryParse(parts[1], out var to) || from.IsBlackKey || to.IsBlackKey ||
                        from.DiatonicIndex > to.DiatonicIndex)
                    {
                        throw new InvalidOperationException("field 'range' must be an ordered natural-note range.");
                    }
                }
                break;
            case SequenceExerciseDefinition sequence:
                if ((sequence.Sequence is null) == (sequence.Scale is null))
                {
                    throw new InvalidOperationException("define exactly one of 'sequence' or 'scale'.");
                }
                if (sequence.Sequence is not null)
                {
                    ValidatePitches(sequence.Sequence, "sequence");
                }
                ValidateHand(sequence.Hand);
                if (sequence.Scale is not null)
                {
                    ValidateScale(sequence.Scale);
                }
                else
                {
                    ValidateFingers(sequence.Fingers, sequence.Sequence!.Count, "fingers");
                }
                break;
            case SpeedExerciseDefinition speed:
                if (speed.Sequence is not null && speed.Scale is not null)
                    throw new InvalidOperationException("define either 'sequence' or 'scale', not both.");
                if (speed.Sequence is not null) ValidatePitches(speed.Sequence, "sequence");
                if (speed.Scale is not null) ValidateScale(speed.Scale);
                if (speed.Repetitions is < 1 or > 100)
                    throw new InvalidOperationException("field 'repetitions' must be between 1 and 100.");
                break;
            case FingerIndependenceExerciseDefinition fingers:
                ValidateHand(fingers.Hand);
                ValidateFingerList(fingers.Fingers, "fingers");
                if (fingers.Sequence is not null)
                {
                    ValidatePitches(fingers.Sequence, "sequence");
                    if (fingers.Fingers.Count > 0 && fingers.Fingers.Count != fingers.Sequence.Count)
                        throw new InvalidOperationException("field 'fingers' must have one entry per sequence pitch.");
                }
                break;
        }
    }

    private static void ValidateCount(int count, string field)
    {
        if (count is < 1 or > 100) throw new InvalidOperationException($"field '{field}' must be between 1 and 100.");
    }

    private static void ValidatePitches(IReadOnlyCollection<string> pitches, string field)
    {
        if (pitches.Count == 0) throw new InvalidOperationException($"field '{field}' must not be empty.");
        for (var index = 0; index < pitches.Count; index++)
            if (!MusicalNote.TryParse(pitches.ElementAt(index), out _))
                throw new InvalidOperationException($"field '{field}[{index}]' is not a valid MIDI note.");
    }

    private static void ValidateRange(PitchRangeDefinition range, string field)
    {
        if (!MusicalNote.TryParse(range.From, out var from)) throw new InvalidOperationException($"field '{field}.from' is not a valid MIDI note.");
        if (!MusicalNote.TryParse(range.To, out var to)) throw new InvalidOperationException($"field '{field}.to' is not a valid MIDI note.");
        if (from.MidiNumber > to.MidiNumber) throw new InvalidOperationException($"field '{field}' is reversed.");
    }

    private static void ValidateScale(ScaleConfiguration scale)
    {
        if (!MusicalNote.TryParse(scale.Root, out var root))
            throw new InvalidOperationException("field 'scale.root' must be a valid MIDI note including octave.");
        if (scale.Type is not (ScaleType.Major or ScaleType.NaturalMinor))
            throw new InvalidOperationException("field 'scale.type' is unsupported.");
        if (scale.Direction is not (ScaleDirection.Ascending or ScaleDirection.Descending or ScaleDirection.AscendingDescending))
            throw new InvalidOperationException("field 'scale.direction' is unsupported.");
        if (scale.Octaves is < 1 or > 10) throw new InvalidOperationException("field 'scale.octaves' must be between 1 and 10.");
        if (root.MidiNumber + 12 * scale.Octaves > 127)
            throw new InvalidOperationException("field 'scale.octaves' generates pitches above MIDI note 127.");
    }

    private static void ValidateHand(PracticeHand hand)
    {
        if (hand is not (PracticeHand.Left or PracticeHand.Right or PracticeHand.Both or PracticeHand.Either))
            throw new InvalidOperationException("field 'hand' is unsupported.");
    }

    private static void ValidateFingers(IReadOnlyCollection<int> fingers, int pitchCount, string field)
    {
        ValidateFingerList(fingers, field);
        if (fingers.Count > 0 && fingers.Count != pitchCount)
            throw new InvalidOperationException($"field '{field}' must have one entry per sequence pitch.");
    }

    private static void ValidateFingerList(IReadOnlyCollection<int> fingers, string field)
    {
        if (fingers.Any(finger => finger is < 1 or > 5))
            throw new InvalidOperationException($"field '{field}' values must be between 1 and 5.");
    }
}
