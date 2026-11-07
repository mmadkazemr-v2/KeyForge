namespace KeyForge.Features.Practice.Services;

/// <summary>Generates a generic ordered-pitch challenge from YAML-authored notes or scale settings.</summary>
public sealed class SequenceChallengeGenerator
{
    public ExerciseChallenge Create(SequenceExerciseDefinition exercise, int? selectedBpm = null)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        if (exercise.Performance is { } performance)
        {
            performance.Validate();
            var bpm = selectedBpm ?? exercise.Tempo?.DefaultBpm ?? 0;
            if (bpm <= 0) throw new InvalidOperationException("Performance requires a positive quarter-note tempo.");
            var events = performance.Events.Select(e => new PerformanceEvent(e.Pitch is null ? null : MusicalNote.Parse(e.Pitch).Value,
                TimeSpan.FromMinutes(e.OnsetBeats / bpm), TimeSpan.FromMinutes(e.DurationBeats / bpm), e.Voice, e.Hand)).ToArray();
            return new ExerciseChallenge
            {
                ExerciseId = exercise.Id,
                ExerciseType = exercise.Type,
                ExpectedInputs = events.Where(e => e.Pitch is not null).OrderBy(e => e.Onset).Select(e => new ExpectedInput
                { Type = InputEventType.Note, Value = e.Pitch!, ExpectedOffset = e.Onset }).ToArray(),
                Performance = new PerformanceChallengeDetails
                {
                    Events = events,
                    TimingReference = performance.TimingReference,
                    ScoringMode = performance.ScoringMode,
                    ReleaseToleranceMilliseconds = performance.ReleaseToleranceMilliseconds,
                    OnsetToleranceMilliseconds = performance.OnsetToleranceMilliseconds
                }
            };
        }
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
