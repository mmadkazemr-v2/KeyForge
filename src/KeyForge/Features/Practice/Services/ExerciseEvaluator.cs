namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Performs the first generic exact-input evaluation. The input source is not
/// considered, so keyboard, virtual-piano, and future MIDI adapters share the
/// same evaluation path.
/// </summary>
public sealed class ExerciseEvaluator : IExerciseEvaluator
{
    /// <inheritdoc />
    public ExerciseEvaluationResult Evaluate(
        ExerciseChallenge challenge,
        IReadOnlyList<ExerciseInputEvent> inputEvents)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        ArgumentNullException.ThrowIfNull(inputEvents);

        if (challenge.ExerciseType == ExerciseType.Rhythm)
        {
            return new RhythmExerciseEvaluator().Evaluate(challenge, inputEvents);
        }

        if (challenge.ExerciseType == ExerciseType.NoteReading)
        {
            return new NoteReadingExerciseEvaluator().Evaluate(challenge, inputEvents);
        }

        if (challenge.ExerciseType is ExerciseType.Interval or ExerciseType.Octave)
        {
            return new NoteTargetExerciseEvaluator().Evaluate(challenge, inputEvents, challenge.ExerciseType);
        }

        var expected = challenge.ExpectedInputs.FirstOrDefault();
        if (expected is null)
        {
            return new ExerciseEvaluationResult
            {
                IsSuccessful = false,
                Score = 0,
                Feedback = ExerciseEvaluationFeedback.UnsupportedChallenge
            };
        }

        var actual = inputEvents.FirstOrDefault();
        if (actual is null)
        {
            return new ExerciseEvaluationResult
            {
                IsSuccessful = false,
                Score = 0,
                Feedback = ExerciseEvaluationFeedback.NoInput,
                ExpectedValue = expected.Value
            };
        }

        var isCorrect = actual.Type == expected.Type &&
            string.Equals(actual.Value.Trim(), expected.Value.Trim(), StringComparison.OrdinalIgnoreCase);

        return new ExerciseEvaluationResult
        {
            IsSuccessful = isCorrect,
            Score = isCorrect ? 100 : 0,
            Feedback = isCorrect
                ? ExerciseEvaluationFeedback.Correct
                : ExerciseEvaluationFeedback.Incorrect,
            ExpectedValue = expected.Value,
            ActualValue = actual.Value
        };
    }
}
