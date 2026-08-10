namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// An exercise that practises playing individual fingers independently,
/// keeping them relaxed while the others stay still.
/// </summary>
public sealed class FingerIndependenceExerciseDefinition : ExerciseDefinition
{
    /// <summary>
    /// The fingers (by standard piano numbering 1–5, thumb = 1) to drill, e.g. [3, 4].
    /// An empty list means all fingers of the selected hand(s).
    /// </summary>
    public List<int> Fingers { get; set; } = [];

    /// <summary>Which hand(s) the exercise is practised with.</summary>
    public PracticeHand Hand { get; set; } = PracticeHand.Both;

    /// <summary>Default constructor that pins the polymorphic discriminator.</summary>
    public FingerIndependenceExerciseDefinition() => Type = ExerciseType.FingerIndependence;
}
