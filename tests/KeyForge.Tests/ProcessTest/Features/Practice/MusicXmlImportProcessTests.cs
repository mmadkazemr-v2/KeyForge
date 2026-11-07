namespace KeyForge.Tests.ProcessTest.Features.Practice;

using KeyForge.Components.Practice;
using KeyForge.Infrastructure.Content.Import;
using KeyForge.Tests.UnitTest.Infrastructure.Content;

public sealed class MusicXmlImportProcessTests
{
    [Fact]
    public void ImportedYamlLoadsThroughCatalogAndMidiEvaluationToProgress()
    {
        var directory = Path.Combine(Path.GetTempPath(), "keyforge-import-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, "original.yaml");
        try
        {
            var yaml = new MusicXmlPerformanceImporter().Import(MusicXmlPerformanceImporterTests.OriginalXml, "P1", "import-test", "student", 60);
            File.WriteAllText(file, yaml);
            var catalog = new FileSystemLessonCatalog(Options.Create(new LessonCatalogOptions { ContentPath = directory }), new YamlLessonParser());
            var store = new InMemoryProgressStore();
            var recorder = new InMemoryExerciseAttemptRecorder();
            var progression = new LessonProgressionService(catalog, store, new ExerciseCompletionEvaluator(recorder));
            var service = new PracticeSessionService(catalog, progression, new ExerciseChallengeGenerator(), new ExerciseEvaluator(), new ExerciseScorer(), recorder);
            var session = service.StartSession("import-test")!;
            var runtime = service.StartExercise(session);
            long sequence = 0;
            foreach (var note in runtime.Challenge.Performance!.Events.Where(e => e.Pitch is not null))
            {
                var input = MidiInputAdapter.TryCreate(new MidiInputMessage { DeviceId = "keys", DeviceName = "test", Status = 0x90, Data1 = MusicalNote.Parse(note.Pitch!).MidiNumber, Data2 = 90, TimestampMilliseconds = 12000 + note.Onset.TotalMilliseconds, Sequence = ++sequence }, runtime.StartedAt.AddSeconds(10), 12000)!;
                runtime.CaptureInput(input);
            }
            var result = service.SubmitAttempt(session, runtime, runtime.StartedAt.AddSeconds(10));
            new ProgressUpdateService(store, catalog, progression, recorder).UpdateProgress("import-test", result);
            Assert.Equal(100, result.Score);
            Assert.True(store.GetProgress("import-test")!.IsCompleted);
            Assert.Single(recorder.GetAttemptsByLesson("import-test"));
        }
        finally { File.Delete(file); Directory.Delete(directory); }
    }
}
