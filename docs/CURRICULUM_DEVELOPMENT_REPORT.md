# Curriculum-ready development report

Date: 2026-11-08 (work began 2026-11-07).

## A. Starting repository state

Branch main; initial HEAD a6883b3b74193ee54f69776640a0fbf85af526cb. Read root AGENTS.md; no nested AGENTS.md or Treasury workflow file was found. Followed the direct-first workflow supplied in the request. Baseline raw status categories: {" M":166,"AD":1,"??":21}. The index contained only a staged empty src/.editorconfig, which was already deleted in the working tree; a complete user-authored root .editorconfig was untracked. The solution still referenced the old location.

Existing work included Tasks51?60 audio changes, library hierarchy, adapted LentoVivo Sessions03?05, unmetered rhythm, high/low comparison and durable progress. It remains preserved and outside the new feature commits. No reset, stash or destructive cleanup was used. No push was made.

Relevant architecture: a single .NET10 Blazor Interactive Server app; polymorphic YAML exercises; challenge generation; normalized browser-relative MIDI input; evaluator/scorer; attempt recorder and progress services; static startup-loaded filesystem catalog. Existing domain models remain framework-independent. No database or new NuGet/npm dependency was introduced.

Audit directory outside the repository: C:\Users\MMAD.KR5\AppData\Local\Temp\keyforge-curriculum-a32c27dde12849fda7541e23c10e38e7. It contains initial byte snapshots, staged/unstaged patches, raw status/EOL inventory, isolated staged-tree patches, per-slice archives and formatter reports.

## B. EditorConfig result

The root configuration was already moved by the user. Preserved its rules byte-for-byte and removed the obsolete staged empty src entry while committing the root file. Corrected the solution reference. root=true now applies the file-type rules to src, tests and applicable root files without duplicate nested configurations. No .gitattributes or nested configuration was found. core.autocrlf=true agrees with the C#/VB CRLF rule; no Git renormalization was run.

Rules: generic spaces/trailing-whitespace trimming and 140-column guideline; C# indentation4/final newline/CRLF; project indentation4; listed XML/config and JSON indentation2; file-type shell/batch EOL rules. Charset remains commented out. Configured C# using ordering, accessibility, expression-body and var fixes were applied through the bundled formatter; structural rewrites were deliberately excluded.

SDK10.0.400 and bundled dotnet format were used. Existing package scripts only vendor Tone/VexFlow; no frontend formatter is configured. Generated/vendored assets were excluded. Generic scans found no authored Razor/JS/CSS/JSON/YAML/Markdown trailing-whitespace or tab-indentation violations. No arbitrary frontend indentation or line wrapping was imposed. The 140-column limit and ReSharper-only preferences are not fully enforceable by the available CLI.

Working-tree phase-zero normalization changed68 test C# files and the test project; application files had already been normalized in Task60. The clean formatting commit includes66 C# files,2 project files, the root configuration and solution reference (70 files total); Git normalizes many EOL-only changes, so byte-change counts differ from committed diff counts. Commit type summary: {"src/":1,"src/.slnx":1,"src/.cs":35,"src/.csproj":1,"tests/.cs":31,"tests/.csproj":1}.

Verification: solution whitespace and selected safe-style checks exit0. Unrestricted info-level style verification exits2 with73 structural suggestions; analyzer verification exits2 with the pre-existing xUnit2013 warning. No severity was suppressed or rewritten. Full severity/rule/file/reason inventory appears below.

## C. Curriculum capability matrix

These are engine capabilities, not proof of exact coverage of all75 sessions. ?Verified? means automated verification; browser/hardware verification is separate.

