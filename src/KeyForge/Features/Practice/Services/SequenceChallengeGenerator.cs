namespace KeyForge.Features.Practice.Services;

/// <summary>Generates a generic ordered-pitch challenge from YAML-authored notes or scale settings.</summary>
public sealed class SequenceChallengeGenerator
{
    public ExerciseChallenge Create(SequenceExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        var notes = exercise.Scale is not null
            ? PitchSequenceBuilder.BuildScale(exercise.Scale, exercise.Id)
            : exercise.Sequence is not null
                ? PitchSequenceBuilder.ParsePitches(exercise.Sequence, exercise.Id, "sequence")
                : throw new InvalidOperationException(
                    $"Sequence exercise '{exercise.Id}' must define either 'sequence' or 'scale'.");

        return new ExerciseChallenge
        {
            ExerciseId = exercise.Id,
            ExerciseType = exercise.Type,
            ExpectedInputs = notes.Select(note => new ExpectedInput { Type = InputEventType.Note, Value = note }).ToArray(),
            Sequence = new SequenceChallengeDetails
            {
                ExpectedNotes = notes,
                Hand = exercise.Hand,
                Fingers = exercise.Fingers.ToArray()
            }
        };
    }
}
