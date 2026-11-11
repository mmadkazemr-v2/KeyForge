# Regular Piano Practice Feature Freeze Criteria

The intended freeze is for the selected regular-piano scope derived from the verified 75-session LentoVivo course. It is not permission to add speculative features or to call a source-incomplete curriculum complete.

## Freeze entry criteria

### Course coverage

- All 75 canonical session positions have a stable, verified source URL or an explicitly accepted/unresolved source-gap record. URL-only leads do not count as source-verified sessions.
- Every available session has verified chapter/topic metadata and its activity types classified as MIDI Scored, Guided Practice, Theory, Ear Training, or Source-Linked Performance.
- Exact score-dependent targets (pitches, octave, sequence, meter, rests, durations, voice/hand assignments) are marked verified only after readable authorized source inspection.
- Adapted KeyForge drills remain labeled adapted and are never silently promoted to official transcriptions.
- Each in-scope activity is either implemented, deliberately guided/source-linked, or explicitly deferred with a reason.

### Software and evaluation

- Each scored activity has a defined objective acceptance rule, expected MIDI events, feedback behavior and completion criterion.
- Each guided activity makes the learner-facing requirement clear and does not claim the software measures physical fingering, hand position, posture, touch or expression unless a validated sensor/evaluator actually does so.
- Displayed notation and scored event data agree for every scored source-backed activity.
- Catalog, unlock, retry, attempt history and progress behavior have regression coverage for representative course flows.
- Release build, full automated suites, formatting checks and the repository's approved content validation pass without unresolved failures.

### Manual practice acceptance

- Task 66's physical MIDI hardware acceptance is completed and recorded separately; automated test results are not physical acceptance.
- A learner can connect/select the intended MIDI source, see clear connection/change state, practice and retry, and understand the result.
- Representative single-note, repeated-note, timing, simultaneous-note/two-hand (only if required), disconnect/reconnect and device-change scenarios have been exercised on actual supported hardware.
- Human review confirms the written technique guidance is suitable; MIDI-only assessment makes no physical-hand/fingering claim.

### Rights and source traceability

- No unlicensed score images, book scans, MIDI files or audio are redistributed in the repository.
- Each source-linked activity points to the learner's authorized material; stored targets are limited to what is permitted and necessary.
- Session → source evidence → activity requirements → YAML identifier → tests is traceable.

## Freeze operating rules

- Fix defects, accessibility issues, security issues, and regressions within the approved course scope.
- Add a capability only when a verified in-scope course activity cannot be delivered correctly with the frozen implementation; document the source, user impact, smallest change, tests and review before implementation.
- Do not add general-purpose advanced music, DAW, pedal, or performance features just because they are technically possible.
- Keep database/persistence replacement, MIDI expansion beyond the accepted device workflows, and future curriculum outside the frozen scope unless separately authorized.

## Exit / reopening

Reopen the freeze only for a documented curriculum requirement, material correctness defect, hardware incompatibility, or an explicitly approved new product scope. Record why the existing frozen capabilities fail and how the change affects course content and tests. Physical Task66 status is tracked independently and must not be inferred from software completion.