| Capability | Status and code evidence |
|---|---|
| Legacy metered/unmetered attack rhythm; rests/dotted/sixteenth values; supported simple/compound meters | Implemented with onset-only limitations; RhythmExerciseDefinition, RhythmChallengeGenerator, RhythmExerciseEvaluator and rhythm process/runtime tests. Unmetered support is preserved prior work. |
| Ordered note practice, major/natural-minor scales and multi-octave direction | Implemented; SequenceExerciseDefinition, PitchSequenceBuilder.BuildScale, SequenceWorkspace and regression tests. |
| Pitched timelines with rests, same-onset events and overlap | Implemented; PerformanceDefinition, PerformanceChallengeDetails, SequenceChallengeGenerator, PerformanceWorkspace. |
| Physical key-hold durations | Implemented opt-in; PerformanceMidiPairing and PerformanceDurationTests. Early/late/missing release feedback. |
| Simultaneous notes and independent instructional parts | Implemented with limitations; deterministic combined-note matching and complete-chord success rule. No physical hand/finger identification. PolyphonicPracticeProcessTests cover eight flows. |
| Treble/bass, sharp accidentals and grand-staff single-note reading | Implemented with rendering limitations; VexFlowNotationMapper, VexFlowStaff and .NET/JS contract tests. Visual layout still needs browser acceptance. |
| Full simultaneous multi-voice score engraving, ties, key-signature spelling and flats | Missing/partial. Performance currently displays an accurate event table; it does not pretend to engrave the timeline as a complete score. Existing key-signature metadata does not establish full key-signature-aware notation. |
| Major/minor arpeggios, inversions, multi-octave directions | Implemented; ArpeggioConfiguration and PitchSequenceBuilder.BuildArpeggio; original content and process tests. |
| Chord recognition and authored performed chords | Recognition preserved; performed chord groups now use explicit timeline events. Automatic chord-progression/voicing generators remain missing. |
| Content transposition | Implemented as explicit semitone transposition for sequences/scales/arpeggios/performance pitches; original IDs and event times remain unchanged. Enharmonic theoretical spelling is limited to the existing sharp-normalized pitch model. |
| Ear training | Existing pitch, interval, chord and prior uncommitted comparative high/low paths preserved and regression-tested. |
| MusicXML import | Limited usable workflow implemented: explicit learner-part preview, validated YAML download, save in configured content folder, restart, practice. Uncompressed score-partwise/fixed-tempo subset only. |
| SMF MIDI/MXL import, repeat navigation and tied import | Missing; unsupported MusicXML structures fail explicitly. |
| Relative dynamics | Implemented with authored, measured keyboard calibration and per-event targets. No automatic calibration wizard or universal velocity cutoff. |
| Sustain | Implemented as optional CC64-aware release endpoints. This permits valid pedal sustain; it does not yet require a particular authored pedal gesture pattern or measure audio decay. |
| Legato/staccato/non-legato assessment | Missing as a dedicated relationship-based scorer. Explicit hold durations can be practiced, but this is not claimed as technique assessment. |
| Sight-reading | Basic note-reading is available; unseen full-score presentation/assessment remains limited. |
| Attempts/progress/restart persistence | Existing contracts reused. Durable anonymous-profile JSON implementation is preserved prior uncommitted work; regression tests pass. No new global shared file. |
| LentoVivo exact source coverage | Unverified beyond the existing audit. Sessions03?05 remain adapted and unchanged; no claim of75-session content completion. |

## D. Completed vertical slices and acceptance evidence

- A: backward-compatible optional Sequence performance contract; parsing, validation, generated events, Practice MIDI workspace, result feedback, attempts and progress. Added original pitched phrase with quarter/half/dotted-half/whole durations.
- B: opt-in keyHold mode, per-device/channel/pitch FIFO pairing, duration tolerance, early/late/missing/orphan release feedback. Legacy onset-only mode preserved.
- C: independent part/hand metadata, combined polyphonic matching, sustained bass with moving melody, complete simultaneous groups required for success. Missing RH/LH, wrong pitch/timing, early release, extra notes, reversed order and restart are covered.
- D1/D2: sharp accidentals and paired grand-staff single-note reading using existing VexFlow. Mapping/render-contract tests and YAML-to-MIDI-to-progress cases cover both. Multi-voice engraving remains a separate gap.
- E1/E2: major/minor arpeggio inversions and explicit transposition through existing Sequence workspace/evaluation/progress; authoring/range/formula/process tests pass.
- F1: selected-part MusicXML to validated YAML with import/preview/download UI, filesystem-catalog reload and successful MIDI-to-progress process coverage. Automatic hot reload/upload into a shared catalog was intentionally not introduced.
- G1/G2: pedal-aware release and calibrated relative dynamics; UI feedback, validation, mode regression and MIDI-to-progress tests pass. No audio engine changes.

Every slice was compiled/tested both in the full working tree and as its exact isolated staged tree before committing. This verifies that commits do not depend on earlier uncommitted feature work.

## E. Commits and verification

| Commit | Capability | Full working-tree .NET tests | Isolated committed-tree .NET tests |
|---|---|---:|---:|
| 9f56308 | Repository formatting/scope | 620 | 596 |
| dda4a68 | Performance events/workspace | 629 | 605 |
| 98de9fe | Key-hold scoring | 637 | 613 |
| 2595aae | Polyphonic/two-part success | 645 | 621 |
| d048956 | Sharp-note reading | 646 | 622 |
| b973fd5 | Arpeggio inversions | 652 | 628 |
| 40bd04a | Transposition | 657 | 633 |
| 108e916 | MusicXML subset import | 664 | 640 |
| a1aec05 | Pedal-aware release | 671 | 647 |
| b29f345 | Calibrated dynamics | 678 | 654 |
| d18e806 | Grand-staff reading | 682 | 658 |

