using KeyForge.Features.Exercises.Models;
using KeyForge.Features.Lessons.Models;
using KeyForge.Infrastructure.Yaml.Parsing;
using YamlDotNet.Core;

namespace KeyForge.Tests.Infrastructure.Yaml.Parsing;

public class YamlLessonParserTests
{
    private readonly IYamlLessonParser _parser = new YamlLessonParser();

    [Fact]
    public void Parse_MinimalLesson_ReturnsLessonDefinition()
    {
        const string yaml = """
            id: lesson-01
            title: "Keyboard Foundations"
            level: beginner
            order: 1
            """;

        var lesson = _parser.Parse(yaml);

        Assert.Equal("lesson-01", lesson.Id);
        Assert.Equal("Keyboard Foundations", lesson.Title);
        Assert.Equal(LessonLevel.Beginner, lesson.Level);
        Assert.Equal(1, lesson.Order);
        Assert.Equal(UnlockMode.Immediate, lesson.Unlock.Mode);
        Assert.Empty(lesson.Exercises);
    }

    [Fact]
    public void Parse_LessonMetadata_CorrectlyMaps()
    {
        const string yaml = """
            id: lesson-01
            title: "Keyboard Foundations"
            description: "Build the foundations required for keyboard playing."
            level: beginner
            order: 1
            estimatedMinutes: 30
            """;

        var lesson = _parser.Parse(yaml);

        Assert.Equal("lesson-01", lesson.Id);
        Assert.Equal("Keyboard Foundations", lesson.Title);
        Assert.Equal("Build the foundations required for keyboard playing.", lesson.Description);
        Assert.Equal(LessonLevel.Beginner, lesson.Level);
        Assert.Equal(1, lesson.Order);
        Assert.Equal(30, lesson.EstimatedMinutes);
    }

    [Fact]
    public void Parse_ImmediateUnlock_CorrectlyMaps()
    {
        const string yaml = """
            id: lesson-01
            unlock:
              mode: immediate
            """;

        var lesson = _parser.Parse(yaml);

        Assert.Equal(UnlockMode.Immediate, lesson.Unlock.Mode);
        Assert.Empty(lesson.Unlock.RequiredLessonIds);
    }

    [Fact]
    public void Parse_PrerequisiteUnlock_CorrectlyMaps()
    {
        const string yaml = """
            id: lesson-03
            unlock:
              mode: prerequisitesCompleted
              requiredLessonIds:
                - lesson-01
                - lesson-02
            """;

        var lesson = _parser.Parse(yaml);

        Assert.Equal(UnlockMode.PrerequisitesCompleted, lesson.Unlock.Mode);
        Assert.Equal(new[] { "lesson-01", "lesson-02" }, lesson.Unlock.RequiredLessonIds);
    }

    [Fact]
    public void Parse_CompletionRule_CorrectlyMaps()
    {
        const string yaml = """
            id: lesson-01
            completion:
              minimumScore: 70
              requireAllExercises: true
            """;

        var lesson = _parser.Parse(yaml);

        var minimumScore = lesson.Completion.MinimumScore;
        Assert.NotNull(minimumScore);
        Assert.Equal(70, minimumScore.Value);
        Assert.True(lesson.Completion.RequireAllExercises);
    }

    [Fact]
    public void Parse_RhythmExercise_ReturnsRhythmExerciseDefinition()
    {
        const string yaml = """
            id: lesson-01
            exercises:
              - id: rhythm-01
                type: rhythm
                title: "Quarter Note Pulse"
                difficulty: easy
                timeSignature: "4/4"
                noteValues: [quarter, eighth, sixteenth]
                patternCount: 4
            """;

        var lesson = _parser.Parse(yaml);

        var exercise = Assert.IsType<RhythmExerciseDefinition>(Assert.Single(lesson.Exercises));
        Assert.Equal(ExerciseType.Rhythm, exercise.Type);
        Assert.Equal("rhythm-01", exercise.Id);
        Assert.Equal("4/4", exercise.TimeSignature);
        Assert.Equal(new[] { "quarter", "eighth", "sixteenth" }, exercise.NoteValues);
        Assert.Equal(4, exercise.PatternCount);
    }

    [Fact]
    public void Parse_NoteReadingExercise_ReturnsNoteReadingExerciseDefinition()
    {
        const string yaml = """
            id: lesson-01
            exercises:
              - id: notes-01
                type: noteReading
                clef: treble
                range: "C4-G5"
                keySignature: "C major"
            """;

        var lesson = _parser.Parse(yaml);

        var exercise = Assert.IsType<NoteReadingExerciseDefinition>(Assert.Single(lesson.Exercises));
        Assert.Equal(ExerciseType.NoteReading, exercise.Type);
        Assert.Equal(ClefType.Treble, exercise.Clef);
        Assert.Equal("C4-G5", exercise.Range);
        Assert.Equal("C major", exercise.KeySignature);
    }

