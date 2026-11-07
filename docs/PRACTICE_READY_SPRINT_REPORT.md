> Historical implementation report, preserved during Task 61.1. For current commit and verification status, see [Task 61.1 reconciliation](TASK61_1_RECONCILIATION_REPORT.md).

# Practice ready sprint report — 2026-11-07

## A. Starting repository snapshot

Root AGENTS.md was read; no nested AGENTS.md was found. No Treasury AI Development Workflow V3.1 document was found in the repository; the explicit workflow supplied in the request was followed. No commits were created.

The starting tree already contained modified audio activation/service/state/JavaScript/tests, localization, Lessons and Practice pages, recognition workspaces, rhythm definition/generator/validator, lesson model/catalog/completion interfaces and tests, and Program.cs. Untracked work included the LentoVivo source audit, library styles/models/query service and tests, course metadata exception, LentoVivo YAML, adapted process/content tests, and tmp/lento-s03.png. All was preserved.

Baseline verification: 613 .NET tests passed, zero failures/skips. One existing xUnit2013 warning in LentoVivoAdaptedPracticeProcessTests.cs:68.

## B. Task status

| Task | Status |
|---|---|
| 55.2 | Unmetered runtime, validation and notation implemented; eight source-exact worksheet exercises blocked by source verification. |
| 54 | Browser count-in and rhythm scheduling unified; delayed-interoperability regression tests pass. |
| 53 | Existing uncommitted implementation retained; audio activation regression passes. |
| 52 | Comparative high/low challenge, choice evaluation and workspace routing implemented; first revealed answer retained for retry scoring. |
| 58 | Anonymous browser-profile JSON persistence implemented and wired into scoped application services. |
| 59 | Automated regression completed; interactive browser/hardware acceptance remains pending. |

The entire requested sprint is not source-content complete because the eight worksheet patterns are not verified.

## C. Files changed in this run, per task

Paths below are relative to the repository. Existing overlapping edits were retained.

- 55.2: src/KeyForge/Features/Exercises/Models/RhythmExerciseDefinition.cs; Features/Practice/Services/RhythmChallengeGenerator.cs; Components/Practice/RhythmWorkspace.razor; Components/Music/VexFlowNotationMapper.cs and VexFlowStaff.razor.js; Components/Exercises/ExercisePresentationMapper.cs; tests/KeyForge.Tests/UnitTest/Features/Practice/Services/RhythmChallengeGeneratorTests.cs; ProcessTest/Features/Practice/RhythmAuthoringProcessTests.cs; docs/RHYTHM_AUTHORING.md.
- 54: src/KeyForge/Components/Practice/RhythmWorkspace.razor and .razor.js; tests/KeyForge.Tests/UnitTest/Components/Practice/RhythmWorkspace.runtime.test.mjs.
- 53: No additional audio-engine changes in this run; existing working-tree implementation verified.
- 52: src/KeyForge/Features/Exercises/Models/EarTrainingTask.cs; Features/Practice/Services/EarTrainingChallengeGenerator.cs and EarTrainingExerciseEvaluator.cs; Components/Practice/EarTrainingWorkspace.razor and IntervalRecognitionWorkspace.razor; Components/Localization/UiText.cs; Content/Lessons/Lesson01.yaml; tests/KeyForge.Tests/UnitTest/Features/Practice/Services/PitchComparisonTests.cs.
- 58: src/KeyForge/Infrastructure/Persistence/Progress/JsonPracticeStore.cs; src/KeyForge/Program.cs; .gitignore; tests/KeyForge.Tests/UnitTest/Infrastructure/Persistence/Progress/JsonPracticeStoreTests.cs; ProcessTest/Features/Practice/DurablePracticeProcessTests.cs.
- 59: This report and automated verification; no commits.

## D. Implemented behavior

Unmetered phrases require a single authored nonempty pattern. Measures and legacy repetition cannot be mixed into that contract. Durations compile cumulatively with quarter-note tempo pulses. There is no padding or artificial displayed time signature. Existing metered generation remains covered by regression tests.

Rhythm count-in now uses the rhythm browser scheduler rather than the server metronome loop. Click scheduling starts without awaiting the .NET origin notification. Completion waits for pending input/notification work. Run versions reject stale .NET start/input/completion callbacks; stopping cancels pending browser timers. MIDI disconnect can pause during count-in as well as performance.

Lesson01 retains ex-03 and now authors pitchComparison. The generator emits two-note questions and higher/lower expected choices. It reuses melodic interval playback and exact choice evaluation. Notes and correct answers appear only in feedback. Retrying after feedback retains the first scored answer. Existing pitch matching, interval and chord generation remain available.

The existing audio activation implementation retains a native user gesture, shared Tone context/synth initialization, suspension/recovery reporting, independent educational stimuli and metronome, and suppression of live monitoring in External DAW mode. Its existing automated tests pass.

## E. Source-verified versus adapted LentoVivo content

No existing lesson/exercise ID was changed. Existing Session03–05 adapted registrations and content were preserved.

The only local worksheet image found was tmp/lento-s03.png (1600×2200 RGB). It displayed as a blank dark image and could not verify any notation. The eight patterns supplied in the request remain reported transcriptions, not independently confirmed source facts. No eight source-exact exercises were added and no synchronization between RH/LH was inferred. The earlier source audit remains available in docs/LENTOVIVO_SESSIONS_03_05_SOURCE_AUDIT.md.

