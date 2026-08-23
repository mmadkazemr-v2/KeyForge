namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Routes supported exercise definitions to their focused challenge generators.
/// </summary>
public sealed class ExerciseChallengeGenerator : IExerciseChallengeGenerator
{
    public ExerciseChallenge Create(ExerciseDefinition exercise, int? selectedBpm = null)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        if (exercise is RhythmExerciseDefinition rhythm)
        {
            return new RhythmChallengeGenerator().Create(rhythm, selectedBpm ?? rhythm.Tempo?.DefaultBpm);
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

        if (exercise is MentalKeyboardExerciseDefinition mentalKeyboard)
        {
            return new MentalKeyboardChallengeGenerator().Create(mentalKeyboard);
        }

        if (exercise is EarTrainingExerciseDefinition earTraining)
        {
            return new EarTrainingChallengeGenerator().Create(earTraining);
        }

        return new ExerciseChallenge
        {
            ExerciseId = exercise.Id,
            ExerciseType = exercise.Type,
            ExpectedInputs = []
        };
    }
}
