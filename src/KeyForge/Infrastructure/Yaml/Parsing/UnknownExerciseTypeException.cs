namespace KeyForge.Infrastructure.Yaml.Parsing;

/// <summary>
/// Thrown when an exercise entry declares a 'type' discriminator that does not
/// map to any supported <see cref="ExerciseType"/>.
/// </summary>
public sealed class UnknownExerciseTypeException : YamlLessonParseException
{
    /// <summary>The unrecognized discriminator value found in the YAML.</summary>
    public string UnknownType { get; }

    public UnknownExerciseTypeException(string unknownType)
        : base($"Unknown exercise type '{unknownType}'.")
    {
        UnknownType = unknownType;
    }
}
