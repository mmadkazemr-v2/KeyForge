# KeyForge Capability Matrix for LentoVivo

Assessment is based on source inspection of lesson models, generators/evaluators, practice components, and tests in the current repository at `f9a72e6`. Task 67.2 added URL-only leads for Sessions 30, 33, 36 and 37 but no retrievable activity requirements; those leads do not change the capability assessment. “Implemented” means a mechanism exists, not that every course activity is authored or source verified.

| Capability | Current status | Evidence / limits | Dependency / requirement |
|---|---|---|---|
| Note reading | Partially Implemented | `NoteReadingExerciseDefinition`, challenge generator, evaluator, staff components and tests exist. Exercise is question/answer style; does not establish comprehensive sight-reading performance or exact source coverage. | Source-derived clef, range, key and target notes; later verify notation mapping against authorized page |
| Musical notation | Partially Implemented | Rhythm and note-reading views render notation; authored performance has a separate event model. Not every articulation/tie/voice/score engraving construct is covered. | A shared timed notation representation for course pieces; only add constructs shown by verified source |
| Rhythm and rests | Partially Implemented | Rhythm YAML supports meter, measures, durations/rests, tempo and generated timelines; evaluator scores onset timing and extra attacks. Rest correctness is indirect; note-off duration is not checked. | Explicitly authored source pattern and human-checkable notation; dedicated rest/absence scoring only if the curriculum needs it |
| Duration-aware notes | Partially Implemented | Performance events support onset/duration and evaluator modes, but ordinary sequence/rhythm paths do not establish physical note hold duration. | Tie/release semantics and reliable MIDI NoteOff handling; acceptance on physical hardware |
| Melodic sequencing | Implemented (limited) | Configured ordered pitches generate expected MIDI note targets; evaluator supports ordering/sequence feedback. Does not score source rhythm or note lengths in generic sequence exercises. | Verified pitches/order/register; timing extensions only when required |
| Scales and technical patterns | Partially Implemented | Scale/arpeggio sequence generation and fingering guide exist. Finger numbers are instructions; MIDI cannot verify which physical finger played. | Source-backed scale/pattern targets; label fingering as guided, not scored |
| Simultaneous notes and chords | Partially Implemented | Performance/polyphonic paths and process tests exist. Notation, tolerant chord windows/rolled chords, and complete course chord vocabulary require per-activity validation. | Verified chord voicings/onset rules; distinguish chord simultaneity from arpeggiation |
| Two-hand coordination | Partially Implemented | Both hands can appear in performance targets; rhythm lesson RH/LH drills are independent and hand identity is not sensed. | Verify keyboard MIDI routing and target voice/hand semantics; possible separate input channels only if source and setup require it |
| Ear training | Implemented (limited) | Interval and ear-training challenge/evaluator paths cover recognition tasks. No claim that every source listening activity maps to current task types. | Map verified listening prompt, answer choices and keys; retain human listening where audio source is unavailable |
| Tempo and timing | Implemented (limited) | Configurable BPM, metronome/count-in and timing evaluators exist. Rhythm timing is relative to KeyForge exercise origin; Task 65 handles device changes. Not synchronized to external REAPER transport. | Course-specific tempo only when verified; physical MIDI acceptance remains separate |
| Performance exercises | Partially Implemented | `PerformanceDefinition`, MIDI pairing/evaluator, dynamics/duration/pedal components and authored performance content exist. Coverage of ties, articulation, expression and all score voices is not established. | Finish shared notation/event contract only for requirements established by verified course sources |
| Feedback and progress | Implemented | Attempt results, score, retries, exercise completion, lesson progression and progress stores/tests exist. Success/completion is software-defined and not pedagogical mastery by itself. | Define transparent per-activity criteria and human completion option for unscorable outcomes |

## Status summary and shared dependencies

No engine feature should be expanded until the full session source index and an authorized/reliable activity requirement are available. Highest shared prerequisites for source-faithful scored pieces are:

Current evidence does not support a complete course requirements freeze: 4 sessions have source metadata verified (03–05 and 14), 4 additional positions have user-supplied URL leads only (30, 33, 36, 37), and 67 positions still lack a session-specific URL in the evidence set. No new capability requirement was established in Task 67.2.

1. Verified source inventory and a traceability record: session → chapter/page → activity → target transcription record → YAML ID → test.
2. One timed musical-event contract shared by notation and scoring, preserving pitch, onset, duration, voice/hand intent and rests where applicable.
3. MIDI input semantics validated on the actual device for NoteOn/NoteOff, simultaneous attacks, repeated notes and input timestamps.
4. Explicit evaluation policy: what is objectively scored, what remains guided, and how retries/progress reflect that distinction.

Do not build generalized advanced features (pedal nuance, broad polyphony, external DAW synchronization, or exhaustive engraving) absent a verified course requirement. Physical hand, posture, touch and fingering assessment require a human or additional sensors and must never be claimed from MIDI note numbers alone.
