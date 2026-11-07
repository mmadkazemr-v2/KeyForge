namespace KeyForge.Features.Lessons.Models;

/// <summary>Stable display metadata shared by lessons in one practice-library course.</summary>
public sealed class CourseMetadata
{
    /// <summary>Stable logical identity. It is never derived from a content path.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Course name displayed in the practice library.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional course description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Explicit ordering among registered courses.</summary>
    public int Order { get; set; }

    /// <summary>Validates the metadata without mutating it.</summary>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        if (Order < 0)
        {
            throw new InvalidOperationException("Course order must be zero or greater.");
        }
    }

    /// <summary>Creates the stable default group used by legacy lessons without course metadata.</summary>
    public static CourseMetadata CreateKeyForgeFundamentals() => new()
    {
        Id = "keyforge-fundamentals",
        Name = "KeyForge Fundamentals",
        Description = "Core beginner piano practice.",
        Order = 10
    };
}
