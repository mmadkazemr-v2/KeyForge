using System.Globalization;
using KeyForge.Components.Audio;
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
        Assert.Equal("شروع ریتم", UiText.StartRhythm);
        Assert.Equal("ضرب‌های ازدست‌رفته", UiText.MissedBeats);
        Assert.Equal("زودتر از ضرب", UiText.GetRhythmTimingTendency(RhythmTimingTendency.Early));
        Assert.Equal("تمرین نت‌خوانی", UiText.NoteReadingPractice);
        Assert.Equal("تمرین کیبورد ذهنی", UiText.MentalKeyboardPractice);
        Assert.Equal("فعال‌سازی صدا", UiText.EnableAudio);
        Assert.Equal("تمرین فاصله", UiText.IntervalPractice);
        Assert.Equal("تمرین اکتاو", UiText.OctavePractice);
        Assert.Equal("پرسش ۲ از ۵", UiText.GetQuestionPosition(2, 5));
        Assert.Equal("پیانوی مجازی", UiText.VirtualPiano);
        Assert.Equal("درست", UiText.CorrectAnswer);
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
        Assert.Equal("Start rhythm", UiText.StartRhythm);
        Assert.Equal("Missed beats", UiText.MissedBeats);
        Assert.Equal("Late", UiText.GetRhythmTimingTendency(RhythmTimingTendency.Late));
        Assert.Equal("Note-reading practice", UiText.NoteReadingPractice);
        Assert.Equal("Mental keyboard practice", UiText.MentalKeyboardPractice);
        Assert.Equal("Enable audio", UiText.EnableAudio);
        Assert.Equal("Selected MIDI device disconnected", UiText.MidiDeviceDisconnected);
        Assert.Equal("Could not open the selected MIDI input", UiText.MidiDeviceOpenFailure);
        Assert.Equal("Select a MIDI input", UiText.MidiSelectDevice);
        Assert.Contains("paused", UiText.MidiPerformancePaused, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Interval practice", UiText.IntervalPractice);
        Assert.Equal("Octave practice", UiText.OctavePractice);
        Assert.Equal("Question 2 of 5", UiText.GetQuestionPosition(2, 5));
        Assert.Equal("Virtual piano", UiText.VirtualPiano);
        Assert.Equal("Incorrect", UiText.IncorrectAnswer);
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

    [Fact]
    public void AudioStatusLabels_LocalizePerCulture()
    {
        using var _ = new CultureScope("fa");
        Assert.Equal("در حال فعال‌سازی صدا…", UiText.GetAudioStatusLabel(AudioPlaybackState.Initializing));
        Assert.Equal("صدا آماده است", UiText.GetAudioStatusLabel(AudioPlaybackState.Ready));
        Assert.Equal("صدا در این مرورگر در دسترس نیست", UiText.GetAudioStatusLabel(AudioPlaybackState.Failed));
        Assert.Equal("صدا هنوز فعال نشده است", UiText.GetAudioStatusLabel(AudioPlaybackState.Uninitialized));

        using var __ = new CultureScope("en");
        Assert.Equal("Activating audio…", UiText.GetAudioStatusLabel(AudioPlaybackState.Initializing));
        Assert.Equal("Audio is ready", UiText.GetAudioStatusLabel(AudioPlaybackState.Ready));
        Assert.Equal("Audio is unavailable in this browser", UiText.GetAudioStatusLabel(AudioPlaybackState.Failed));
        Assert.Equal("Audio is not enabled yet", UiText.GetAudioStatusLabel(AudioPlaybackState.Uninitialized));
    }

    [Fact]
    public void AudioFailureMessages_LocalizePerCulture_WithoutMentioningMicrophonePermission()
    {
        using var _ = new CultureScope("fa");
        Assert.Equal(
            "صدا فعال نشد. تنظیمات صدای این سایت را در مرورگر بررسی کنید.",
            UiText.GetAudioFailureMessage(AudioFailureReason.BrowserBlocked));
        Assert.Equal(
            "صدا در این مرورگر در دسترس نیست",
            UiText.GetAudioFailureMessage(AudioFailureReason.NotInitialized));

        using var __ = new CultureScope("en");
        Assert.Equal(
            "Audio could not be activated. Check this site's sound settings in your browser.",
            UiText.GetAudioFailureMessage(AudioFailureReason.BrowserBlocked));
        Assert.DoesNotContain("microphone", UiText.GetAudioFailureMessage(AudioFailureReason.BrowserBlocked),
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("permission", UiText.GetAudioFailureMessage(AudioFailureReason.BrowserBlocked),
            StringComparison.OrdinalIgnoreCase);
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
