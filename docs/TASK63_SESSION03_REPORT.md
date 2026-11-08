# Task 63 - Session 03 worksheet rhythms

Objective: implement the eight previously reported independent hand sequences through existing YAML, challenge, MIDI, attempt and progress infrastructure. No Core redesign or dependencies.

Starting snapshot: main at a03d56116639e3502161bbb74b747754b7b94600; clean index/working tree. Root AGENTS.md reviewed; no nested AGENTS.md found.

## Source confidence

The supplied LentoVivo_Session03_Rhythm.png does not exist. The original @LentoVivo-????? ???? 3.pdf exists, but no pdftoppm or Python renderer was available on PATH during this run. The PDF skill was consulted; no fresh rendering or visual transcription verification was completed. Implementation uses the user's concrete previously reported patterns. YAML descriptions and comments explicitly mark fresh source verification pending. This is software-complete playable transcription content, not a claim of newly verified worksheet fidelity.

## Content

Lesson ID lento-vivo-session-03-adapted and course lento-vivo-piano are preserved. Both quarter-pulse adapted exercises retain their IDs and definitions. Session04/05 are unchanged.

| Exercise ID | Quarter-note beat durations | Total |
|---|---|---|
| lento-vivo-s03-section-01-rh | 1,1,1,1,1,1,2,2,2,2 | 14 |
| lento-vivo-s03-section-01-lh | 1,1,2,2,2,2 | 10 |
| lento-vivo-s03-section-02-rh | 2,1,1,2,1,2,1,1,1 | 12 |
| lento-vivo-s03-section-02-lh | 2,1,1,1,1,1,2,1,2 | 12 |
| lento-vivo-s03-section-03-rh | 3,1,1,3,1,3 | 12 |
| lento-vivo-s03-section-03-lh | 3,1,1,3,3,1 | 12 |
| lento-vivo-s03-section-04-rh | 4,1,1,2,3,4 | 15 |
| lento-vivo-s03-section-04-lh | 4,1,1,2,1 | 9 |

Every new exercise has unmetered: true, one explicit pattern, no authored time signature, no measures and no pitches. Quarter/half/dottedHalf/whole reuse existing duration enums. Default 55 BPM, range 40-70 in steps of 5; four-beat visual count-in and audio:false. Independent RH/LH descriptions explicitly say hand identity is instructional and key holds are not scored.

## Reused path and behavior

YamlLessonParser and existing validators -> FileSystemLessonCatalog -> PracticeLibraryQueryService -> PracticeSessionService/ExerciseChallengeGenerator/RhythmChallengeGenerator -> MidiInputAdapter.TryCreateBeat -> RhythmExerciseEvaluator -> recorder -> ProgressUpdateService. No new audio or scoring dependencies. RhythmWorkspace already renders unmetered notation, sequence, BPM, countdown and feedback. Its heading now displays the exercise title (including hand), and count-in length is visible before starting. Parent exercise presentation retains full instructions. Existing start/restart/cancellation remains intact.

Pitch-independent Beat inputs score onset offsets only. Duration scoring belongs to the performance contract with pitched targets and is not enabled for these rhythms. Written note lengths determine the next attack and total timeline, not NoteOff/key-hold accuracy. Standard MIDI does not establish physical hand identity. Tests vary trigger pitches without changing results.

Task62 audio defaults are unchanged; new content disables authored metronome audio as well. Browser-relative timestamps and post-count-in local origin remain unchanged. REAPER matching BPM does not establish transport/beat-phase alignment. Onset errors are relative to KeyForge's local exercise start, not a synchronized REAPER downbeat.

Progress keeps requireAllExercises:true and minimumScore:60. A successful attempt completes its exercise; failed retries do not erase completion/best score. Adding eight exercises means all ten must be completed for a newly evaluated full lesson completion; no stored data is deleted or migrated, and existing adapted attempt identities remain valid.

## Verification

- dotnet build -c Release --nologo: pass, 0 warnings/errors.
- dotnet test -c Release --no-build --nologo: 706 passed, 0 failed/skipped (682 existing + 24 new pattern/tempo cases).
- Focused filter LentoVivo/Yaml/PracticeLibrary/Progress/LessonCatalog: 262 passed, 0 failed/skipped.
- node --test over all rg-discovered *.runtime.test.mjs: 29 passed, 0 failed.
- dotnet format KeyForge.slnx --no-restore --verify-no-changes --verbosity quiet: pass.
- dotnet format whitespace KeyForge.slnx --no-restore --verify-no-changes --verbosity quiet: pass.
- git diff --check and staged whitespace check: pass.

Each of the eight exercises at 40/55/70 BPM loads actual YAML, validates duration sequence/total, compiled onsets, quarter pulses, pitch-independent expected inputs, catalog/library membership, correct MIDI score 100, early/late penalties, missing hits/failure, extra-hit penalty, attempt recording and completion/progress/best score. The first onset stays at the origin for early-hit tests because pre-origin MIDI is intentionally rejected. Timing assertions allow one 100 ns compiled tick. Existing adapted content tests now scope legacy-only assertions to the preserved adapted drills; all lessons still generate challenges.

## Limits and next task

Browser automation attempted about:blank but failed to connect to its endpoint at 127.0.0.1:9223. No rendered UI, physical MIDI, audible silence or REAPER verification is claimed. New rhythm tests use existing in-memory test stores; existing full-suite durable progress tests remain passing. No Session04/05 content edits and no Task62 setting changes.

Recommended next task: visually verify these eight sequences against a readable worksheet rendering, reconcile any transcription differences, then perform a browser/MIDI practice check using the user's REAPER setup. No unrelated features or remote push.