## F. Timing and scoring guarantees and limitations

Count-in and exercise deadlines use performance.now() and absolute elapsed deadlines on the same browser-relative time axis. Interop latency no longer delays the pulse loop. MIDIMessageEvent timestamps remain the input timing authority through the existing MIDI adapter. Neither Blazor render times nor wall-clock dates determine onset offsets.

JavaScript timers remain subject to browser scheduling jitter and tab throttling. Audio clicks are requested at deadlines; no claim is made about physical speaker output alignment or output latency. Rhythm scoring checks attack timing, not sustained note durations, fingering, or actual hand attribution. Metered and unmetered phrases use the same evaluator.

## G. Persistence design and restart behavior

Interactive Server hosting has no authenticated user identity. An HttpOnly SameSite=Lax persistent anonymous profile cookie selects a separate JSON file per browser profile. Application consumers of progress/attempt services now have scoped lifetimes. Catalog/parser and stateless generation/evaluation remain singleton. No global shared progress file or database was introduced.

Storage defaults to src/KeyForge/App_Data/Practice/<profile>.json; override PracticeStorage:Directory for a durable deployment location. Each snapshot contains attempts and progress. Reads return deserialized snapshots rather than mutable stored references. Attempt GUIDs deduplicate repeated writes; successful and failed attempts retain their semantics. Attempt writes also update count/best-score projections. Lesson completion continues through existing services and successful-attempt history.

Each file operation reloads under a shared in-process path lock, avoiding stale per-circuit caches. Writes serialize to a temporary file, flush, then replace the destination. Missing/empty files represent no history. Malformed JSON or invalid snapshot structure raises an explicit InvalidDataException rather than silently showing empty progress. Reading/navigation does not create a file. Preserve profile files and cookie across restarts; clearing cookies creates a new anonymous profile. Different people sharing one browser profile share that profile's history. This implementation supports one server process; cross-process concurrent writers and cloud synchronization are not supported. Back up the profile directory.

## H. Exact automated verification

Final commands and observed results:

- dotnet build -c Release --nologo: succeeded, zero errors, one pre-existing xUnit2013 warning.
- dotnet test -c Release --no-build --nologo: 620 passed, zero failed, zero skipped.
- node --test with every rg-discovered *.runtime.test.mjs under tests: 25 passed, zero failed/cancelled/skipped.
- Focused .NET filter covering Yaml, LessonCatalog, LentoVivo, AudioPlayback, JsonPractice, DurablePractice, PitchComparison and RhythmAuthoring: 141 passed (before the final snapshot projection hardening; the final full suite covers that change).
- git -c core.safecrlf=false diff --check: passed.
- HTTP /lessons smoke: 200, 45,444 response characters, profile cookie issued.

YAML validation, catalog, adapted Session03–05, MIDI timing/scoring, audio and persistence tests run within the .NET suite; no separate standalone YAML validator command was found. New coverage includes 40/55/70 BPM unmetered offsets, metered/unmetered YAML-to-MIDI process flow, delayed interop/count-in timing, pitch comparison, durable successful/failed histories and duplicate prevention, malformed/missing/empty storage, navigation without writes, and restart completion evaluation.

A build attempted while the temporary smoke-test server was running hit an executable file lock. The temporary server was stopped and the final build/test pass above succeeded. An initial PowerShell web request needed -UseBasicParsing; its corrected HTTP check succeeded.

## I. Remaining browser/hardware validation

Chrome debugging endpoint 127.0.0.1:9223 refused connection. No interactive browser verification was completed. The temporary HTTP server was stopped. Physical MIDI and audible output were not verified in this run.

## J. Final student acceptance checklist

1. Open the library, expand Fundamentals and LentoVivo Sessions03–05, and select exercises without increasing attempt counts.
2. Activate internal audio with the button; hear the prompt and verify retry/recovery after audio suspension.
3. In Lesson01 high/low, hear two notes, answer higher/lower, and confirm the answer is hidden until submission. Retry an incorrect question and confirm the revealed retry does not erase the initial error.
4. Test a metered rhythm and an authored unmetered phrase at 40, 55 and 70 BPM; verify count-in, first click, onset feedback, missed and extra attacks.
5. Disconnect/reconnect MIDI during count-in and practice; restart and check for orphaned clicks or duplicate attempts.
6. Practice a note sequence, interval question and chord question. Confirm correct/incorrect results and completion indicators.
7. Switch to External DAW: confirm MIDI scoring continues, live browser piano is suppressed, educational prompts still play, and metronome mode remains independent.
8. Complete an exercise, reload, restart the application, and verify history/counts/best score/completion remain. Open a separate browser profile and verify it has separate history.
9. Inspect unmetered notation on desktop/mobile for overflow, absence of invented meter/barlines, and readable counts.

## K. Unresolved blockers

Source-exact Session03 content remains blocked by lack of a readable worksheet. Supply a readable source before labeling the eight reported patterns confirmed. Browser rendering/audio latency and physical MIDI acceptance remain deferred to the final manual session. The product is not labeled browser-verified or hardware-verified.
