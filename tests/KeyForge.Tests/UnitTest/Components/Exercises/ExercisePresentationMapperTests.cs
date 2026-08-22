using System.Globalization;
using KeyForge.Components.Exercises;

namespace KeyForge.Tests.UnitTest.Components.Exercises;

public sealed class ExercisePresentationMapperTests
{
    [Fact]
    public void Rhythm_MapsRhythmConfigurationInEnglish()
    {
        using var _ = new CultureScope("en");
        var exercise = new RhythmExerciseDefinition
        {
            Difficulty = Difficulty.Medium,
            Duration = 5,
            Tempo = 84,
            TimeSignature = "4/4",
            NoteValues = ["quarter", "eighth"],
            PatternCount = 4
        };

        var items = ExercisePresentationMapper.Map(exercise);

        AssertItem(items, "Difficulty", "Medium");
        AssertItem(items, "Tempo", "84 BPM");
        AssertItem(items, "Time signature", "4/4", isTechnical: true);
        AssertItem(items, "Note values", "Quarter, Eighth");
        AssertItem(items, "Pattern count", "4");
    }

    [Fact]
    public void NoteReading_MapsClefRangeAndKeySignature()
    {
        using var _ = new CultureScope("en");
        var exercise = new NoteReadingExerciseDefinition
        {
            Clef = ClefType.Grand,
            Range = "C3-G5",
            KeySignature = "C major"
        };

        var items = ExercisePresentationMapper.Map(exercise);

        AssertItem(items, "Clef", "Grand staff");
        AssertItem(items, "Note range", "C3-G5", isTechnical: true);
        AssertItem(items, "Key signature", "C major", isTechnical: true);
    }

    [Fact]
    public void EarTraining_MapsLocalizedPersianTaskRoundsAndKeys()
    {
        using var _ = new CultureScope("fa");
        var exercise = new EarTrainingExerciseDefinition
        {
            TaskType = EarTrainingTask.ChordRecognition,
            Rounds = 10,
            Keys = ["C major", "G major"]
        };

        var items = ExercisePresentationMapper.Map(exercise);

        AssertItem(items, "نوع تمرین شنیداری", "تشخیص آکورد");
        AssertItem(items, "تعداد دورها", "۱۰");
        AssertItem(items, "گام‌ها", "C major، G major", isTechnical: true);
    }

    [Fact]
    public void Interval_MapsIntervalsDirectionAndStartingNote()
    {
        using var _ = new CultureScope("en");
        var exercise = new IntervalExerciseDefinition
        {
            Intervals = ["M3", "P5"],
            Direction = ExerciseDirection.Descending,
            StartingNote = "C4"
        };

        var items = ExercisePresentationMapper.Map(exercise);

        AssertItem(items, "Intervals", "M3, P5", isTechnical: true);
        AssertItem(items, "Direction", "Descending");
        AssertItem(items, "Starting note", "C4", isTechnical: true);
    }

    [Fact]
    public void Octave_MapsOctaveConfiguration()
    {
        using var _ = new CultureScope("en");
        var exercise = new OctaveExerciseDefinition
        {
            Octaves = 2,
            Direction = ExerciseDirection.Both,
            StartingNote = "C3"
        };

        var items = ExercisePresentationMapper.Map(exercise);

        AssertItem(items, "Octave count", "2");
        AssertItem(items, "Direction", "Both directions");
        AssertItem(items, "Starting note", "C3", isTechnical: true);
    }

    [Fact]
    public void FingerIndependence_MapsPersianHandAndFingerNumbers()
    {
        using var _ = new CultureScope("fa");
        var exercise = new FingerIndependenceExerciseDefinition
        {
            Hand = PracticeHand.Both,
            Fingers = [2, 3, 4]
        };

        var items = ExercisePresentationMapper.Map(exercise);

        AssertItem(items, "دست", "هر دو دست");
        AssertItem(items, "انگشت‌ها", "۲، ۳، ۴");
    }

    [Fact]
    public void MentalKeyboard_MapsNavigationConfiguration()
    {
        using var _ = new CultureScope("en");
        var exercise = new MentalKeyboardExerciseDefinition
        {
            StartingNote = "G3",
            Direction = ExerciseDirection.Ascending,
            Steps = 6
        };

        var items = ExercisePresentationMapper.Map(exercise);

        AssertItem(items, "Starting note", "G3", isTechnical: true);
        AssertItem(items, "Direction", "Ascending");
        AssertItem(items, "Steps", "6");
    }

    [Fact]
    public void Speed_MapsStartingAndTargetTempoPatternAndRepetitions()
    {
        using var _ = new CultureScope("en");
        var exercise = new SpeedExerciseDefinition
        {
            Tempo = 80,
            TargetTempo = 120,
            Pattern = SpeedPattern.Arpeggio,
            Repetitions = 12
        };

        var items = ExercisePresentationMapper.Map(exercise);

        AssertItem(items, "Starting tempo", "80 BPM");
        AssertItem(items, "Target tempo", "120 BPM");
        AssertItem(items, "Speed pattern", "Arpeggio");
        AssertItem(items, "Repetitions", "12");
    }

    [Fact]
    public void UnknownSubtype_FallsBackToCommonMetadata()
    {
        using var _ = new CultureScope("en");
        var exercise = new UnknownExerciseDefinition
        {
            Difficulty = Difficulty.Hard,
            Duration = 3,
            Tempo = 70
        };

        var items = ExercisePresentationMapper.Map(exercise);

        Assert.Equal(3, items.Count);
        AssertItem(items, "Difficulty", "Hard");
        AssertItem(items, "Duration", "3 min");
        AssertItem(items, "Tempo", "70 BPM");
    }

    private static void AssertItem(
        IReadOnlyList<ExercisePresentationItem> items,
        string label,
        string value,
        bool isTechnical = false)
    {
        var item = Assert.Single(items, item => item.Label == label);
        Assert.Equal(value, item.Value);
        Assert.Equal(isTechnical, item.IsTechnical);
    }

    private sealed class UnknownExerciseDefinition : ExerciseDefinition
    {
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;

        public CultureScope(string cultureName)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUiCulture;
        }
    }
}