    [Fact]
    public void Parse_EarTrainingExercise_ReturnsEarTrainingExerciseDefinition()
    {
        const string yaml = """
            id: lesson-01
            exercises:
              - id: ear-01
                type: earTraining
                taskType: chordRecognition
                rounds: 10
                keys: [C major, G major]
            """;

        var lesson = _parser.Parse(yaml);

        var exercise = Assert.IsType<EarTrainingExerciseDefinition>(Assert.Single(lesson.Exercises));
        Assert.Equal(ExerciseType.EarTraining, exercise.Type);
        Assert.Equal(EarTrainingTask.ChordRecognition, exercise.TaskType);
        Assert.Equal(10, exercise.Rounds);
        Assert.Equal(new[] { "C major", "G major" }, exercise.Keys);
    }

    [Fact]
    public void Parse_IntervalExercise_ReturnsIntervalExerciseDefinition()
    {
        const string yaml = """
            id: lesson-01
            exercises:
              - id: interval-01
                type: interval
                intervals: [M2, M3, P5]
                direction: ascending
                startingNote: "C4"
            """;

        var lesson = _parser.Parse(yaml);

        var exercise = Assert.IsType<IntervalExerciseDefinition>(Assert.Single(lesson.Exercises));
        Assert.Equal(ExerciseType.Interval, exercise.Type);
        Assert.Equal(new[] { "M2", "M3", "P5" }, exercise.Intervals);
        Assert.Equal(ExerciseDirection.Ascending, exercise.Direction);
        Assert.Equal("C4", exercise.StartingNote);
    }

    [Fact]
    public void Parse_OctaveExercise_ReturnsOctaveExerciseDefinition()
    {
        const string yaml = """
            id: lesson-01
            exercises:
              - id: octave-01
                type: octave
                octaves: 2
                direction: descending
                startingNote: "C3"
            """;

        var lesson = _parser.Parse(yaml);

        var exercise = Assert.IsType<OctaveExerciseDefinition>(Assert.Single(lesson.Exercises));
        Assert.Equal(ExerciseType.Octave, exercise.Type);
        Assert.Equal(2, exercise.Octaves);
        Assert.Equal(ExerciseDirection.Descending, exercise.Direction);
        Assert.Equal("C3", exercise.StartingNote);
    }

    [Fact]
    public void Parse_FingerIndependenceExercise_ReturnsFingerIndependenceExerciseDefinition()
    {
        const string yaml = """
            id: lesson-01
            exercises:
              - id: fingers-01
                type: fingerIndependence
                fingers: [3, 4]
                hand: right
            """;

        var lesson = _parser.Parse(yaml);

        var exercise = Assert.IsType<FingerIndependenceExerciseDefinition>(Assert.Single(lesson.Exercises));
        Assert.Equal(ExerciseType.FingerIndependence, exercise.Type);
        Assert.Equal(new[] { 3, 4 }, exercise.Fingers);
        Assert.Equal(PracticeHand.Right, exercise.Hand);
    }

    [Fact]
    public void Parse_MentalKeyboardExercise_ReturnsMentalKeyboardExerciseDefinition()
    {
        const string yaml = """
            id: lesson-01
            exercises:
              - id: mental-01
                type: mentalKeyboard
                startingNote: "C4"
                direction: ascending
                steps: 4
            """;

        var lesson = _parser.Parse(yaml);

        var exercise = Assert.IsType<MentalKeyboardExerciseDefinition>(Assert.Single(lesson.Exercises));
        Assert.Equal(ExerciseType.MentalKeyboard, exercise.Type);
        Assert.Equal("C4", exercise.StartingNote);
        Assert.Equal(ExerciseDirection.Ascending, exercise.Direction);
        Assert.Equal(4, exercise.Steps);
    }

    [Fact]
    public void Parse_SpeedExercise_ReturnsSpeedExerciseDefinition()
    {
        const string yaml = """
            id: lesson-01
            exercises:
              - id: speed-01
                type: speed
                pattern: scale
                targetTempo: 120
                repetitions: 12
            """;

        var lesson = _parser.Parse(yaml);

        var exercise = Assert.IsType<SpeedExerciseDefinition>(Assert.Single(lesson.Exercises));
        Assert.Equal(ExerciseType.Speed, exercise.Type);
        Assert.Equal(SpeedPattern.Scale, exercise.Pattern);
        Assert.Equal(120, exercise.TargetTempo);
        Assert.Equal(12, exercise.Repetitions);
    }

    [Fact]
    public void Parse_MixedExercises_ReturnsCorrectConcreteTypes()
    {
        const string yaml = """
            id: lesson-01
            title: "Mixed Exercises"
            exercises:
              - { id: ex-01, type: rhythm }
              - { id: ex-02, type: noteReading }
              - { id: ex-03, type: earTraining }
              - { id: ex-04, type: interval }
              - { id: ex-05, type: octave }
              - { id: ex-06, type: fingerIndependence }
              - { id: ex-07, type: mentalKeyboard }
              - { id: ex-08, type: speed }
            """;

        var lesson = _parser.Parse(yaml);

        Assert.Equal(8, lesson.Exercises.Count);
        Assert.IsType<RhythmExerciseDefinition>(lesson.Exercises[0]);
        Assert.IsType<NoteReadingExerciseDefinition>(lesson.Exercises[1]);
        Assert.IsType<EarTrainingExerciseDefinition>(lesson.Exercises[2]);
        Assert.IsType<IntervalExerciseDefinition>(lesson.Exercises[3]);
        Assert.IsType<OctaveExerciseDefinition>(lesson.Exercises[4]);
        Assert.IsType<FingerIndependenceExerciseDefinition>(lesson.Exercises[5]);
        Assert.IsType<MentalKeyboardExerciseDefinition>(lesson.Exercises[6]);
        Assert.IsType<SpeedExerciseDefinition>(lesson.Exercises[7]);
    }

