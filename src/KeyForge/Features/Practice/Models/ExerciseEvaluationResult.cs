namespace KeyForge.Features.Practice.Models;

/// <summary>
/// The outcome of evaluating a single exercise attempt against its definition.
/// <para>
/// This is a pure data model produced by <see cref="Services.IExerciseEvaluator"/>.
/// It contains only the information necessary for the current evaluation step.
/// </para>
/// </summary>
public sealed class ExerciseEvaluationResult
{
    /// <summary>
    /// Whether the attempt met the minimum requirements to be
    /// considered successful for this exercise type.
    /// </summary>
    public bool IsSuccessful { get; init; }

    /// <summary>
    /// Raw score generated from the challenge and captured input.
    /// The scorer normalizes this value to the final 0..100 range.
    /// </summary>
    public int? Score { get; init; }

    /// <summary>Presentation-neutral information describing the outcome.</summary>
    public ExerciseEvaluationFeedback Feedback { get; init; }

    /// <summary>The primary expected value, when one was available.</summary>
    public string? ExpectedValue { get; init; }

    /// <summary>The primary captured value, when one was available.</summary>
    public string? ActualValue { get; init; }

    /// <summary>Detailed timing feedback when the evaluated challenge is Rhythm.</summary>
    public RhythmEvaluationDetails? Rhythm { get; init; }

    /// <summary>Question-by-question feedback when the challenge is Note Reading.</summary>
    public NoteReadingEvaluationDetails? NoteReading { get; init; }
}