All counts passed with zero failures/skips. JavaScript rendering-contract tests were run for notation slices. Each isolated index was seeded from HEAD and received only that slice's baseline-to-working delta; staged diffs were reviewed and checked. Normal-index entries were synchronized only for committed slice paths. The superseded staged empty EditorConfig was deliberately replaced by the root configuration. No unrelated feature changes were staged.

## F. Architecture and timing contracts

Sequence remains the authoring/runtime type; optional performance data adds pitched timelines without a new engine or DI service layer. Domain authoring models remain POCOs. Runtime records copy event/calibration values. The existing ExerciseEvaluator dispatches to a focused PerformanceExerciseEvaluator. MIDI capture and existing submission/attempt/progress services remain the integration points.

Performance events use quarter-note beats and compile to TimeSpan offsets at selected BPM. The default firstAttack reference aligns to the learner's first captured attack; it assesses relative rhythm and cannot assess leading-rest duration or REAPER beat phase. workspaceStart is explicit and uses the browser-origin capture; it is not REAPER synchronization. MIDIMessageEvent.timeStamp remains authoritative through MidiInputAdapter. Wall-clock timestamps are record metadata, not musical timing.

Matching is deterministic: expected events by onset/pitch; nearest unclaimed same-pitch attack inside configured tolerance, with stable input ordering. This is not a global optimal alignment algorithm for arbitrarily ambiguous dense performances. NoteOff pairing is FIFO per device/channel/pitch. Default onset and release tolerances are150ms and can be authored. Simultaneous groups must be complete to succeed; numeric scores retain partial credit and extra-note penalties.

keyHold assesses mechanical key duration. pedalSustain uses CC64-latched endpoints and requires an endpoint to score a release; it does not measure REAPER's acoustic envelope. Neither mode identifies actual fingers or hands. Relative velocity targets use explicit soft/loud calibration; values must be measured on the student's setup.

REAPER remains responsible for normal-practice piano sound and audible metronome. The new workspace/scorers/importer generate no audio. Existing audio components and routing were preserved. Matching BPM alone does not phase-synchronize REAPER with KeyForge.

## G. Compatibility and content integrity

All pre-existing YAML files are byte-identical to the starting snapshot, preserving IDs, course metadata, values, comments and ordering. All new default examples are original and explicitly labeled, under lesson ID performance-foundations in Content/Lessons/Performance01.yaml. No copyrighted course arrangement or guessed Session03 transcription was added.

Existing Rhythm, Sequence, scale, ear-training, audio, attempt and progress suites pass. Navigation/restart resets do not record a new attempt; submission remains the recording boundary. Earlier Task58 persistence registration/implementation was left intact in the working tree and was not mixed into these commits.

## H. Final verification

- dotnet build -c Release --nologo: succeeded,0 errors,1 pre-existing xUnit2013 warning in LentoVivoAdaptedPracticeProcessTests.cs:68.
- dotnet test -c Release --no-build --nologo:682 passed,0 failed,0 skipped.
- Focused YAML/catalog/library/progress/MIDI/rhythm/performance/polyphony/arpeggio/transposition/MusicXML/dynamics filter:363 passed,0 failed,0 skipped.
- node --test over every discovered *.runtime.test.mjs:26 passed,0 failed/cancelled/skipped.
- dotnet format whitespace KeyForge.slnx --no-restore --verify-no-changes:exit0.
- dotnet format style KeyForge.slnx --severity info with safe diagnostic list --verify-no-changes:exit0. Safe list: IDE0007 IDE0009 IDE0036 IDE0040 IDE0065 IDE0161 IDE0022 IDE0025 IDE0026 IDE0027 IDE0011 IDE0048.
- Unrestricted info-level style verification:exit2,73 classified structural suggestions below.
- Analyzer verification:exit2,one pre-existing warning below; not suppressed.
- git -c core.safecrlf=false diff --check and staged diff check:passed.
- HTTP smoke: /lessons200; /practice/performance-foundations?exercise=original-duration-phrase200; original-two-part-phrase route200; /import200. Temporary server stopped.
- Chrome automation:connection refused at127.0.0.1:9223. No interactive browser or physical MIDI/REAPER verification is claimed.

## I. Remaining gaps

Engine: full timeline notation with voices/ties/key-signature spelling/flats; dedicated articulation relationships; explicit required pedal patterns; SMF MIDI and compressed-MXL import; more advanced importer structures and tempo maps; automatic calibration UI; generalized chord progression/voicing authoring. This is substantial curriculum-engine progress, not completion of every requested general-purpose capability.

Content: source-exact Session03 worksheet remains unverified; existing audit records a blank/unusable local rendering. Exact Session04/05 pages, song arrangements, learner duet parts and remaining75-session material are still required. Adapted exercises remain labeled as adapted. A complete engine and complete course content are separate deliverables.

Acceptance: browser layout/rendering, mobile/RTL usability, real Alesis note/release/pedal events, measured calibration and REAPER audio/metronome workflow require final manual acceptance. Synthetic events do not verify physical timing, audio latency or phase alignment.

