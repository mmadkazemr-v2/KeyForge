namespace KeyForge.Tests.UnitTest.Features.Practice.Services;

public sealed class PitchComparisonTests
{
    [Fact]
    public void ComparisonUsesTwoNotesAndChoiceEvaluation()
    {
        var exercise = new EarTrainingExerciseDefinition { Id = "compare", TaskType = EarTrainingTask.PitchComparison, Rounds = 10 };
        var challenge = new EarTrainingChallengeGenerator().Create(exercise);
        Assert.Equal(new[] { "higher", "lower" }, challenge.EarTraining!.IntervalChoices);
        Assert.Contains(challenge.ExpectedInputs, x => x.Value == "higher");
        Assert.Contains(challenge.ExpectedInputs, x => x.Value == "lower");
        foreach (var pair in challenge.EarTraining.IntervalQuestions.Zip(challenge.ExpectedInputs))
        {
            var ascending = MusicalNote.Parse(pair.First.TargetNote).MidiNumber > MusicalNote.Parse(pair.First.SourceNote).MidiNumber;
            Assert.Equal(ascending ? "higher" : "lower", pair.Second.Value);
            Assert.Equal(2, KeyForge.Components.Practice.IntervalRecognitionWorkspace.BuildPlaybackSequence(pair.First).Count);
        }
        var inputs = challenge.ExpectedInputs.Select(x => new ExerciseInputEvent { Source = InputSource.ComputerKeyboard, Type = InputEventType.Choice, Value = x.Value, Timestamp = DateTime.UtcNow }).ToArray();
        Assert.Equal(100, new EarTrainingExerciseEvaluator().Evaluate(challenge, inputs).Score);
        Assert.Equal(0, new EarTrainingExerciseEvaluator().Evaluate(challenge, inputs.Select(x => new ExerciseInputEvent { Source = InputSource.ComputerKeyboard, Type = InputEventType.Note, Value = x.Value, Timestamp = DateTime.UtcNow }).ToArray()).Score);
    }
}
