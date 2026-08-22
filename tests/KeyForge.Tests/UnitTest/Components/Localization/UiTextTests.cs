using System.Globalization;
using KeyForge.Components.Localization;

namespace KeyForge.Tests.UnitTest.Components.Localization;

public sealed class UiTextTests
{
    [Fact]
    public void PersianCulture_UsesPersianLabelsAndRtlDirection()
    {
        using var _ = new CultureScope("fa");

        Assert.Equal("fa", UiText.LanguageCode);
        Assert.Equal("rtl", UiText.Direction);
        Assert.True(UiText.IsPersian);
        Assert.Equal("درس‌ها", UiText.Lessons);
        Assert.Equal("قفل‌شده", UiText.GetLessonStatus(false, false));
        Assert.Equal("در دسترس", UiText.GetLessonStatus(true, false));
        Assert.Equal("تکمیل‌شده", UiText.GetLessonStatus(true, true));
        Assert.Equal("نت‌خوانی", UiText.GetExerciseType(ExerciseType.NoteReading));
        Assert.Equal("جلسه تمرین", UiText.PracticeSession);
        Assert.Equal("تمرین ۲ از ۵", UiText.GetExercisePosition(2, 5));
        Assert.Equal("هر دو دست", UiText.GetPracticeHand(PracticeHand.Both));
    }

    [Fact]
    public void EnglishCulture_UsesEnglishLabelsAndLtrDirection()
    {
        using var _ = new CultureScope("en");

        Assert.Equal("en", UiText.LanguageCode);
        Assert.Equal("ltr", UiText.Direction);
        Assert.False(UiText.IsPersian);
        Assert.Equal("Lessons", UiText.Lessons);
        Assert.Equal("Available", UiText.GetLessonStatus(true, false));
        Assert.Equal("Beginner", UiText.GetLessonLevel(LessonLevel.Beginner));
        Assert.Equal("Medium", UiText.GetDifficulty(Difficulty.Medium));
        Assert.Equal("Practice session", UiText.PracticeSession);
        Assert.Equal("Exercise 2 of 5", UiText.GetExercisePosition(2, 5));
        Assert.Equal("Both hands", UiText.GetPracticeHand(PracticeHand.Both));
    }

    [Theory]
    [InlineData("fa", true)]
    [InlineData("FA", true)]
    [InlineData("en", true)]
    [InlineData("EN", true)]
    [InlineData("de", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsSupportedLanguage_RecognizesOnlyConfiguredLanguages(
        string? language,
        bool expected)
    {
        Assert.Equal(expected, UiText.IsSupportedLanguage(language));
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;

        public CultureScope(string cultureName)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUiCulture;
        }
    }
}