## J. Recommended next slice

Add a narrow timed multi-voice notation contract and renderer for the existing performance events, beginning with the original bass-sustain/melody example. Validate that notation and scoring share the exact same event timeline, then add tie representation without changing existing IDs. Treat explicit required pedal intervals and articulation assessment as separate slices. Obtain readable source material before claiming LentoVivo source-exact content.

## K. Final Git state

Current product HEAD:d18e806. All new feature slices are independently committed. The documentation accompanying this report is committed separately. Earlier unrelated/uncommitted work remains intentionally present; the repository is not clean. No push was made. The full final raw status is preserved in the audit directory's final-status.txt; initial status and binary patches are alongside it. There are no unrelated staged changes.

Windows EOL/stat-cache differences can make raw status list more entries than git diff --name-only; no blanket renormalization or broad staging was used to hide them. Review actual diffs alongside the captured baseline.

## Remaining diagnostic inventory

| Severity | Rule | File:line | Reason not automatically changed |
|---|---|---|---|
| info | IDE0270 | src/KeyForge/Infrastructure/Yaml/Validation/YamlLessonValidator.cs:193 | Null-handling rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Infrastructure/Yaml/Validation/YamlLessonValidator.cs:100 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0270 | src/KeyForge/Features/Practice/Services/PracticeSessionService.cs:78 | Null-handling rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/PracticeSessionService.cs:57 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Infrastructure/Content/Import/MusicXmlPerformanceImporter.cs:120 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Infrastructure/Persistence/Progress/JsonPracticeStore.cs:30 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0330 | src/KeyForge/Features/Progress/Services/ProgressUpdateService.cs:17 | Lock type change; synchronization review required |
| info | IDE0305 | src/KeyForge/Components/Music/VexFlowNotationMapper.cs:16 | Collection-expression conversion; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/EarTrainingChallengeGenerator.cs:17 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/EarTrainingChallengeGenerator.cs:147 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/EarTrainingChallengeGenerator.cs:201 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/RhythmExerciseEvaluator.cs:157 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/RhythmExerciseEvaluator.cs:190 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/RhythmChallengeGenerator.cs:43 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0042 | src/KeyForge/Features/Practice/Services/PerformanceExerciseEvaluator.cs:15 | Tuple/deconstruction structural style; separate review |
| info | IDE0300 | src/KeyForge/Features/Practice/Services/FingerIndependenceChallengeGenerator.cs:12 | Collection-expression conversion; separate semantic review |
| info | IDE0305 | src/KeyForge/Features/Practice/Services/FingerIndependenceChallengeGenerator.cs:12 | Collection-expression conversion; separate semantic review |
| info | IDE0305 | src/KeyForge/Features/Practice/Services/FingerIndependenceChallengeGenerator.cs:19 | Collection-expression conversion; separate semantic review |
| info | IDE0305 | src/KeyForge/Features/Practice/Services/FingerIndependenceChallengeGenerator.cs:20 | Collection-expression conversion; separate semantic review |
| info | IDE0300 | src/KeyForge/Features/Practice/Services/PitchSequenceBuilder.cs:66 | Collection-expression conversion; separate semantic review |
| info | IDE0300 | src/KeyForge/Features/Practice/Services/PitchSequenceBuilder.cs:67 | Collection-expression conversion; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/PitchSequenceBuilder.cs:11 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/PitchSequenceBuilder.cs:23 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/ExerciseChallengeGenerator.cs:52 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Components/Audio/AudioSequenceNote.cs:12 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Lessons/Services/LessonProgressionService.cs:23 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Lessons/Services/LessonProgressionService.cs:65 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/MentalKeyboardChallengeGenerator.cs:56 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/IntervalChallengeGenerator.cs:71 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Components/Practice/MetronomeTiming.cs:8 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Components/Practice/MetronomeTiming.cs:18 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Components/Practice/MetronomeTiming.cs:29 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/NoteReadingChallengeGenerator.cs:44 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/NoteReadingChallengeGenerator.cs:55 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/NoteReadingChallengeGenerator.cs:67 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/NoteReadingChallengeGenerator.cs:88 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0290 | src/KeyForge/Features/Practice/Services/NoteReadingChallengeGenerator.cs:13 | Primary constructor conversion; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Models/MusicalNote.cs:29 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Models/MusicalNote.cs:67 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Models/MusicalNote.cs:79 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/OctaveChallengeGenerator.cs:45 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Features/Practice/Services/OctaveChallengeGenerator.cs:66 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Components/Practice/VirtualPianoInputAdapter.cs:20 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Components/Practice/NoteReadingKeyboardInputAdapter.cs:47 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0046 | src/KeyForge/Components/Practice/MidiInputAdapter.cs:115 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0290 | src/KeyForge/Infrastructure/Content/Lessons/Exceptions/LessonContentDirectoryNotFoundException.cs:11 | Primary constructor conversion; separate semantic review |
| info | IDE0290 | src/KeyForge/Infrastructure/Content/Lessons/Exceptions/CourseMetadataConflictException.cs:10 | Primary constructor conversion; separate semantic review |
| info | IDE0290 | src/KeyForge/Infrastructure/Content/Lessons/Exceptions/LessonContentLoadException.cs:13 | Primary constructor conversion; separate semantic review |
| info | IDE0290 | src/KeyForge/Infrastructure/Content/Lessons/Exceptions/DuplicateLessonIdException.cs:21 | Primary constructor conversion; separate semantic review |
| info | IDE0290 | src/KeyForge/Infrastructure/Yaml/Exceptions/UnknownExerciseTypeException.cs:12 | Primary constructor conversion; separate semantic review |
| info | IDE0330 | src/KeyForge/Infrastructure/Practice/InMemory/InMemoryExerciseAttemptRecorder.cs:15 | Lock type change; synchronization review required |
| info | IDE0330 | src/KeyForge/Components/Practice/MidiInputRouter.cs:6 | Lock type change; synchronization review required |
| info | IDE0290 | tests/KeyForge.Tests/ProcessTest/Features/Lessons/LessonCatalogDisplayProcessTests.cs:104 | Primary constructor conversion; separate semantic review |
| info | IDE0046 | tests/KeyForge.Tests/UnitTest/Components/Audio/AudioPlaybackServiceTests.cs:477 | Conditional/control-flow rewrite; separate semantic review |
| info | IDE0290 | tests/KeyForge.Tests/ProcessTest/Features/Lessons/LessonDetailDisplayProcessTests.cs:114 | Primary constructor conversion; separate semantic review |
| info | IDE0090 | tests/KeyForge.Tests/UnitTest/Features/Progress/Services/ProgressUpdateServiceTests.cs:20 | Object-creation structural suggestion; outside chosen formatting fixes |
| info | IDE0290 | tests/KeyForge.Tests/UnitTest/Features/Progress/Services/ProgressUpdateServiceTests.cs:457 | Primary constructor conversion; separate semantic review |
| info | IDE0290 | tests/KeyForge.Tests/UnitTest/Features/Progress/Services/ProgressUpdateServiceTests.cs:436 | Primary constructor conversion; separate semantic review |
| info | IDE0305 | tests/KeyForge.Tests/UnitTest/Features/Lessons/Services/PracticeLibraryQueryServiceTests.cs:82 | Collection-expression conversion; separate semantic review |
| info | IDE0300 | tests/KeyForge.Tests/ProcessTest/Features/Practice/ScalePracticeProcessTests.cs:27 | Collection-expression conversion; separate semantic review |
| info | IDE0300 | tests/KeyForge.Tests/ProcessTest/Features/Practice/ScalePracticeProcessTests.cs:28 | Collection-expression conversion; separate semantic review |
| info | IDE0300 | tests/KeyForge.Tests/ProcessTest/Features/Practice/ScalePracticeProcessTests.cs:32 | Collection-expression conversion; separate semantic review |
| info | IDE0300 | tests/KeyForge.Tests/ProcessTest/Features/Practice/ScalePracticeProcessTests.cs:33 | Collection-expression conversion; separate semantic review |
| info | IDE0290 | tests/KeyForge.Tests/UnitTest/Features/Lessons/Services/LessonProgressQueryServiceTests.cs:418 | Primary constructor conversion; separate semantic review |
| info | IDE0290 | tests/KeyForge.Tests/UnitTest/Features/Lessons/Services/LessonProgressionServiceTests.cs:1009 | Primary constructor conversion; separate semantic review |
| info | IDE0042 | tests/KeyForge.Tests/UnitTest/Features/Practice/Services/PitchComparisonTests.cs:13 | Tuple/deconstruction structural style; separate review |
| info | IDE0305 | tests/KeyForge.Tests/UnitTest/Features/Practice/Services/RhythmChallengeGeneratorTests.cs:40 | Collection-expression conversion; separate semantic review |
| info | IDE0305 | tests/KeyForge.Tests/UnitTest/Features/Practice/Services/RhythmChallengeGeneratorTests.cs:39 | Collection-expression conversion; separate semantic review |
| info | IDE0305 | tests/KeyForge.Tests/UnitTest/Features/Practice/Services/PracticeSessionServiceTests.cs:342 | Collection-expression conversion; separate semantic review |
| info | IDE0305 | tests/KeyForge.Tests/UnitTest/Infrastructure/Yaml/Parsing/LentoVivoContentTests.cs:114 | Collection-expression conversion; separate semantic review |
| info | IDE0305 | tests/KeyForge.Tests/UnitTest/Infrastructure/Yaml/Parsing/RhythmAuthoringTests.cs:332 | Collection-expression conversion; separate semantic review |
| info | IDE0305 | tests/KeyForge.Tests/ProcessTest/Features/Practice/PracticeFlowProcessTests.cs:245 | Collection-expression conversion; separate semantic review |
| info | IDE0090 | tests/KeyForge.Tests/UnitTest/Features/Lessons/Services/ExerciseCompletionEvaluatorTests.cs:39 | Object-creation structural suggestion; outside chosen formatting fixes |
| warning | xUnit2013 | tests/KeyForge.Tests/ProcessTest/Features/Practice/LentoVivoAdaptedPracticeProcessTests.cs:68 | Pre-existing collection-count assertion warning in prior uncommitted test |

