namespace KeyForge.Infrastructure.Content.Lessons.Exceptions;

/// <summary>Thrown when lessons register the same course ID with inconsistent display metadata.</summary>
public sealed class CourseMetadataConflictException : Exception
{
    public string CourseId { get; }
    public string OriginalFilePath { get; }
    public string ConflictingFilePath { get; }

    public CourseMetadataConflictException(
        string courseId,
        string originalFilePath,
        string conflictingFilePath)
        : base($"Course '{courseId}' has conflicting metadata in '{originalFilePath}' and '{conflictingFilePath}'.")
    {
        CourseId = courseId;
        OriginalFilePath = originalFilePath;
        ConflictingFilePath = conflictingFilePath;
    }
}
