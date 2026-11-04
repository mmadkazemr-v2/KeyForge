namespace KeyForge.Components.Practice;

/// <summary>Conventional fingering guidance for a one-octave C Major scale.</summary>
public static class ScaleFingeringGuide
{
    public static IReadOnlyList<int> GetFingers(PracticeHand hand, ScaleDirection direction) => (hand, direction) switch
    {
        (PracticeHand.Right, ScaleDirection.Ascending) => [1, 2, 3, 1, 2, 3, 4, 5],
        (PracticeHand.Right, ScaleDirection.Descending) => [5, 4, 3, 2, 1, 3, 2, 1],
        (PracticeHand.Left, ScaleDirection.Ascending) => [5, 4, 3, 2, 1, 3, 2, 1],
        (PracticeHand.Left, ScaleDirection.Descending) => [1, 2, 3, 1, 2, 3, 4, 5],
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Fingering guidance supports ascending or descending one-octave scales with one hand.")
    };
}
