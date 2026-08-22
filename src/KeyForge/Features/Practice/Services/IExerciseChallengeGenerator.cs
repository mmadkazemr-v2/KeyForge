namespace KeyForge.Features.Practice.Services;

/// <summary>Generates a runtime challenge from an exercise definition.</summary>
public interface IExerciseChallengeGenerator
{
    ExerciseChallenge Create(ExerciseDefinition exercise);
}
