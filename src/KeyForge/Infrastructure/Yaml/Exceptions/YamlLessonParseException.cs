namespace KeyForge.Infrastructure.Yaml.Exceptions;

/// <summary>
/// Thrown when lesson YAML content cannot be parsed into a <see cref="LessonDefinition"/>.
/// Wraps low-level YamlDotNet failures so callers get a domain-specific error.
/// </summary>
public class YamlLessonParseException : Exception
{
    public YamlLessonParseException(string message)
        : base(message)
    {
    }

    public YamlLessonParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
