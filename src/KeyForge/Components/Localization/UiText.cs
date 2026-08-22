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
    public static string NumeralDigits => IsPersian ? "۰۱۲۳۴۵۶۷۸۹" : "0123456789";

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
    public static string NotationUnavailable => Select(
        "نمایش نت برای این ورودی در دسترس نیست.",
        "Notation is unavailable for this input.");

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
    public static string RuntimeChallenge => Select("چالش تعاملی", "Interactive challenge");
    public static string RuntimeChallengeInstructions => Select(
        "نت خواسته‌شده را با نام علمی آن وارد کنید.",
        "Enter the requested note using scientific pitch notation.");
    public static string RuntimeAnswer => Select("پاسخ شما", "Your answer");
    public static string TargetNote => Select("نت هدف", "Target note");
    public static string RuntimeAnswerHint => Select("برای نمونه: C4 یا F#3", "For example: C4 or F#3");
    public static string EvaluateAnswer => Select("بررسی پاسخ", "Evaluate answer");
    public static string RuntimeUnavailable => Select("حالت تعاملی در راه است", "Interactive mode is coming");
    public static string RuntimeUnavailableDescription => Select(
        "اجرای تعاملی این نوع تمرین هنوز آماده نیست. برای ادامه، به تمرین بعدی بروید.",
        "Interactive runtime for this exercise type is not available yet. Continue to the next exercise.");
    public static string RhythmPractice => Select("تمرین ریتم", "Rhythm practice");
    public static string KeepTheBeat => Select("ضرب را نگه دارید", "Keep the beat");
    public static string RhythmReady => Select("آماده", "Ready");
    public static string StartRhythm => Select("شروع ریتم", "Start rhythm");
    public static string Countdown => Select("شمارش معکوس", "Countdown");
    public static string RhythmGo => Select("شروع", "Start");
    public static string RhythmActive => Select("در حال اجرا", "Active");
    public static string EvaluatingRhythm => Select("در حال ارزیابی", "Evaluating");
    public static string PressKOnEachBeat => Select(
        "روی هر ضرب، کلید K را فشار دهید.",
        "Press K on each beat.");
    public static string CapturedHits => Select("ضرب‌های ثبت‌شده", "Captured hits");
    public static string BeatProgression => Select("روند ضرب‌ها", "Beat progression");
    public static string RhythmProgress => Select("پیشرفت ریتم", "Rhythm progress");
    public static string RhythmStartError => Select(
        "شروع تمرین ریتم ممکن نشد. دوباره تلاش کنید.",
        "The rhythm exercise could not start. Please try again.");
    public static string NoteReadingPractice => Select("تمرین نت‌خوانی", "Note-reading practice");
    public static string ReadTheNote => Select("نت روی حامل را پیدا کنید", "Read the note on the staff");
    public static string NoteReadingInstructions => Select(
        "نت نمایش‌داده‌شده را روی پیانو انتخاب کنید. نام نت پس از پاسخ نمایش داده می‌شود.",
        "Select the displayed note on the piano. The note name is revealed after you answer.");
    public static string QuestionProgress => Select("پیشرفت پرسش‌ها", "Question progress");
    public static string GetQuestionPosition(int current, int total) => Select(
        $"پرسش {FormatNumber(current)} از {FormatNumber(total)}",
        $"Question {current} of {total}");
    public static string VirtualPiano => Select("پیانوی مجازی", "Virtual piano");
    public static string SelectPianoNote => Select("یک کلید پیانو را انتخاب کنید", "Select a piano key");
    public static string ComputerKeyboardHint => Select(
        "ورودی صفحه‌کلید: A تا K نت‌های C4 تا C5 را می‌نوازند؛ کلیدهای W، E، T، Y و U نت‌های دیز هستند.",
        "Keyboard input: A through K play C4 through C5; W, E, T, Y, and U are the sharp notes.");
    public static string CorrectAnswer => Select("درست", "Correct");
    public static string IncorrectAnswer => Select("نادرست", "Incorrect");
    public static string ExpectedNote => Select("نت درست", "Expected note");
    public static string YourNote => Select("نت انتخابی شما", "Your note");
    public static string RetryQuestion => Select("تلاش دوباره برای این پرسش", "Retry this question");
    public static string ContinueQuestions => Select("ادامه", "Continue");
    public static string CorrectAnswers => Select("پاسخ‌های درست", "Correct answers");
    public static string IncorrectAnswers => Select("پاسخ‌های نادرست", "Incorrect answers");
    public static string GetStaffAriaLabel(ClefType clef) => Select(
        $"حامل موسیقی با کلید {GetClef(clef)} و یک نت هدف",
        $"Music staff with {GetClef(clef)} clef and one target note");
    public static string GetPianoKeyAriaLabel(string note) => Select(
        $"کلید پیانو، نت {note}",
        $"Piano key, note {note}");
    public static string ExpectedBeats => Select("ضرب‌های مورد انتظار", "Expected beats");
    public static string SuccessfulHits => Select("ضرب‌های موفق", "Successful hits");
    public static string MissedBeats => Select("ضرب‌های ازدست‌رفته", "Missed beats");
    public static string ExtraHits => Select("ضرب‌های اضافه", "Extra hits");
    public static string AverageTimingError => Select("میانگین خطای زمانی", "Average timing error");
    public static string TimingTendency => Select("گرایش زمانی", "Timing tendency");
    public static string Millisecond => Select("میلی‌ثانیه", "ms");
    public static string NoTimingData => Select("بدون داده", "No data");
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

    public static string GetBpmLabel(int bpm) =>
        $"{FormatNumber(bpm)} {BeatsPerMinute}";

    public static string FormatTimingError(double? milliseconds) => milliseconds.HasValue
        ? $"{FormatNumber((int)Math.Round(milliseconds.Value))} {Millisecond}"
        : NoTimingData;

    public static string GetRhythmTimingTendency(RhythmTimingTendency tendency) => tendency switch
    {
        RhythmTimingTendency.Early => Select("زودتر از ضرب", "Early"),
        RhythmTimingTendency.OnTime => Select("هماهنگ با ضرب", "On time"),
        RhythmTimingTendency.Late => Select("دیرتر از ضرب", "Late"),
        _ => NoTimingData
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
