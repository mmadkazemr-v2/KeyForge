# Beginner rhythm authoring (Task 48)

`RhythmExerciseDefinition` accepts either `pattern` (exactly one complete measure) or
`measures` (an explicit list of complete measures). Each event is a typed mapping:

```yaml
type: rhythm
id: rhythm-example
title: Quarter, two eighths, quarter, quarter
timeSignature: 4/4
tempo:
  defaultBpm: 80
pattern:
  - { duration: quarter }
  - { duration: eighth }
  - { duration: eighth }
  - { duration: quarter }
  - { duration: quarter }
```

## Durations, rests, and meter

| Duration | Quarter-note beats | Sixteenth units |
| --- | ---: | ---: |
| `whole` | 4 | 16 |
| `half` | 2 | 8 |
| `quarter` | 1 | 4 |
| `eighth` | 1/2 | 2 |
| `sixteenth` | 1/4 | 1 |
| `dottedHalf` | 3 | 12 |
| `dottedQuarter` | 3/2 | 6 |
| `dottedEighth` | 3/4 | 3 |

Set `rest: true` on any event to occupy its duration without creating an expected
hit. Rests remain in the displayed/compiled timeline. A quarter-rest example is:

```yaml
timeSignature: 4/4
pattern:
  - { duration: quarter }
  - { duration: quarter, rest: true }
  - { duration: quarter }
  - { duration: quarter }
```

The supported meters are `2/4`, `3/4`, `4/4`, and `6/8`. Measure lengths are
numerator × denominator-note duration: respectively 8, 12, 16, and 12 sixteenth
units. Each authored measure must fill that length exactly. For example:

- Four `quarter` events fill 4/4.
- Eight `eighth` events fill 4/4.
- Three `quarter` events fill 3/4.
- Two `quarter` events fill 2/4.
- Six `eighth` events fill 6/8.
- `dottedQuarter`, `eighth`, `half` fill 4/4.

BPM always refers to quarter notes in this foundation. At 60 BPM a quarter is
1000 ms, an eighth 500 ms, and a sixteenth 250 ms. At 120 BPM these halve. In 6/8,
six eighths therefore occupy three quarter-note beats; metronome pulses follow
the six denominator beats. Dotted-quarter BPM for compound meter is not an
alternate tempo convention in this task.

## Multiple measures

```yaml
timeSignature: 4/4
tempo: { defaultBpm: 60 }
measures:
  - pattern:
      - { duration: quarter }
      - { duration: quarter }
      - { duration: quarter }
      - { duration: quarter }
  - pattern:
      - { duration: half }
      - { duration: quarter }
      - { duration: quarter }
```

Do not combine `pattern` and `measures`. `patternCount` is retained only for legacy
pulse definitions; authored exercises use explicit measures. At most 100 measures
are allowed. The test-only fixture is
`tests/KeyForge.Tests/Fixtures/Rhythm/LessonRhythm.yaml` and is outside the
production lesson catalog.

## Compilation, input timing, and evaluation

Musical positions accumulate as integer sixteenth units. Each absolute position
is converted directly to `TimeSpan` ticks using quarter-note BPM with a single
tick rounding; rounded intervals are not repeatedly added. The timeline includes
note and rest events, total duration, and metronome pulse offsets. Only note
onsets become `ExpectedInput` targets. Durations do not evaluate how long a key is
held.

The browser origin is established by `RhythmWorkspace.startRhythm` after the
existing metronome count-in has finished. Count-in is excluded from scored
offsets. Count-in retains quarter-note clicks; the active 6/8 grid uses eighth-note
pulses. The first timeline event is at offset zero, even if it is a rest.
Computer-keyboard input uses browser elapsed time; MIDI input continues to use
`MIDIMessageEvent.timeStamp - browserTimelineOriginMilliseconds`. UTC receipt
times are historical metadata.

The existing nearest-unmatched-onset scoring remains: ≤40 ms excellent,
≤80 ms good, ≤120 ms acceptable, otherwise missed/extra. Hits in rest interiors
have no target and incur the existing five-point extra-hit penalty. Onset
tolerance also applies to an early note immediately after a rest. A rest-only
exercise scores silence as 100 and uses that same extra-hit penalty for taps.

VexFlow renders authored measures through the shared `VexFlowStaff` component,
including dots, rests, and basic beams. The active timeline marks rests separately
and stays active until the complete final duration has elapsed.

## Compatibility and current limits

Legacy exercises with no `pattern` or `measures` retain their numerator ×
`patternCount` uniformly spaced quarter pulses. Their `noteValues` list remains
descriptive metadata, not a generated rhythm. This includes the historical legacy
6/8 pulse interpretation; use authored measures for denominator-aware timing.
Lesson01–03 require no migration.

For the browser fixture check, launch a separate local app instance from the
repository root (PowerShell), then run `tests/KeyForge.Tests/Browser/RhythmAuthoring.browser.js`
with the project's Playwright browser runner:

```powershell
$rhythmFixturePath = (Resolve-Path tests/KeyForge.Tests/Fixtures/Rhythm).Path
dotnet run --project src/KeyForge --no-launch-profile --urls http://127.0.0.1:5266 --KeyForge:LessonCatalog:ContentPath="$rhythmFixturePath"
```

The existing timer/metronome scheduler is retained. Previously observed browser
checks reported about 95–103 ms onset error; this is historical evidence, not a
fresh calibration measurement from Task 48. Browser scheduling, server/circuit
round trips, and audible cue latency remain uncalibrated. They do not replace the
MIDI event timestamp used for evaluation. Ties, tuplets, swing, tempo/meter maps,
polyrhythms, and note-release/duration scoring remain outside this foundation.

## Unmetered phrases (Task 55.2)

Set `unmetered: true` and author one nonempty `pattern`. Do not specify `measures` or a legacy `patternCount` other than 1. No meter is inferred; `timeSignature` is ignored in this mode. Notes and rests use their existing durations. Tempo refers to a quarter-note pulse, and offsets accumulate those durations without padding. Configure `metronome.countInBeats` explicitly when a count-in is desired. Metered definitions retain their existing behavior.

```yaml
unmetered: true
tempo: { defaultBpm: 55 }
metronome: { enabled: true, audio: true, visual: true, countInBeats: 4 }
pattern:
  - { duration: quarter }
  - { duration: half }
  - { duration: dottedHalf }
```
