namespace KeyForge.Features.Practice.Services;

/// <summary>
/// Compiles authored musical durations into a runtime rhythm timeline.
/// </summary>
public sealed class RhythmChallengeGenerator
{
    public ExerciseChallenge Create(RhythmExerciseDefinition exercise, int? selectedBpm = null)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        exercise.Validate();

        var bpm = selectedBpm ?? exercise.Tempo?.DefaultBpm ?? 0;
        if (bpm <= 0)
            throw new InvalidOperationException($"Rhythm exercise '{exercise.Id}', field 'tempo.defaultBpm' must be greater than zero.");

        var meter = RhythmMeter.Parse(exercise.TimeSignature);
        var timeline = exercise.Pattern is not null || exercise.Measures is not null
            ? CompileAuthoredPattern(exercise, meter, bpm)
            : CompileLegacyPulsePattern(exercise, meter, bpm);

        var expectedInputs = timeline.Events
            .Where(item => !item.IsRest)
            .Select(item => new ExpectedInput
            {
                Type = InputEventType.Beat,
                Value = "beat",
                ExpectedOffset = item.StartOffset
            })
            .ToArray();

        return new ExerciseChallenge
        {
            ExerciseId = exercise.Id,
            ExerciseType = ExerciseType.Rhythm,
            ExpectedInputs = expectedInputs,
            Rhythm = timeline
        };
    }

    public static TimeSpan CalculateBeatInterval(int beatsPerMinute)
    {
        if (beatsPerMinute <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(beatsPerMinute),
                "Beats per minute must be greater than zero.");
        }

        return TimeSpan.FromMilliseconds(60_000d / beatsPerMinute);
    }

    public static int GetBeatsPerPattern(string timeSignature) => RhythmMeter.Parse(timeSignature).Numerator;

    private static RhythmChallengeDetails CompileLegacyPulsePattern(
        RhythmExerciseDefinition exercise,
        RhythmMeter meter,
        int bpm)
    {
        // Legacy lessons historically treated every numerator unit as a quarter-note pulse.
        var pulseCount = checked(meter.Numerator * exercise.PatternCount);
        var events = Enumerable.Range(0, pulseCount)
            .Select(index => new RhythmTimelineEvent
            {
                Duration = RhythmDuration.Quarter,
                IsRest = false,
                StartOffset = ToTimeSpan(index * 4, bpm),
                Length = ToTimeSpan(4, bpm),
                MeasureNumber = (index / meter.Numerator) + 1,
                StartSixteenthUnits = index * 4
            })
            .ToArray();
        var pulseOffsets = events.Select(item => item.StartOffset).ToArray();
        return new RhythmChallengeDetails
        {
            Events = events,
            PulseOffsets = pulseOffsets,
            Duration = ToTimeSpan(pulseCount * 4, bpm)
        };
    }

    private static RhythmChallengeDetails CompileAuthoredPattern(
        RhythmExerciseDefinition exercise,
        RhythmMeter meter,
        int bpm)
    {
        var measures = exercise.Measures ?? [new RhythmMeasureDefinition { Pattern = exercise.Pattern! }];
        var timelineEvents = new List<RhythmTimelineEvent>();
        var pulseOffsets = new List<TimeSpan>();
        var totalUnits = 0;
        for (var measureIndex = 0; measureIndex < measures.Count; measureIndex++)
        {
            var measure = measures[measureIndex];
            var positionUnits = 0;
            foreach (var item in measure.Pattern)
            {
                var durationUnits = RhythmDurationUnits.GetSixteenthUnits(item.Duration);
                timelineEvents.Add(new RhythmTimelineEvent
                {
                    Duration = item.Duration,
                    IsRest = item.Rest,
                    StartOffset = ToTimeSpan(totalUnits + positionUnits, bpm),
                    Length = ToTimeSpan(durationUnits, bpm),
                    MeasureNumber = measureIndex + 1,
                    StartSixteenthUnits = totalUnits + positionUnits
                });
                positionUnits += durationUnits;
            }

            for (var beatUnits = 0; beatUnits < meter.BarSixteenthUnits; beatUnits += meter.BeatSixteenthUnits)
                pulseOffsets.Add(ToTimeSpan(totalUnits + beatUnits, bpm));
            totalUnits += meter.BarSixteenthUnits;
        }

        return new RhythmChallengeDetails
        {
            Events = timelineEvents,
            PulseOffsets = pulseOffsets,
            Duration = ToTimeSpan(totalUnits, bpm)
        };
    }

    private static TimeSpan ToTimeSpan(int sixteenthUnits, int bpm)
    {
        var ticks = decimal.Round(
            (decimal)TimeSpan.TicksPerMinute * sixteenthUnits / (bpm * 4m),
            0,
            MidpointRounding.ToEven);
        return TimeSpan.FromTicks((long)ticks);
    }
}
