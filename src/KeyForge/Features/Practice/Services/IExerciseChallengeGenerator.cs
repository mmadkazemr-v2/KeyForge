namespace KeyForge.Features.Practice.Services;

/// <summary>Generates a runtime challenge from an exercise definition.</summary>
public interface IExerciseChallengeGenerator
{
    public ExerciseChallenge Create(ExerciseDefinition exercise, int? selectedBpm = null);
}