## Commit file scopes

### 9f56308 ? FORMAT(repo): enforce editorconfig across application and tests

Files/scope:

- .editorconfig
- KeyForge.slnx
- src/KeyForge/Components/Audio/IAudioPlaybackService.cs
- src/KeyForge/Components/Music/VexFlowStaffRenderRequest.cs
- src/KeyForge/Components/Practice/OrderedNoteProgress.cs
- src/KeyForge/Features/Exercises/Models/EarTrainingExerciseDefinition.cs
- src/KeyForge/Features/Exercises/Models/EarTrainingTask.cs
- src/KeyForge/Features/Exercises/Models/RhythmExerciseDefinition.cs
- src/KeyForge/Features/Exercises/Models/RhythmPatternEvent.cs
- src/KeyForge/Features/Lessons/Models/LessonDefinition.cs
- src/KeyForge/Features/Lessons/Models/LessonLevel.cs
- src/KeyForge/Features/Lessons/Models/LessonListItem.cs
- src/KeyForge/Features/Lessons/Services/IExerciseCompletionEvaluator.cs
- src/KeyForge/Features/Lessons/Services/ILessonCatalog.cs
- src/KeyForge/Features/Lessons/Services/ILessonProgressQueryService.cs
- src/KeyForge/Features/Lessons/Services/ILessonProgressionService.cs
- src/KeyForge/Features/Lessons/Services/LessonProgressQueryService.cs
- src/KeyForge/Features/Lessons/Services/LessonProgressionService.cs
- src/KeyForge/Features/Practice/Models/RhythmChallengeDetails.cs
- src/KeyForge/Features/Practice/Models/SequenceEvaluationDetails.cs
- src/KeyForge/Features/Practice/Services/ExerciseScorer.cs
- src/KeyForge/Features/Practice/Services/IExerciseAttemptRecorder.cs
- src/KeyForge/Features/Practice/Services/IExerciseChallengeGenerator.cs
- src/KeyForge/Features/Practice/Services/IExerciseEvaluator.cs
- src/KeyForge/Features/Practice/Services/IExerciseScorer.cs
- src/KeyForge/Features/Practice/Services/IPracticeSessionService.cs
- src/KeyForge/Features/Practice/Services/PitchSequenceBuilder.cs
- src/KeyForge/Features/Practice/Services/RhythmChallengeGenerator.cs
- src/KeyForge/Features/Practice/Services/RhythmExerciseEvaluator.cs
- src/KeyForge/Features/Practice/Services/SequenceExerciseEvaluator.cs
- src/KeyForge/Features/Progress/Services/IProgressStore.cs
- src/KeyForge/Features/Progress/Services/IProgressUpdateService.cs
- src/KeyForge/GlobalUsings.cs
- src/KeyForge/Infrastructure/Content/Lessons/FileSystemLessonCatalog.cs
- src/KeyForge/Infrastructure/Content/Lessons/Validation/LessonContentValidator.cs
- src/KeyForge/Infrastructure/Yaml/Parsing/IYamlLessonParser.cs
- src/KeyForge/KeyForge.csproj
- src/KeyForge/Program.cs
- tests/KeyForge.Tests/GlobalUsings.cs
- tests/KeyForge.Tests/KeyForge.Tests.csproj
- tests/KeyForge.Tests/ProcessTest/Features/Practice/MidiPracticeProcessTests.cs
- tests/KeyForge.Tests/ProcessTest/Features/Practice/PracticeFlowProcessTests.cs
- tests/KeyForge.Tests/ProcessTest/Features/Practice/PracticeSessionUiFlowProcessTests.cs
- tests/KeyForge.Tests/ProcessTest/Features/Practice/SequencePracticeProcessTests.cs
- tests/KeyForge.Tests/UnitTest/Components/Audio/AudioPlaybackServiceTests.cs
- tests/KeyForge.Tests/UnitTest/Components/Localization/UiTextTests.cs
- tests/KeyForge.Tests/UnitTest/Components/Practice/MetronomeTimingTests.cs
- tests/KeyForge.Tests/UnitTest/Components/Practice/MidiInputAdapterTests.cs
- tests/KeyForge.Tests/UnitTest/Components/Practice/MidiPracticeInputStateTests.cs
- tests/KeyForge.Tests/UnitTest/Components/Practice/RhythmKeyboardInputAdapterTests.cs
- tests/KeyForge.Tests/UnitTest/Components/Practice/ScaleFingeringGuideTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Lessons/LessonProgressionModelTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Lessons/Services/ExerciseCompletionEvaluatorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Models/MusicalNoteTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Models/RuntimeExerciseTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/EarTrainingChallengeGeneratorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/ExerciseChallengeGeneratorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/ExerciseEvaluatorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/IntervalChallengeGeneratorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/MentalKeyboardChallengeGeneratorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/NoteReadingChallengeGeneratorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/NoteReadingExerciseEvaluatorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/OctaveChallengeGeneratorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/PracticeSessionServiceTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/RhythmChallengeGeneratorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/RhythmExerciseEvaluatorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/SequenceExerciseEvaluatorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/SpeedChallengeGeneratorTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Progress/Services/ProgressUpdateServiceTests.cs
- tests/KeyForge.Tests/UnitTest/Infrastructure/Content/Lessons/FileSystemLessonCatalogTests.cs

