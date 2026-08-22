namespace KeyForge.Features.Practice.Models;

/// <summary>Evaluation details for one note-reading question.</summary>
public sealed class NoteReadingQuestionEvaluation
{
    public required int QuestionNumber { get; init; }
    public required string ExpectedNote { get; init; }
    public string? ActualNote { get; init; }
    public bool IsCorrect { get; init; }
}
