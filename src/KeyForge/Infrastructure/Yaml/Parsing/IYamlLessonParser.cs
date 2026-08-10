using KeyForge.Features.Lessons.Models;

namespace KeyForge.Infrastructure.Yaml.Parsing;

/// <summary>
/// Parses lesson YAML content into a strongly typed <see cref="LessonDefinition"/>.
/// <para>
/// The parser only parses the YAML string it is given. Reading files or
/// discovering lessons is intentionally a separate concern.
/// </para>
/// </summary>
public interface IYamlLessonParser
{
    /// <summary>
    /// Parses YAML content into a <see cref="LessonDefinition"/>.
    /// </summary>
    /// <param name="yaml">The YAML content of a single lesson.</param>
    /// <returns>The strongly typed lesson definition.</returns>
    /// <exception cref="YamlLessonParseException">
    /// Thrown when the content is empty, malformed, or structurally invalid.
    /// </exception>
    /// <exception cref="UnknownExerciseTypeException">
    /// Thrown when an exercise declares an unsupported 'type' discriminator.
    /// </exception>
    LessonDefinition Parse(string yaml);
}