### dda4a68 ? FEATURE(practice): introduce reusable musical performance events

Files/scope:

- src/KeyForge/Components/Pages/Practice.razor
- src/KeyForge/Components/Practice/PerformanceFeedback.razor
- src/KeyForge/Components/Practice/PerformanceWorkspace.razor
- src/KeyForge/Content/Lessons/Performance01.yaml
- src/KeyForge/Features/Exercises/Models/PerformanceDefinition.cs
- src/KeyForge/Features/Exercises/Models/SequenceExerciseDefinition.cs
- src/KeyForge/Features/Practice/Models/ExerciseChallenge.cs
- src/KeyForge/Features/Practice/Models/ExerciseEvaluationResult.cs
- src/KeyForge/Features/Practice/Models/PerformanceChallengeDetails.cs
- src/KeyForge/Features/Practice/Services/ExerciseChallengeGenerator.cs
- src/KeyForge/Features/Practice/Services/ExerciseEvaluator.cs
- src/KeyForge/Features/Practice/Services/PerformanceExerciseEvaluator.cs
- src/KeyForge/Features/Practice/Services/SequenceChallengeGenerator.cs
- src/KeyForge/Infrastructure/Yaml/Validation/YamlLessonValidator.cs
- tests/KeyForge.Tests/ProcessTest/Features/Practice/PerformancePracticeProcessTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/PerformanceExerciseEvaluatorTests.cs

