namespace KeyForge.Components.Exercises;

/// <summary>
/// Maps exercise definitions to localized presentation metadata.
/// This mapping is presentation-only and does not evaluate or mutate exercises.
/// </summary>
public static class ExercisePresentationMapper
{
    public static IReadOnlyList<ExercisePresentationItem> Map(ExerciseDefinition exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        var items = new List<ExercisePresentationItem>();
        AddCommonMetadata(items, exercise, exercise is SpeedExerciseDefinition);

        switch (exercise)
        {
            case RhythmExerciseDefinition rhythm:
                AddRhythmMetadata(items, rhythm);
                break;
            case NoteReadingExerciseDefinition noteReading:
                AddNoteReadingMetadata(items, noteReading);
                break;
            case EarTrainingExerciseDefinition earTraining:
                AddEarTrainingMetadata(items, earTraining);
                break;
            case IntervalExerciseDefinition interval:
                AddIntervalMetadata(items, interval);
                break;
            case OctaveExerciseDefinition octave:
                AddOctaveMetadata(items, octave);
                break;
            case FingerIndependenceExerciseDefinition fingerIndependence:
                AddFingerMetadata(items, fingerIndependence);
                break;
            case MentalKeyboardExerciseDefinition mentalKeyboard:
                AddMentalKeyboardMetadata(items, mentalKeyboard);
                break;
            case SpeedExerciseDefinition speed:
                AddSpeedMetadata(items, speed);
                break;
        }

        return items.AsReadOnly();
    }

    private static void AddCommonMetadata(
        ICollection<ExercisePresentationItem> items,
        ExerciseDefinition exercise,
        bool usesStartingTempo)
    {
        items.Add(new ExercisePresentationItem(
            UiText.DifficultyLabel,
            UiText.GetDifficulty(exercise.Difficulty)));

        if (exercise.Duration > 0)
        {
            items.Add(new ExercisePresentationItem(
                UiText.Duration,
                $"{UiText.FormatNumber(exercise.Duration)} {UiText.Minute}"));
        }

        if (exercise.Tempo > 0)
        {
            items.Add(new ExercisePresentationItem(
                usesStartingTempo ? UiText.StartingTempo : UiText.Tempo,
                FormatTempo(exercise.Tempo)));
        }
    }

    private static void AddRhythmMetadata(
        ICollection<ExercisePresentationItem> items,
        RhythmExerciseDefinition exercise)
    {
        AddText(items, UiText.TimeSignature, exercise.TimeSignature, isTechnical: true);

        if (exercise.NoteValues.Count > 0)
        {
            items.Add(new ExercisePresentationItem(
                UiText.NoteValues,
                string.Join(UiText.ListSeparator, exercise.NoteValues.Select(UiText.GetNoteValue))));
        }

        AddPositiveNumber(items, UiText.PatternCount, exercise.PatternCount);
    }

    private static void AddNoteReadingMetadata(
        ICollection<ExercisePresentationItem> items,
        NoteReadingExerciseDefinition exercise)
    {
        items.Add(new ExercisePresentationItem(UiText.Clef, UiText.GetClef(exercise.Clef)));
        AddText(items, UiText.NoteRange, exercise.Range, isTechnical: true);
        AddText(items, UiText.KeySignature, exercise.KeySignature, isTechnical: true);
    }

    private static void AddEarTrainingMetadata(
        ICollection<ExercisePresentationItem> items,
        EarTrainingExerciseDefinition exercise)
    {
        items.Add(new ExercisePresentationItem(
            UiText.EarTrainingTaskLabel,
            UiText.GetEarTrainingTask(exercise.TaskType)));
        AddPositiveNumber(items, UiText.Rounds, exercise.Rounds);

        if (exercise.Keys.Count > 0)
        {
            items.Add(new ExercisePresentationItem(
                UiText.Keys,
                string.Join(UiText.ListSeparator, exercise.Keys),
                IsTechnical: true));
        }
    }

    private static void AddIntervalMetadata(
        ICollection<ExercisePresentationItem> items,
        IntervalExerciseDefinition exercise)
    {
        if (exercise.Intervals.Count > 0)
        {
            items.Add(new ExercisePresentationItem(
                UiText.Intervals,
                string.Join(UiText.ListSeparator, exercise.Intervals),
                IsTechnical: true));
        }

        items.Add(new ExercisePresentationItem(
            UiText.ExerciseDirectionLabel,
            UiText.GetExerciseDirection(exercise.Direction)));
        AddText(items, UiText.StartingNote, exercise.StartingNote, isTechnical: true);
    }

    private static void AddOctaveMetadata(
        ICollection<ExercisePresentationItem> items,
        OctaveExerciseDefinition exercise)
    {
        AddPositiveNumber(items, UiText.OctaveCount, exercise.Octaves);
        items.Add(new ExercisePresentationItem(
            UiText.ExerciseDirectionLabel,
            UiText.GetExerciseDirection(exercise.Direction)));
        AddText(items, UiText.StartingNote, exercise.StartingNote, isTechnical: true);
    }

    private static void AddFingerMetadata(
        ICollection<ExercisePresentationItem> items,
        FingerIndependenceExerciseDefinition exercise)
    {
        items.Add(new ExercisePresentationItem(UiText.Hand, UiText.GetPracticeHand(exercise.Hand)));
        items.Add(new ExercisePresentationItem(
            UiText.Fingers,
            exercise.Fingers.Count == 0
                ? UiText.AllFingers
                : string.Join(UiText.ListSeparator, exercise.Fingers.Select(UiText.FormatNumber))));
    }

    private static void AddMentalKeyboardMetadata(
        ICollection<ExercisePresentationItem> items,
        MentalKeyboardExerciseDefinition exercise)
    {
        AddText(items, UiText.StartingNote, exercise.StartingNote, isTechnical: true);
        items.Add(new ExercisePresentationItem(
            UiText.ExerciseDirectionLabel,
            UiText.GetExerciseDirection(exercise.Direction)));
        AddPositiveNumber(items, UiText.Steps, exercise.Steps);
    }

    private static void AddSpeedMetadata(
        ICollection<ExercisePresentationItem> items,
        SpeedExerciseDefinition exercise)
    {
        items.Add(new ExercisePresentationItem(
            UiText.SpeedPatternLabel,
            UiText.GetSpeedPattern(exercise.Pattern)));

        if (exercise.TargetTempo > 0)
        {
            items.Add(new ExercisePresentationItem(
                UiText.TargetTempo,
                FormatTempo(exercise.TargetTempo)));
        }

        AddPositiveNumber(items, UiText.Repetitions, exercise.Repetitions);
    }

    private static void AddPositiveNumber(
        ICollection<ExercisePresentationItem> items,
        string label,
        int value)
    {
        if (value > 0)
        {
            items.Add(new ExercisePresentationItem(label, UiText.FormatNumber(value)));
        }
    }

    private static void AddText(
        ICollection<ExercisePresentationItem> items,
        string label,
        string value,
        bool isTechnical = false)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            items.Add(new ExercisePresentationItem(label, value, isTechnical));
        }
    }

    private static string FormatTempo(int tempo) =>
        $"{UiText.FormatNumber(tempo)} {UiText.BeatsPerMinute}";
}
