namespace KeyForge.Components.Practice;

/// <summary>Maps browser keyboard data to the runtime's generic input event.</summary>
public static class RhythmKeyboardInputAdapter
{
    public const string PracticeKeyCode = "KeyK";

    public static ExerciseInputEvent? Create(
        string code,
        bool isRepeat,
        bool isActive,
        double elapsedMilliseconds,
        DateTime timelineStartedAt)
    {
        if (!isActive ||
            isRepeat ||
            !string.Equals(code, PracticeKeyCode, StringComparison.Ordinal) ||
            !double.IsFinite(elapsedMilliseconds) ||
            elapsedMilliseconds < 0)
        {
            return null;
        }

        var offset = TimeSpan.FromMilliseconds(elapsedMilliseconds);
        return new ExerciseInputEvent
        {
            Timestamp = timelineStartedAt + offset,
            Offset = offset,
            Source = InputSource.ComputerKeyboard,
            Type = InputEventType.Beat,
            Value = "beat"
        };
    }
}
