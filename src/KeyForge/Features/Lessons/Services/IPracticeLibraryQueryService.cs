namespace KeyForge.Features.Lessons.Services;

/// <summary>Provides a read-only course/session/exercise view over existing lesson content.</summary>
public interface IPracticeLibraryQueryService
{
    public IReadOnlyList<PracticeLibraryCourse> GetCourses();
}
