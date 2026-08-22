namespace KeyForge.Components.Localization;

/// <summary>
/// Central bilingual UI copy and presentation-only labels.
/// The active request culture selects Persian or English without leaking
/// localization concerns into domain models.
/// </summary>
public static class UiText
{
    public const string ProductName = "KeyForge";
    public const string PersianLanguage = "فارسی";
    public const string EnglishLanguage = "English";

    public static bool IsPersian => LanguageCode == "fa";
    public static string LanguageCode =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase)
            ? "en"
            : "fa";
    public static string Direction => IsPersian ? "rtl" : "ltr";

    public static string LanguageSelector => Select("انتخاب زبان", "Select language");
    public static string Home => Select("خانه", "Home");
    public static string Lessons => Select("درس‌ها", "Lessons");
    public static string NavigationMenu => Select("منوی اصلی", "Main menu");

    public static string HomeEyebrow => Select("تمرین هدفمند پیانو", "Focused piano practice");
    public static string HomeTitle => Select("مسیر یادگیری موسیقی را منظم پیش ببرید", "Build a consistent path through music");
    public static string HomeDescription => Select(
        "درس‌های ساختاریافته را دنبال کنید، تمرین‌ها را انجام دهید و پیشرفت خود را ببینید.",
        "Follow structured lessons, complete exercises, and see your progress.");
    public static string BrowseLessons => Select("مشاهده درس‌ها", "Browse lessons");

    public static string LearningPath => Select("مسیر یادگیری", "Learning path");
    public static string LessonsDescription => Select(
        "مهارت‌های پیانو را با جلسه‌های کوتاه و هدفمند، قدم‌به‌قدم تقویت کنید.",
        "Develop your piano skills step by step with short, focused sessions.");
    public static string NoLessonsTitle => Select("درسی در دسترس نیست", "No lessons available");
    public static string NoLessonsDescription => Select(
        "درس‌های جدید پس از اضافه‌شدن در این بخش نمایش داده می‌شوند.",
        "New lessons will appear here when they are added.");
    public static string Level => Select("سطح", "Level");
    public static string EstimatedTime => Select("زمان تقریبی", "Estimated time");
    public static string Minute => Select("دقیقه", "min");
    public static string BestScore => Select("بهترین امتیاز", "Best score");
    public static string ViewLesson => Select("مشاهده درس", "View lesson");

    public static string LessonDetails => Select("جزئیات درس", "Lesson details");
    public static string LessonNotFound => Select("درس پیدا نشد", "Lesson not found");
    public static string LessonNotFoundDescription => Select(
        "درس موردنظر وجود ندارد یا دیگر در دسترس نیست.",
        "The requested lesson does not exist or is no longer available.");
    public static string BackToLessons => Select("بازگشت به درس‌ها", "Back to lessons");
    public static string StartPractice => Select("شروع تمرین", "Start practice");
    public static string LockedLessonTitle => Select("این درس قفل است", "This lesson is locked");
    public static string LockedLessonDescription => Select(
        "این درس هنوز برای شروع در دسترس نیست.",
        "This lesson is not available to start yet.");
    public static string LessonOverview => Select("نمای کلی درس", "Lesson overview");
    public static string Exercises => Select("تمرین‌ها", "Exercises");
    public static string ExerciseCount => Select("تعداد تمرین‌ها", "Exercise count");
    public static string NoExercises => Select(
        "هنوز تمرینی برای این درس تعریف نشده است.",
        "No exercises have been defined for this lesson yet.");
    public static string DifficultyLabel => Select("درجه سختی", "Difficulty");
    public static string Duration => Select("مدت", "Duration");
    public static string Tempo => Select("سرعت", "Tempo");
    public static string BeatsPerMinute => Select("ضرب در دقیقه", "BPM");

    public static string PracticeSession => Select("جلسه تمرین", "Practice session");
    public static string PracticeSessionDescription => Select(
        "تمرین را با تمرکز انجام دهید و پس از دیدن نتیجه، خودتان به تمرین بعدی بروید.",
        "Stay focused on the exercise, review your result, then continue when you are ready.");
    public static string SessionProgress => Select("پیشرفت جلسه", "Session progress");
    public static string GetExercisePosition(int current, int total) => Select(
        $"تمرین {FormatNumber(current)} از {FormatNumber(total)}",
        $"Exercise {current} of {total}");
    public static string CurrentExercise => Select("تمرین فعلی", "Current exercise");
    public static string AttemptDetails => Select("ثبت تلاش", "Submit an attempt");
    public static string AttemptInstructions => Select(
        "پس از انجام تمرین، وضعیت تکمیل و امتیاز خود را وارد کنید.",
        "After practising, enter whether you completed the attempt and your score.");
    public static string AttemptCompleted => Select("تلاش را کامل کردم", "I completed the attempt");
    public static string AttemptCompletedHint => Select(
        "اگر تمرین را کامل نکردید، این گزینه را بردارید.",
        "Clear this option if you did not complete the exercise.");
    public static string AttemptScore => Select("امتیاز تلاش", "Attempt score");
    public static string AttemptScoreHint => Select("عددی بین صفر تا ۱۰۰", "A number from 0 to 100");
    public static string SubmitAttempt => Select("ثبت نتیجه", "Submit result");
    public static string AttemptResult => Select("نتیجه تلاش", "Attempt result");
    public static string AttemptSuccessful => Select("تلاش موفق", "Successful attempt");
    public static string AttemptFailed => Select("نیاز به تلاش دوباره", "Try again");
    public static string AttemptSuccessfulDescription => Select(
        "این تلاش با موفقیت ثبت شد. پیش از ادامه می‌توانید نتیجه را مرور کنید.",
        "This attempt was recorded successfully. Review the result before continuing.");
    public static string AttemptFailedDescription => Select(
        "این تلاش کامل محسوب نشد. می‌توانید دوباره تلاش کنید یا به تمرین بعدی بروید.",
        "This attempt was not completed. You can retry or continue to the next exercise.");
    public static string Score => Select("امتیاز", "Score");
    public static string ExerciseCompleted => Select("تمرین تکمیل شده است", "Exercise completed");
    public static string RetryExercise => Select("تلاش دوباره", "Retry exercise");
    public static string NextExercise => Select("تمرین بعدی", "Next exercise");
    public static string FinishSession => Select("پایان جلسه", "Finish session");
    public static string SessionComplete => Select("جلسه تمرین به پایان رسید", "Practice session complete");
    public static string SessionCompleteDescription => Select(
        "همه تمرین‌های این جلسه مرور شدند.",
        "You have worked through every exercise in this session.");
    public static string LessonCompletionStatus => Select("وضعیت تکمیل درس", "Lesson completion status");
    public static string LessonCompleted => Select("درس تکمیل شد", "Lesson completed");
    public static string LessonIncomplete => Select("درس هنوز تکمیل نشده است", "Lesson not completed yet");
    public static string ReturnToLessons => Select("بازگشت به درس‌ها", "Return to lessons");
    public static string PracticeNotFound => Select("جلسه تمرین پیدا نشد", "Practice session not found");
    public static string PracticeNotFoundDescription => Select(
        "برای این شناسه، درس معتبری برای شروع تمرین وجود ندارد.",
        "No valid lesson is available to start a practice session for this ID.");
    public static string PracticeLocked => Select("این درس هنوز قفل است", "This lesson is still locked");
    public static string PracticeLockedDescription => Select(
        "برای شروع این جلسه، ابتدا درس‌های پیش‌نیاز را تکمیل کنید.",
        "Complete the prerequisite lessons before starting this session.");
    public static string PracticeStartError => Select("شروع جلسه ممکن نشد", "Could not start the session");
    public static string PracticeStartErrorDescription => Select(
        "هنگام آماده‌سازی جلسه مشکلی رخ داد. دوباره تلاش کنید.",
        "Something went wrong while preparing the session. Please try again.");
    public static string AttemptSubmissionError => Select(
        "ثبت این تلاش ممکن نشد. دوباره تلاش کنید.",
        "This attempt could not be submitted. Please try again.");
    public static string Hand => Select("دست", "Hand");
    public static string ExerciseConfiguration => Select("تنظیمات تمرین", "Exercise configuration");
    public static string TimeSignature => Select("کسر میزان", "Time signature");
    public static string NoteValues => Select("ارزش زمانی نت‌ها", "Note values");
    public static string PatternCount => Select("تعداد الگوها", "Pattern count");
    public static string Clef => Select("کلید", "Clef");
    public static string NoteRange => Select("گستره نت‌ها", "Note range");
    public static string KeySignature => Select("سرکلید", "Key signature");
    public static string EarTrainingTaskLabel => Select("نوع تمرین شنیداری", "Ear-training task");
    public static string Rounds => Select("تعداد دورها", "Rounds");
    public static string Keys => Select("گام‌ها", "Keys");
    public static string Intervals => Select("فاصله‌ها", "Intervals");
    public static string ExerciseDirectionLabel => Select("جهت", "Direction");
    public static string StartingNote => Select("نت شروع", "Starting note");
    public static string OctaveCount => Select("تعداد اکتاو", "Octave count");
    public static string Fingers => Select("انگشت‌ها", "Fingers");
    public static string AllFingers => Select("همه انگشت‌ها", "All fingers");
    public static string Steps => Select("تعداد گام‌ها", "Steps");
    public static string StartingTempo => Select("سرعت شروع", "Starting tempo");
    public static string TargetTempo => Select("سرعت هدف", "Target tempo");
    public static string SpeedPatternLabel => Select("الگوی سرعت", "Speed pattern");
    public static string Repetitions => Select("تکرارها", "Repetitions");
    public static string ListSeparator => Select("، ", ", ");

    public static string Locked => Select("قفل‌شده", "Locked");
    public static string Available => Select("در دسترس", "Available");
    public static string Completed => Select("تکمیل‌شده", "Completed");
    public static string Unknown => Select("نامشخص", "Unknown");

    public static string UnexpectedError => Select("خطای پیش‌بینی‌نشده‌ای رخ داد.", "An unexpected error occurred.");
    public static string Reload => Select("بارگذاری دوباره", "Reload");
    public static string ErrorTitle => Select("خطا", "Error");
    public static string ErrorDescription => Select(
        "هنگام پردازش درخواست مشکلی رخ داد.",
        "An error occurred while processing your request.");
    public static string RequestId => Select("شناسه درخواست", "Request ID");
    public static string ContentNotFound => Select("صفحه پیدا نشد", "Page not found");
    public static string ContentNotFoundDescription => Select(
        "محتوایی که به‌دنبال آن هستید وجود ندارد.",
        "The content you are looking for does not exist.");

    public static string RejoiningServer => Select("در حال اتصال دوباره به سرور...", "Reconnecting to the server...");
    public static string RejoinRetryPrefix => Select("اتصال برقرار نشد؛ تلاش دوباره تا", "Connection failed; retrying in");
    public static string SecondsLater => Select("ثانیه دیگر.", "seconds.");
    public static string RejoinFailed => Select(
        "ارتباط با سرور برقرار نشد. دوباره تلاش کنید یا صفحه را بارگذاری کنید.",
        "Could not reconnect to the server. Try again or reload the page.");
    public static string Retry => Select("تلاش دوباره", "Retry");
    public static string SessionPaused => Select("نشست توسط سرور متوقف شده است.", "The session was paused by the server.");
    public static string ResumeFailed => Select(
        "ادامه نشست ممکن نشد. دوباره تلاش کنید یا صفحه را بارگذاری کنید.",
        "Could not resume the session. Try again or reload the page.");
    public static string Resume => Select("ادامه", "Resume");

    public static bool IsSupportedLanguage(string? language) =>
        string.Equals(language, "fa", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(language, "en", StringComparison.OrdinalIgnoreCase);

    public static string GetLessonStatus(bool isUnlocked, bool isCompleted) => (isUnlocked, isCompleted) switch
    {
        (_, true) => Completed,
        (true, false) => Available,
        _ => Locked
    };

    public static string GetLessonLevel(LessonLevel level) => level switch
    {
        LessonLevel.Beginner => Select("مبتدی", "Beginner"),
        LessonLevel.Elementary => Select("مقدماتی", "Elementary"),
        LessonLevel.Intermediate => Select("متوسط", "Intermediate"),
        LessonLevel.Advanced => Select("پیشرفته", "Advanced"),
        LessonLevel.Master => Select("حرفه‌ای", "Master"),
        _ => Unknown
    };

    public static string GetExerciseType(ExerciseType type) => type switch
    {
        ExerciseType.Rhythm => Select("ریتم", "Rhythm"),
        ExerciseType.NoteReading => Select("نت‌خوانی", "Note reading"),
        ExerciseType.EarTraining => Select("تربیت شنوایی", "Ear training"),
        ExerciseType.Interval => Select("فاصله‌شناسی", "Interval"),
        ExerciseType.Octave => Select("اکتاو", "Octave"),
        ExerciseType.FingerIndependence => Select("استقلال انگشتان", "Finger independence"),
        ExerciseType.MentalKeyboard => Select("کیبورد ذهنی", "Mental keyboard"),
        ExerciseType.Speed => Select("سرعت", "Speed"),
        _ => Unknown
    };

    public static string GetDifficulty(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => Select("آسان", "Easy"),
        Difficulty.Medium => Select("متوسط", "Medium"),
        Difficulty.Hard => Select("سخت", "Hard"),
        Difficulty.Expert => Select("تخصصی", "Expert"),
        _ => Unknown
    };

    public static string GetPracticeHand(PracticeHand hand) => hand switch
    {
        PracticeHand.Left => Select("چپ", "Left"),
        PracticeHand.Right => Select("راست", "Right"),
        PracticeHand.Both => Select("هر دو دست", "Both hands"),
        _ => Unknown
    };

    public static string GetClef(ClefType clef) => clef switch
    {
        ClefType.Treble => Select("سل", "Treble"),
        ClefType.Bass => Select("فا", "Bass"),
        ClefType.Grand => Select("حامل بزرگ", "Grand staff"),
        _ => Unknown
    };

    public static string GetEarTrainingTask(EarTrainingTask task) => task switch
    {
        EarTrainingTask.PitchMatching => Select("تطبیق زیرایی", "Pitch matching"),
        EarTrainingTask.ChordRecognition => Select("تشخیص آکورد", "Chord recognition"),
        EarTrainingTask.ScaleRecognition => Select("تشخیص گام", "Scale recognition"),
        EarTrainingTask.MelodyDictation => Select("دیکته ملودی", "Melody dictation"),
        _ => Unknown
    };

    public static string GetExerciseDirection(ExerciseDirection direction) => direction switch
    {
        ExerciseDirection.Ascending => Select("صعودی", "Ascending"),
        ExerciseDirection.Descending => Select("نزولی", "Descending"),
        ExerciseDirection.Both => Select("هر دو جهت", "Both directions"),
        _ => Unknown
    };

    public static string GetSpeedPattern(SpeedPattern pattern) => pattern switch
    {
        SpeedPattern.Scale => Select("گام", "Scale"),
        SpeedPattern.Arpeggio => Select("آرپژ", "Arpeggio"),
        SpeedPattern.RepeatedNote => Select("نت تکراری", "Repeated note"),
        SpeedPattern.Chromatic => Select("کروماتیک", "Chromatic"),
        _ => Unknown
    };

    public static string GetNoteValue(string noteValue) => noteValue.ToLowerInvariant() switch
    {
        "whole" => Select("گرد", "Whole"),
        "half" => Select("سفید", "Half"),
        "quarter" => Select("سیاه", "Quarter"),
        "eighth" => Select("چنگ", "Eighth"),
        "sixteenth" => Select("دولاچنگ", "Sixteenth"),
        _ => noteValue
    };

    public static string FormatNumber(int value) => IsPersian
        ? FormatPersianNumber(value)
        : value.ToString(CultureInfo.InvariantCulture);

    private static string FormatPersianNumber(int value) => string.Concat(
        value.ToString(CultureInfo.InvariantCulture).Select(character =>
            character is >= '0' and <= '9'
                ? (char)('۰' + character - '0')
                : character));

    private static string Select(string persian, string english) => IsPersian ? persian : english;
}
