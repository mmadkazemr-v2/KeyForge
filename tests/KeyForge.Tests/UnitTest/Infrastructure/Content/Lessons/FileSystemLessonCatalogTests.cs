namespace KeyForge.Tests.UnitTest.Infrastructure.Content.Lessons;

/// <summary>
/// Tests the <see cref="FileSystemLessonCatalog"/> against isolated temporary
/// directories. Each test gets its own directory which is removed afterwards,
/// so the real project Content directory is never touched.
/// </summary>
public sealed class FileSystemLessonCatalogTests : IDisposable
{
    private readonly string _contentDirectory;

    public FileSystemLessonCatalogTests()
    {
        _contentDirectory = Path.Combine(Path.GetTempPath(), "keyforge-lesson-catalog-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_contentDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_contentDirectory))
        {
            Directory.Delete(_contentDirectory, recursive: true);
        }
    }

    [Fact]
    public void GetAll_ReturnsAllLessons()
    {
        WriteLessonFile("lesson-01.yaml", "lesson-01", order: 1);
        WriteLessonFile("lesson-02.yaml", "lesson-02", order: 2);
        WriteLessonFile("lesson-03.yaml", "lesson-03", order: 3);

        var catalog = CreateCatalog();

        Assert.Equal(3, catalog.GetAll().Count);
    }

    [Fact]
    public void GetAll_ReturnsLessonsOrderedByLessonOrder()
    {
        WriteLessonFile("z.yaml", "z-lesson", order: 1);
        WriteLessonFile("a.yaml", "a-lesson", order: 3);
        WriteLessonFile("m.yaml", "m-lesson", order: 2);

        var catalog = CreateCatalog();

        var ids = catalog.GetAll().Select(lesson => lesson.Id).ToArray();
        Assert.Equal(new[] { "z-lesson", "m-lesson", "a-lesson" }, ids);
    }

    [Fact]
    public void GetAll_DiscoversRootAndNestedYamlInDeterministicOrder()
    {
        WriteLessonFile("root.yaml", "root-lesson", order: 2);
        WriteLessonFile(Path.Combine("LentoVivo", "Session04.yaml"), "lento-session-04", order: 1);

        var catalog = CreateCatalog();

        Assert.Equal(
            ["lento-session-04", "root-lesson"],
            catalog.GetAll().Select(lesson => lesson.Id));
        Assert.NotNull(catalog.GetById("lento-session-04"));
    }

    [Fact]
    public void GetById_ReturnsCorrectLesson()
    {
        WriteLessonFile("lesson-real.yaml", "lesson-real-id", order: 1);

        var catalog = CreateCatalog();

        var lesson = catalog.GetById("lesson-real-id");
        Assert.NotNull(lesson);
        Assert.Equal("lesson-real-id", lesson.Id);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        WriteLessonFile("lesson-01.yaml", "lesson-01", order: 1);

        var catalog = CreateCatalog();

        Assert.Null(catalog.GetById("does-not-exist"));
    }

    [Fact]
    public void GetById_IsCaseInsensitive()
    {
        WriteLessonFile("lesson-01.yaml", "lesson-01", order: 1);

        var catalog = CreateCatalog();

        var lesson = catalog.GetById("LESSON-01");
        Assert.NotNull(lesson);
        Assert.Equal("lesson-01", lesson.Id);
    }

    [Fact]
    public void Catalog_UsesYamlId_NotFilename()
    {
        WriteLessonFile("completely-different-name.yaml", "actual-lesson-id", order: 1);

        var catalog = CreateCatalog();

        Assert.NotNull(catalog.GetById("actual-lesson-id"));
        Assert.Null(catalog.GetById("completely-different-name"));
    }

    [Fact]
    public void Catalog_SupportsYamlFiles()
    {
        WriteLessonFile("lesson.yaml", "lesson-01", order: 1);

        var catalog = CreateCatalog();

        Assert.Equal("lesson-01", Assert.Single(catalog.GetAll()).Id);
    }

    [Fact]
    public void Catalog_SupportsYmlFiles()
    {
        WriteLessonFile("lesson.yml", "lesson-01", order: 1);

        var catalog = CreateCatalog();

        Assert.Equal("lesson-01", Assert.Single(catalog.GetAll()).Id);
        Assert.NotNull(catalog.GetById("lesson-01"));
    }

    [Fact]
    public void Catalog_IgnoresUnrelatedFiles()
    {
        WriteLessonFile("lesson.yaml", "lesson-01", order: 1);
        File.WriteAllText(Path.Combine(_contentDirectory, "notes.txt"), "not a lesson");
        File.WriteAllText(Path.Combine(_contentDirectory, "random.json"), "{}");
        File.WriteAllText(Path.Combine(_contentDirectory, "README.md"), "# notes");

        var catalog = CreateCatalog();

        Assert.Equal("lesson-01", Assert.Single(catalog.GetAll()).Id);
        Assert.NotNull(catalog.GetById("lesson-01"));
    }

    [Fact]
    public void Catalog_DetectsDuplicateLessonIds()
    {
        WriteLessonFile("a.yaml", "lesson-01", order: 1);
        WriteLessonFile("b.yaml", "lesson-01", order: 2);

        var exception = Assert.Throws<DuplicateLessonIdException>(CreateCatalog);

        Assert.Equal("lesson-01", exception.LessonId);
        Assert.Contains("lesson-01", exception.Message);
        Assert.Contains("a.yaml", exception.Message);
        Assert.Contains("b.yaml", exception.Message);
    }

    [Fact]
    public void Catalog_DetectsDuplicateLessonIdsAcrossRootAndNestedFiles()
    {
        WriteLessonFile("same.yaml", "same-lesson", order: 1);
        WriteLessonFile(Path.Combine("nested", "same.yaml"), "same-lesson", order: 2);

        var exception = Assert.Throws<DuplicateLessonIdException>(CreateCatalog);

        Assert.Contains("same-lesson", exception.Message);
        Assert.Contains("nested", exception.Message);
    }

    [Fact]
    public void Catalog_AllowsRepeatedExerciseIdsInDifferentLessons()
    {
        WriteLessonWithExercise("one.yaml", "lesson-one", "ex-01");
        WriteLessonWithExercise("two.yaml", "lesson-two", "ex-01");

        var catalog = CreateCatalog();

        Assert.Equal(2, catalog.GetAll().Count);
        Assert.All(catalog.GetAll(), lesson => Assert.Equal("ex-01", Assert.Single(lesson.Exercises).Id));
    }

    [Fact]
    public void Catalog_RejectsConflictingMetadataForOneCourseId()
    {
        WriteLessonWithCourse("a.yaml", "lesson-a", "lento-vivo", "LentoVivo", 20);
        WriteLessonWithCourse("b.yaml", "lesson-b", "lento-vivo", "Another title", 20);

        var exception = Assert.Throws<CourseMetadataConflictException>(CreateCatalog);

        Assert.Equal("lento-vivo", exception.CourseId);
        Assert.Contains("a.yaml", exception.OriginalFilePath);
        Assert.Contains("b.yaml", exception.ConflictingFilePath);
    }

    [Fact]
    public void Catalog_RejectsDuplicateExerciseIdsWithinOneLesson()
    {
        File.WriteAllText(
            Path.Combine(_contentDirectory, "duplicate-exercises.yaml"),
            """
            id: lesson-duplicate-exercises
            order: 1
            exercises:
              - { id: repeated, type: rhythm }
              - { id: REPEATED, type: rhythm }
            """);

        var exception = Assert.Throws<LessonContentLoadException>(CreateCatalog);

        Assert.Contains("duplicate exercise id", exception.InnerException!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Catalog_InvalidYaml_ThrowsMeaningfulException()
    {
        File.WriteAllText(Path.Combine(_contentDirectory, "broken-lesson.yaml"), "id: \"unterminated\n");

        var exception = Assert.Throws<LessonContentLoadException>(CreateCatalog);

        Assert.Contains("broken-lesson.yaml", exception.Message);
        Assert.IsAssignableFrom<YamlLessonParseException>(exception.InnerException);
    }

    [Fact]
    public void Catalog_EmptyDirectory_ReturnsEmptyCollection()
    {
        var catalog = CreateCatalog();

        Assert.Empty(catalog.GetAll());
    }

    [Fact]
    public void Catalog_MissingDirectory_ThrowsExpectedException()
    {
        var missingDirectory = Path.Combine(Path.GetTempPath(), "keyforge-missing-dir", Guid.NewGuid().ToString("N"));
        var options = Options.Create(new LessonCatalogOptions { ContentPath = missingDirectory });

        var exception = Assert.Throws<LessonContentDirectoryNotFoundException>(
            () => new FileSystemLessonCatalog(options, new YamlLessonParser()));

        Assert.Equal(Path.GetFullPath(missingDirectory), exception.DirectoryPath);
    }

    [Fact]
    public void Catalog_DoesNotExposeMutableCollection()
    {
        WriteLessonFile("lesson-01.yaml", "lesson-01", order: 1);
        WriteLessonFile("lesson-02.yaml", "lesson-02", order: 2);

        var catalog = CreateCatalog();
        var originalCount = catalog.GetAll().Count;

        var lessons = catalog.GetAll();
        var mutableView = Assert.IsAssignableFrom<IList<LessonDefinition>>(lessons);

        Assert.Throws<NotSupportedException>(() => mutableView.Add(new LessonDefinition()));

        Assert.Equal(originalCount, catalog.GetAll().Count);
    }

    private FileSystemLessonCatalog CreateCatalog()
    {
        var options = Options.Create(new LessonCatalogOptions { ContentPath = _contentDirectory });
        return new FileSystemLessonCatalog(options, new YamlLessonParser());
    }

    private void WriteLessonFile(string fileName, string id, int order)
    {
        var filePath = Path.Combine(_contentDirectory, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(
            filePath,
            $"id: {id}\norder: {order}\n");
    }

    private void WriteLessonWithExercise(string fileName, string lessonId, string exerciseId)
    {
        var filePath = Path.Combine(_contentDirectory, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, $"""
            id: {lessonId}
            order: 1
            exercises:
              - id: {exerciseId}
                type: rhythm
            """);
    }

    private void WriteLessonWithCourse(string fileName, string lessonId, string courseId, string courseName, int courseOrder)
    {
        var filePath = Path.Combine(_contentDirectory, fileName);
        File.WriteAllText(filePath, $"""
            id: {lessonId}
            order: 1
            course:
              id: {courseId}
              name: "{courseName}"
              order: {courseOrder}
            """);
    }
}
