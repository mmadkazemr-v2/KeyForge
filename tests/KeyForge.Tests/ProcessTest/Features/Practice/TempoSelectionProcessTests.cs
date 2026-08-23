using KeyForge.Components.Practice;

namespace KeyForge.Tests.ProcessTest.Features.Practice;

public sealed class TempoSelectionProcessTests
{
    [Fact]
    public void SelectedTempo_StartRetryAndAttempt_UsesRuntimeOnlySelection()
    {
        var exercise = new RhythmExerciseDefinition
        {
            Id = "rhythm",
            Tempo = new TempoConfiguration
            {
                DefaultBpm = 80,
                AllowUserChange = true,
                MinBpm = 60,
                MaxBpm = 120,
                Step = 5,
                SuggestedBpms = [60, 80, 100]
            },
            Metronome = new MetronomeConfiguration { Enabled = true, CountInBeats = 3 },
            TimeSignature = "4/4",
            PatternCount = 1
        };
        var generator = new RhythmChallengeGenerator();
        var runtime = new RuntimeExercise(
            exercise,
            generator.Create(exercise, 80),
            DateTime.UtcNow,
            80,
            bpm => generator.Create(exercise, bpm));

        runtime.SetSelectedBpm(100);

        Assert.Equal(100, runtime.SelectedBpm);
        Assert.Equal(600, runtime.Challenge.ExpectedInputs[1].ExpectedOffset!.Value.TotalMilliseconds);
        Assert.Equal(80, exercise.Tempo.DefaultBpm);
        Assert.Equal([1, 2, 3], MetronomeTiming.GetCountInBeats(exercise.Metronome.CountInBeats));

        // A retry creates a fresh runtime, then reapplies the runtime-only selection.
        var retry = new RuntimeExercise(
            exercise,
            generator.Create(exercise, 80),
            DateTime.UtcNow,
            80,
            bpm => generator.Create(exercise, bpm));
        retry.SetSelectedBpm(runtime.SelectedBpm!.Value);

        Assert.Equal(100, retry.SelectedBpm);
        Assert.Equal(600, retry.Challenge.ExpectedInputs[1].ExpectedOffset!.Value.TotalMilliseconds);
        Assert.Equal(80, exercise.Tempo.DefaultBpm);
    }
}
