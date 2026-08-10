namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// An exercise that practises identifying musical elements by ear.
/// The concrete task is described by <see cref="TaskType"/>.
/// </summary>
public sealed class EarTrainingExerciseDefinition : ExerciseDefinition
{
    /// <summary>The specific ear-training task being practised.</summary>
    public EarTrainingTask TaskType { get; set; } = EarTrainingTask.PitchMatching;

    /// <summary>Number of questions/repetitions the student should complete.</summary>
    public int Rounds { get; set; } = 10;

    /// <summary>
    /// Key centers the exercise draws from, e.g. ["C major", "G major"].
    /// An empty list means no key constraint.
    /// </summary>
    public List<string> Keys { get; set; } = [];

    /// <summary>Default constructor that pins the polymorphic discriminator.</summary>
    public EarTrainingExerciseDefinition() => Type = ExerciseType.EarTraining;
}
