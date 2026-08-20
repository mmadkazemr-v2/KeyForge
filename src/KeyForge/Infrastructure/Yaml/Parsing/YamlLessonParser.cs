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
                options.AddKeyValueTypeDiscriminator<ExerciseDefinition>("type", ExerciseTypeMap.Types))
            .Build();
    }

    public LessonDefinition Parse(string yaml)
    {
        ArgumentNullException.ThrowIfNull(yaml);

        if (string.IsNullOrWhiteSpace(yaml))
        {
            throw new YamlLessonParseException("The YAML content is empty.");
        }

        YamlLessonValidator.Validate(yaml);

        try
        {
            return _deserializer.Deserialize<LessonDefinition>(yaml);
        }
        catch (YamlException ex)
        {
            throw new YamlLessonParseException("Failed to parse lesson from YAML.", ex);
        }
    }
}
