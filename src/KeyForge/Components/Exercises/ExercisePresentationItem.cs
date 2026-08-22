namespace KeyForge.Components.Exercises;

/// <summary>
/// One localized piece of exercise metadata prepared for display.
/// Technical values use an isolated LTR direction inside RTL layouts.
/// </summary>
public sealed record ExercisePresentationItem(
    string Label,
    string Value,
    bool IsTechnical = false);