### 98de9fe ? FEATURE(scoring): evaluate MIDI note hold durations

Files/scope:

- src/KeyForge/Components/Practice/PerformanceFeedback.razor
- src/KeyForge/Components/Practice/PerformanceWorkspace.razor
- src/KeyForge/Content/Lessons/Performance01.yaml
- src/KeyForge/Features/Exercises/Models/PerformanceDefinition.cs
- src/KeyForge/Features/Practice/Models/PerformanceChallengeDetails.cs
- src/KeyForge/Features/Practice/Services/PerformanceExerciseEvaluator.cs
- src/KeyForge/Features/Practice/Services/PerformanceMidiPairing.cs
- src/KeyForge/Features/Practice/Services/SequenceChallengeGenerator.cs
- tests/KeyForge.Tests/ProcessTest/Features/Practice/PerformancePracticeProcessTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/PerformanceDurationTests.cs

### 2595aae ? FEATURE(practice): support polyphonic and two-hand exercises

Files/scope:

- src/KeyForge/Components/Practice/PerformanceFeedback.razor
- src/KeyForge/Content/Lessons/Performance01.yaml
- src/KeyForge/Features/Practice/Models/PerformanceChallengeDetails.cs
- src/KeyForge/Features/Practice/Services/PerformanceExerciseEvaluator.cs
- tests/KeyForge.Tests/ProcessTest/Features/Practice/PolyphonicPracticeProcessTests.cs

### d048956 ? FEATURE(notation): render sharp notes in reading practice

Files/scope:

- src/KeyForge/Components/Music/VexFlowNotationMapper.cs
- src/KeyForge/Components/Music/VexFlowStaff.razor.js
- src/KeyForge/Components/Music/VexFlowStaffRenderRequest.cs
- src/KeyForge/Content/Lessons/Performance01.yaml
- tests/KeyForge.Tests/ProcessTest/Features/Practice/PerformancePracticeProcessTests.cs
- tests/KeyForge.Tests/UnitTest/Components/Music/VexFlowNotationMapperTests.cs
- tests/KeyForge.Tests/UnitTest/Components/Music/VexFlowStaff.runtime.test.mjs