    [Fact]
    public void Parse_EnumValues_FromStrings()
    {
        const string yaml = """
            id: lesson-02
            title: "Enum Strings"
            level: intermediate
            unlock:
              mode: previousLessonCompleted
            completion:
              minimumScore: 80
              requireAllExercises: true
            exercises:
              - id: ex-01
                type: noteReading
                difficulty: medium
                clef: bass
              - id: ex-02
                type: interval
                difficulty: hard
                direction: descending
            """;

        var lesson = _parser.Parse(yaml);

        Assert.Equal(LessonLevel.Intermediate, lesson.Level);
        Assert.Equal(UnlockMode.PreviousLessonCompleted, lesson.Unlock.Mode);
        var minimumScore = lesson.Completion.MinimumScore;
        Assert.NotNull(minimumScore);
        Assert.Equal(80, minimumScore.Value);
        Assert.True(lesson.Completion.RequireAllExercises);

        var noteReading = Assert.IsType<NoteReadingExerciseDefinition>(lesson.Exercises[0]);
        Assert.Equal(Difficulty.Medium, noteReading.Difficulty);
        Assert.Equal(ClefType.Bass, noteReading.Clef);

        var interval = Assert.IsType<IntervalExerciseDefinition>(lesson.Exercises[1]);
        Assert.Equal(Difficulty.Hard, interval.Difficulty);
        Assert.Equal(ExerciseDirection.Descending, interval.Direction);
    }

    [Fact]
    public void Parse_UnknownExerciseType_ThrowsMeaningfulException()
    {
        const string yaml = """
            id: lesson-01
            exercises:
              - id: ex-01
                type: somethingUnknown
            """;

        var ex = Assert.Throws<UnknownExerciseTypeException>(() => _parser.Parse(yaml));

        Assert.Contains("somethingUnknown", ex.Message);
        Assert.Equal("somethingUnknown", ex.UnknownType);
    }

    [Fact]
    public void Parse_InvalidYaml_ThrowsMeaningfulException()
    {
        const string yaml = """
            id: lesson-01
            title: "unterminated
            """;

        var ex = Assert.Throws<YamlLessonParseException>(() => _parser.Parse(yaml));

        Assert.Contains("lesson", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.IsAssignableFrom<YamlException>(ex.InnerException);
    }

    [Fact]
    public void Parse_EmptyYaml_ThrowsMeaningfulException()
    {
        var emptyEx = Assert.Throws<YamlLessonParseException>(() => _parser.Parse(string.Empty));
        Assert.Contains("empty", emptyEx.Message, StringComparison.OrdinalIgnoreCase);

        var whitespaceEx = Assert.Throws<YamlLessonParseException>(() => _parser.Parse("   \n\n  "));
        Assert.Contains("empty", whitespaceEx.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_MissingExerciseType_ThrowsMeaningfulException()
    {
        const string yaml = """
            id: lesson-01
            exercises:
              - id: ex-01
                title: "No Type"
            """;

        var ex = Assert.Throws<YamlLessonParseException>(() => _parser.Parse(yaml));

        Assert.Contains("type", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_ExerciseSpecificProperties_CorrectlyMaps()
    {
        const string yaml = """
            id: lesson-01
            title: "Full Rhythm"
            exercises:
              - id: rhythm-01
                type: rhythm
                title: "Quarter Note Pulse"
                description: "Practice a steady quarter-note pulse."
                duration: 5
                tempo: 60
                difficulty: easy
                timeSignature: "6/8"
                noteValues: [quarter, eighth]
                patternCount: 4
            """;

        var lesson = _parser.Parse(yaml);

        var exercise = Assert.IsType<RhythmExerciseDefinition>(Assert.Single(lesson.Exercises));
        Assert.Equal(ExerciseType.Rhythm, exercise.Type);
        Assert.Equal("rhythm-01", exercise.Id);
        Assert.Equal("Quarter Note Pulse", exercise.Title);
        Assert.Equal("Practice a steady quarter-note pulse.", exercise.Description);
        Assert.Equal(5, exercise.Duration);
        Assert.Equal(60, exercise.Tempo);
        Assert.Equal(Difficulty.Easy, exercise.Difficulty);
        Assert.Equal("6/8", exercise.TimeSignature);
        Assert.Equal(new[] { "quarter", "eighth" }, exercise.NoteValues);
        Assert.Equal(4, exercise.PatternCount);
    }
}
