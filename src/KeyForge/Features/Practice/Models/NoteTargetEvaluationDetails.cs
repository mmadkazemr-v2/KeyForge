namespace KeyForge.Features.Practice.Models;

/// <summary>Aggregate feedback for exercises answered by selecting exact target notes.</summary>
public sealed class NoteTargetEvaluationDetails
{
    public required IReadOnlyList<NoteTargetQuestionEvaluation> Questions { get; init; }
    public int QuestionCount => Questions.Count;
    public int CorrectAnswerCount => Questions.Count(question => question.IsCorrect);
    public int IncorrectAnswerCount => QuestionCount - CorrectAnswerCount;
}

/// <summary>Evaluation of one note-target question.</summary>
public sealed class NoteTargetQuestionEvaluation
{
    public required int QuestionNumber { get; init; }
    public required string ExpectedNote { get; init; }
    public string? ActualNote { get; init; }
    public required bool IsCorrect { get; init; }
}