### b973fd5 ? FEATURE(practice): author major and minor arpeggio inversions

Files/scope:

- src/KeyForge/Content/Lessons/Performance01.yaml
- src/KeyForge/Features/Exercises/Models/ArpeggioConfiguration.cs
- src/KeyForge/Features/Exercises/Models/SequenceExerciseDefinition.cs
- src/KeyForge/Features/Practice/Services/PitchSequenceBuilder.cs
- src/KeyForge/Features/Practice/Services/SequenceChallengeGenerator.cs
- src/KeyForge/Infrastructure/Yaml/Validation/YamlLessonValidator.cs
- tests/KeyForge.Tests/ProcessTest/Features/Practice/ArpeggioPracticeProcessTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/ArpeggioTests.cs

### 40bd04a ? FEATURE(practice): transpose authored sequences and performance timelines

Files/scope:

- src/KeyForge/Content/Lessons/Performance01.yaml
- src/KeyForge/Features/Exercises/Models/SequenceExerciseDefinition.cs
- src/KeyForge/Features/Practice/Services/PitchSequenceBuilder.cs
- src/KeyForge/Features/Practice/Services/SequenceChallengeGenerator.cs
- src/KeyForge/Infrastructure/Yaml/Validation/YamlLessonValidator.cs
- tests/KeyForge.Tests/ProcessTest/Features/Practice/ArpeggioPracticeProcessTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/SequenceTranspositionTests.cs

### 108e916 ? FEATURE(import): preview selected MusicXML parts as validated practice YAML

Files/scope:

- src/KeyForge/Components/Layout/NavMenu.razor
- src/KeyForge/Components/Pages/Import.razor
- src/KeyForge/Infrastructure/Content/Import/MusicXmlPerformanceImporter.cs
- tests/KeyForge.Tests/ProcessTest/Features/Practice/MusicXmlImportProcessTests.cs
- tests/KeyForge.Tests/UnitTest/Infrastructure/Content/MusicXmlPerformanceImporterTests.cs

### a1aec05 ? FEATURE(scoring): support opt-in pedal-aware MIDI release timing

Files/scope:

- src/KeyForge/Components/Practice/PerformanceWorkspace.razor
- src/KeyForge/Content/Lessons/Performance01.yaml
- src/KeyForge/Features/Exercises/Models/PerformanceDefinition.cs
- src/KeyForge/Features/Practice/Services/PerformanceExerciseEvaluator.cs
- src/KeyForge/Features/Practice/Services/PerformanceMidiPairing.cs
- tests/KeyForge.Tests/ProcessTest/Features/Practice/PerformancePracticeProcessTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/PerformancePedalTests.cs

### b29f345 ? FEATURE(scoring): assess dynamics using explicit keyboard calibration

Files/scope:

- src/KeyForge/Components/Practice/PerformanceFeedback.razor
- src/KeyForge/Components/Practice/PerformanceWorkspace.razor
- src/KeyForge/Features/Exercises/Models/PerformanceDefinition.cs
- src/KeyForge/Features/Practice/Models/PerformanceChallengeDetails.cs
- src/KeyForge/Features/Practice/Services/PerformanceExerciseEvaluator.cs
- src/KeyForge/Features/Practice/Services/SequenceChallengeGenerator.cs
- tests/KeyForge.Tests/ProcessTest/Features/Practice/DynamicsPracticeProcessTests.cs
- tests/KeyForge.Tests/UnitTest/Features/Practice/Services/PerformanceDynamicsTests.cs

### d18e806 ? FEATURE(notation): display grand-staff note-reading questions

Files/scope:

- src/KeyForge/Components/Music/VexFlowNotationMapper.cs
- src/KeyForge/Components/Music/VexFlowStaff.razor
- src/KeyForge/Components/Music/VexFlowStaff.razor.js
- src/KeyForge/Components/Music/VexFlowStaffRenderRequest.cs
- src/KeyForge/Content/Lessons/Performance01.yaml
- tests/KeyForge.Tests/ProcessTest/Features/Practice/PerformancePracticeProcessTests.cs
- tests/KeyForge.Tests/UnitTest/Components/Music/MusicStaffVexFlowMappingTests.cs
- tests/KeyForge.Tests/UnitTest/Components/Music/VexFlowNotationMapperTests.cs
- tests/KeyForge.Tests/UnitTest/Components/Music/VexFlowStaff.runtime.test.mjs

## Import references

The minimal importer uses MusicXML's quarter-note division units and cursor movement rules. Primary references: [MusicXML divisions](https://www.w3.org/2021/06/musicxml40/musicxml-reference/elements/divisions/) and [MusicXML duration](https://www.w3.org/2021/06/musicxml40/musicxml-reference/elements/duration/). Unsupported structures are rejected rather than translated by guesswork.
