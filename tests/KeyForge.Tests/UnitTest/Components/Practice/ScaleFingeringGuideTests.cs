using KeyForge.Components.Practice;

namespace KeyForge.Tests.UnitTest.Components.Practice;

public sealed class ScaleFingeringGuideTests
{
    [Theory]
    [InlineData(PracticeHand.Right, ScaleDirection.Ascending, new[] { 1, 2, 3, 1, 2, 3, 4, 5 })]
    [InlineData(PracticeHand.Right, ScaleDirection.Descending, new[] { 5, 4, 3, 2, 1, 3, 2, 1 })]
    [InlineData(PracticeHand.Left, ScaleDirection.Ascending, new[] { 5, 4, 3, 2, 1, 3, 2, 1 })]
    [InlineData(PracticeHand.Left, ScaleDirection.Descending, new[] { 1, 2, 3, 1, 2, 3, 4, 5 })]
    public void GetFingers_ReturnsConventionalCmajorOneOctaveFingering(
        PracticeHand hand,
        ScaleDirection direction,
        int[] expected)
    {
        Assert.Equal(expected, ScaleFingeringGuide.GetFingers(hand, direction));
    }

    [Theory]
    [InlineData(PracticeHand.Both)]
    [InlineData(PracticeHand.Either)]
    public void GetFingers_RejectsNonSingleHandSelection(PracticeHand hand)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ScaleFingeringGuide.GetFingers(hand, ScaleDirection.Ascending));
    }
}
