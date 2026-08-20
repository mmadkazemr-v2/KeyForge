namespace KeyForge.Infrastructure.Yaml.Parsing;

/// <summary>
/// Maps YAML 'type' discriminator values to their concrete exercise types.
/// Used by both YamlDotNet's type discriminator and the pre-deserialization
/// validation in <see cref="YamlLessonValidator"/>.
/// </summary>
internal static class ExerciseTypeMap
{
    internal static readonly Dictionary<string, Type> Types = new()
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
}
