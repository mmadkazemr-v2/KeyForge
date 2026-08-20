namespace KeyForge.Tests.Features.Practice.Services;

/// <summary>
/// Tests <see cref="PracticeSessionService"/> and <see cref="PracticeSession"/>
/// using in-memory fakes to verify practice session behaviour.
/// </summary>
public sealed class PracticeSessionServiceTests
{
    private static readonly RhythmExerciseDefinition Exercise1 = new()
    {
        Id = "ex-01",
        Title = "Quarter Notes",
        Type = ExerciseType.Rhythm
    };

    private static readonly NoteReadingExerciseDefinition Exercise2 = new()
    {
        Id = "ex-02",
        Title = "Treble Clef",
        Type = ExerciseType.NoteReading
    };

    private static readonly SpeedExerciseDefinition Exercise3 = new()
    {
        Id = "ex-03",
        Title = "Speed Drill",
        Type = ExerciseType.Speed
    };

    private static ILessonCatalog CreateCatalog(params LessonDefinition[] lessons)
    {
        return new FakeLessonCatalog(lessons);
    }

    private static PracticeSessionService CreateService(ILessonCatalog catalog) => new(catalog);

    [Fact]
    public void StartSession_ValidLesson_ReturnsSession()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Title = "Test Lesson",
            Exercises = [Exercise1, Exercise2]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog);

        var session = service.StartSession("lesson-01");

        Assert.NotNull(session);
        Assert.Equal("lesson-01", session.LessonId);
        Assert.Equal(2, session.Exercises.Count);
    }

    [Fact]
    public void StartSession_UnknownLesson_ReturnsNull()
    {
        var catalog = CreateCatalog();
        var service = CreateService(catalog);

        var session = service.StartSession("unknown");

        Assert.Null(session);
    }

    [Fact]
    public void StartSession_ExerciseOrderPreserved()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1, Exercise2, Exercise3]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog);

        var session = service.StartSession("lesson-01")!;

        Assert.Equal("ex-01", session.Exercises[0].Id);
        Assert.Equal("ex-02", session.Exercises[1].Id);
        Assert.Equal("ex-03", session.Exercises[2].Id);
    }

    [Fact]
    public void StartSession_FirstExerciseIsCurrent()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1, Exercise2]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog);

        var session = service.StartSession("lesson-01")!;

        Assert.Equal(0, session.CurrentExerciseIndex);
        Assert.Equal("ex-01", session.GetCurrentExercise()!.Id);
    }

    [Fact]
    public void Next_MovesToNextExercise()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1, Exercise2, Exercise3]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog);
        var session = service.StartSession("lesson-01")!;

        session.Next();

        Assert.Equal(1, session.CurrentExerciseIndex);
        Assert.Equal("ex-02", session.GetCurrentExercise()!.Id);
    }

    [Fact]
    public void Next_AtLastExercise_SessionFinished()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1, Exercise2]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog);
        var session = service.StartSession("lesson-01")!;

        session.Next();
        session.Next();

        Assert.True(session.IsFinished);
        Assert.Null(session.GetCurrentExercise());
    }

    [Fact]
    public void Next_PastEnd_DoesNotThrow()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog);
        var session = service.StartSession("lesson-01")!;

        session.Next();
        session.Next();
        session.Next();

        Assert.True(session.IsFinished);
    }

    [Fact]
    public void IsFinished_EmptyLesson_ReturnsTrue()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Exercises = []
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog);

        var session = service.StartSession("lesson-01")!;

        Assert.True(session.IsFinished);
        Assert.Null(session.GetCurrentExercise());
    }

    [Fact]
    public void StartSession_NullLessonId_ThrowsArgumentNullException()
    {
        var catalog = CreateCatalog();
        var service = CreateService(catalog);

        Assert.Throws<ArgumentNullException>(() => service.StartSession(null!));
    }

    [Fact]
    public void StartSession_DoesNotMutateLesson()
    {
        var lesson = new LessonDefinition
        {
            Id = "lesson-01",
            Title = "Original Title",
            Exercises = [Exercise1]
        };
        var catalog = CreateCatalog(lesson);
        var service = CreateService(catalog);

        service.StartSession("lesson-01");

        Assert.Equal("lesson-01", lesson.Id);
        Assert.Equal("Original Title", lesson.Title);
        Assert.Single(lesson.Exercises);
    }

    [Fact]
    public void PracticeSession_ImplementsIPracticeSessionService()
    {
        IPracticeSessionService service = new PracticeSessionService(CreateCatalog());

        Assert.NotNull(service);
    }

    private sealed class FakeLessonCatalog : ILessonCatalog
    {
        private readonly Dictionary<string, LessonDefinition> _lessons;

        public FakeLessonCatalog(LessonDefinition[] lessons)
        {
            _lessons = lessons.ToDictionary(l => l.Id, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<LessonDefinition> GetAll() => _lessons.Values.ToList().AsReadOnly();

        public LessonDefinition? GetById(string id) =>
            _lessons.TryGetValue(id, out var lesson) ? lesson : null;
    }
}
