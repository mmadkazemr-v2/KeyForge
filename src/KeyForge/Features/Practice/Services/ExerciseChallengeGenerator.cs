namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Routes supported exercise definitions to their focused challenge generators.
/// </summary>
public sealed class ExerciseChallengeGenerator : IExerciseChallengeGenerator
{
    public ExerciseChallenge Create(ExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        if (exercise is RhythmExerciseDefinition rhythm)
        {
            return new RhythmChallengeGenerator().Create(rhythm);
        }

        if (exercise is NoteReadingExerciseDefinition noteReading)
        {
            return new NoteReadingChallengeGenerator().Create(noteReading);
        }

        if (exercise is IntervalExerciseDefinition interval)
        {
            return new IntervalChallengeGenerator().Create(interval);
        }

        if (exercise is OctaveExerciseDefinition octave)
        {
            return new OctaveChallengeGenerator().Create(octave);
        }

        return new ExerciseChallenge
        {
            ExerciseId = exercise.Id,
            ExerciseType = exercise.Type,
            ExpectedInputs = []
        };
    }
}
