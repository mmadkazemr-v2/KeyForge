using KeyForge.Features.Exercises.Models;
using YamlDotNet.RepresentationModel;

namespace KeyForge.Infrastructure.Yaml.Parsing;

/// <summary>
/// Resolves and validates the concrete <see cref="ExerciseDefinition"/> type for
/// an exercise YAML node, based on its 'type' discriminator value.
/// </summary>
internal static class YamlExerciseParser
{
    /// <summary>
    /// Maps YAML 'type' discriminator values to their concrete exercise types.
    /// The same mapping drives YamlDotNet's key-value type discriminator and
    /// the pre-deserialization validation.
    /// </summary>
    internal static readonly Dictionary<string, Type> ConcreteTypes = new()
    {
        ["rhythm"] = typeof(RhythmExerciseDefinition),
        ["noteReading"] = typeof(NoteReadingExerciseDefinition),
        ["earTraining"] = typeof(EarTrainingExerciseDefinition),
        ["interval"] = typeof(IntervalExerciseDefinition),
        ["octave"] = typeof(OctaveExerciseDefinition),
        ["fingerIndependence"] = typeof(FingerIndependenceExerciseDefinition),
        ["mentalKeyboard"] = typeof(MentalKeyboardExerciseDefinition),
        ["speed"] = typeof(SpeedExerciseDefinition)
    };

    /// <summary>
    /// Validates a single exercise node: it must be a mapping that declares a
    /// supported 'type' discriminator.
    /// </summary>
    /// <exception cref="YamlLessonParseException">
    /// The node is not a mapping, or it is missing the 'type' discriminator.
    /// </exception>
    /// <exception cref="UnknownExerciseTypeException">
    /// The 'type' discriminator does not map to a supported exercise type.
    /// </exception>
    internal static void Validate(YamlNode node)
    {
        if (node is not YamlMappingNode mapping)
        {
            throw new YamlLessonParseException(
                $"Each exercise must be a YAML mapping, but found a {node.NodeType} node.");
        }

        var typeValue = ReadDiscriminator(mapping);

        if (!ConcreteTypes.ContainsKey(typeValue))
        {
            throw new UnknownExerciseTypeException(typeValue);
        }
    }

    private static string ReadDiscriminator(YamlMappingNode mapping)
    {
        var typeScalar = mapping.Children
            .Where(pair => pair.Key is YamlScalarNode { Value: "type" })
            .Select(pair => pair.Value as YamlScalarNode)
            .FirstOrDefault();

        if (typeScalar is null || string.IsNullOrWhiteSpace(typeScalar.Value))
        {
            throw new YamlLessonParseException("Exercise is missing the required 'type' discriminator.");
        }

        return typeScalar.Value;
    }
}
