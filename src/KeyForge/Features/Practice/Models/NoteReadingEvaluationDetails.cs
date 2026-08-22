namespace KeyForge.Features.Practice.Models;

/// <summary>Aggregate, presentation-neutral feedback for a note-reading attempt.</summary>
public sealed class NoteReadingEvaluationDetails
{
    public required IReadOnlyList<NoteReadingQuestionEvaluation> Questions { get; init; }
    public int QuestionCount => Questions.Count;
    public int CorrectAnswerCount => Questions.Count(question => question.IsCorrect);
    public int IncorrectAnswerCount => QuestionCount - CorrectAnswerCount;
}
