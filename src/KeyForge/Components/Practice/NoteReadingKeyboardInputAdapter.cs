namespace KeyForge.Components.Practice;

/// <summary>
/// Provides a small browser-keyboard adapter for C4 through C5. It is a UI
/// concern; evaluators receive only normalized note events.
/// </summary>
public static class NoteReadingKeyboardInputAdapter
{
    private static readonly IReadOnlyDictionary<string, string> Mapping =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["KeyA"] = "C4",
            ["KeyW"] = "C#4",
            ["KeyS"] = "D4",
            ["KeyE"] = "D#4",
            ["KeyD"] = "E4",
            ["KeyF"] = "F4",
            ["KeyT"] = "F#4",
            ["KeyG"] = "G4",
            ["KeyY"] = "G#4",
            ["KeyH"] = "A4",
            ["KeyU"] = "A#4",
            ["KeyJ"] = "B4",
            ["KeyK"] = "C5"
        };

    public static ExerciseInputEvent? TryCreate(
        string? code,
        bool repeat,
        bool isActive,
        DateTime exerciseStartedAt,
        DateTime occurredAt,
        DateTime? timelineStartedAt = null)
    {
        if (!isActive || repeat || string.IsNullOrWhiteSpace(code) ||
            !Mapping.TryGetValue(code, out var value))
        {
            return null;
        }

        if (occurredAt < exerciseStartedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(occurredAt));
        }

        var offsetOrigin = timelineStartedAt ?? exerciseStartedAt;
        if (offsetOrigin < exerciseStartedAt || occurredAt < offsetOrigin)
        {
            throw new ArgumentOutOfRangeException(nameof(timelineStartedAt));
        }

        return new ExerciseInputEvent
        {
            Timestamp = occurredAt,
            Offset = occurredAt - offsetOrigin,
            Source = InputSource.ComputerKeyboard,
            Type = InputEventType.Note,
            Value = value
        };
    }
}
