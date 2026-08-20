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

        foreach (var exerciseNode in exercises.Children)
        {
            ValidateExerciseNode(exerciseNode);
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

    private static void ValidateExerciseNode(YamlNode node)
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
    }
}
