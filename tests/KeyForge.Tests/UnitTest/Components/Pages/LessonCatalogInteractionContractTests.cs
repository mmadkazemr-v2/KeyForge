namespace KeyForge.Tests.UnitTest.Components.Pages;

public sealed class LessonCatalogInteractionContractTests
{
    [Fact]
    public void LessonsPage_UsesAccessibleInteractiveCourseHeaderAndPreservesCatalogNavigation()
    {
        var source = File.ReadAllText(FindLessonsPage());

        Assert.Contains("@rendermode InteractiveServer", source, StringComparison.Ordinal);
        Assert.Contains("<button class=\"practice-library__row practice-library__course-row\"", source, StringComparison.Ordinal);
        Assert.Contains("aria-expanded=\"@courseExpanded\" aria-controls=\"@coursePanelId\"", source, StringComparison.Ordinal);
        Assert.Contains("@onclick=\"() => ToggleCourse(course.Metadata.Id)\"", source, StringComparison.Ordinal);
        Assert.Contains("@onclick=\"() => ToggleSession(lesson.Id)\"", source, StringComparison.Ordinal);
        Assert.Contains("hidden=\"@(!courseExpanded)\"", source, StringComparison.Ordinal);
        Assert.Contains("hidden=\"@(!sessionExpanded)\"", source, StringComparison.Ordinal);
        Assert.Contains("GetLessonUrl(lesson.Id)", source, StringComparison.Ordinal);
        Assert.Contains("GetExerciseUrl(lesson.Id, exercise.Id)", source, StringComparison.Ordinal);
    }

    private static string FindLessonsPage()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "src", "KeyForge", "Components", "Pages", "Lessons.razor");
            if (File.Exists(path))
                return path;
        }

        throw new DirectoryNotFoundException("Could not locate Lessons.razor.");
    }
}
