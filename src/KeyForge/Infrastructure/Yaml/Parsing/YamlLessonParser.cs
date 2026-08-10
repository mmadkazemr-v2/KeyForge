using KeyForge.Features.Exercises.Models;
using KeyForge.Features.Lessons.Models;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KeyForge.Infrastructure.Yaml.Parsing;

/// <summary>
/// Parses lesson YAML content into a strongly typed <see cref="LessonDefinition"/>
/// using YamlDotNet. Exercises are deserialized into their concrete
/// <see cref="ExerciseDefinition"/> subclasses based on the 'type' discriminator.
/// </summary>
public sealed class YamlLessonParser : IYamlLessonParser
{
    private readonly IDeserializer _deserializer;

    public YamlLessonParser()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithEnumNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeDiscriminatingNodeDeserializer(options =>
                options.AddKeyValueTypeDiscriminator<ExerciseDefinition>("type", YamlExerciseParser.ConcreteTypes))
            .Build();
    }

    public LessonDefinition Parse(string yaml)
    {
        ArgumentNullException.ThrowIfNull(yaml);

        if (string.IsNullOrWhiteSpace(yaml))
        {
            throw new YamlLessonParseException("The YAML content is empty.");
        }

        ValidateExerciseTypes(yaml);

        try
        {
            return _deserializer.Deserialize<LessonDefinition>(yaml);
        }
        catch (YamlException ex)
        {
            throw new YamlLessonParseException("Failed to parse lesson from YAML.", ex);
        }
    }

    /// <summary>
    /// Walks the lesson YAML and verifies that every exercise declares a known
    /// 'type' discriminator. This runs before deserialization so that invalid
    /// exercises fail with a clear, domain-specific error.
    /// </summary>
    private static void ValidateExerciseTypes(string yaml)
    {
        YamlStream stream;
        try
        {
            stream = new YamlStream();
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

        if (!root.Children.Any(pair => pair.Key is YamlScalarNode { Value: "exercises" }))
        {
            return;
        }

        var exercisesNode = root.Children
            .Where(pair => pair.Key is YamlScalarNode { Value: "exercises" })
            .Select(pair => pair.Value)
            .First();

        if (exercisesNode is not YamlSequenceNode exercises)
        {
            throw new YamlLessonParseException("The 'exercises' entry must be a sequence.");
        }

        foreach (var exerciseNode in exercises.Children)
        {
            YamlExerciseParser.Validate(exerciseNode);
        }
    }
}
