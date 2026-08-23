namespace KeyForge.Features.Practice.Models;

/// <summary>Question-by-question feedback for an ear-training attempt.</summary>
public sealed class EarTrainingEvaluationDetails
{
    public required IReadOnlyList<NoteTargetQuestionEvaluation> Questions { get; init; }
    public int QuestionCount => Questions.Count;
    public int CorrectAnswerCount => Questions.Count(question => question.IsCorrect);
    public int IncorrectAnswerCount => QuestionCount - CorrectAnswerCount;
}
