# KeyForge Exercise & YAML Capability Audit

> **Purpose:** Agent-facing specification and audit task for verifying whether KeyForge's current YAML lesson schema and runtime can support the intended piano, MIDI, rhythm, two-hand, ear-training, and Progressive Metal practice system.
>
> **Important:** This document distinguishes between:
> 1. **CURRENT / IMPLEMENTED YAML** — syntax and capabilities already accepted by the repository today.
> 2. **PROPOSED / NOT YET ACCEPTED YAML** — backward-compatible schema extensions required for advanced exercises.
>
> Do not treat proposed fields as implemented until the parser, models, validators, generators, evaluators, UI, and tests support them.

---

## 1. Goal

Audit the KeyForge repository and answer this question:

> **Can the current YAML-driven exercise architecture represent and evaluate the complete training path we want, from beginner piano to advanced Progressive Metal / Dream Theater-style technical training?**

The audit must cover both:

- **Content expressiveness:** Can the YAML describe the exercise?
- **Runtime capability:** Can KeyForge generate, render, capture, evaluate, score, and persist the exercise correctly?

A YAML field existing in a file is **not enough**. The complete path must work:

```text
YAML
  ↓
YamlLessonParser
  ↓
Exercise Definition
  ↓
Challenge Generator
  ↓
RuntimeExercise
  ↓
User / MIDI Input
  ↓
Evaluator
  ↓
Scoring
  ↓
Attempt / Progress
```

---

# 2. Current KeyForge Architecture

The repository currently contains a usable lesson/practice pipeline and should be extended rather than rewritten.

Important existing pieces:

```text
Content/Lessons/*.yaml
        ↓
FileSystemLessonCatalog
        ↓
YamlLessonParser
        ↓
ExerciseTypeMap
        ↓
YamlLessonValidator
        ↓
LessonDefinition
        ↓
Concrete ExerciseDefinition
        ↓
PracticeSessionService.StartExercise()
        ↓
ExerciseChallengeGenerator
        ↓
RuntimeExercise
        ↓
Input Adapters
        ↓
ExerciseEvaluator
        ↓
ExerciseScorer
        ↓
Attempt / Progress
```

Important runtime input path for MIDI:

```text
Alesis V61 MKII
    ↓ USB MIDI
Browser Web MIDI API
    ↓
MidiInputControl.razor.js
    ↓
MidiInputControl.razor
    ↓
MidiInputRouter
    ↓
MidiInputAdapter
    ↓
ExerciseInputEvent
    ↓
RuntimeExercise
```

The repository already contains Browser Web MIDI support, but the current evaluation contract is still much narrower than the final training system.

---

# 3. Current YAML Conventions

## 3.1 General rules

The current parser uses:

- **camelCase properties**
- polymorphic exercise definitions
- a `type` discriminator
- strongly typed C# models
- validation through `YamlLessonValidator`

Known exercise type discriminator values include:

```yaml
type: rhythm
type: noteReading
type: interval
type: octave
type: mentalKeyboard
type: earTraining
type: speed
type: fingerIndependence
```

These correspond to the current eight exercise-definition families.

Unknown fields must **not** be assumed to work automatically. Because the YAML parser is strongly typed, adding new YAML properties normally requires changes to the corresponding C# model/parser/validation flow.

---

# 4. Current Lesson-Level Shape

The agent must inspect:

```text
Content/Lessons/Lesson01.yaml
Content/Lessons/Lesson02.yaml
Content/Lessons/Lesson03.yaml
```

and compare them to:

```text
Features/Lessons/Models/LessonDefinition.cs
Infrastructure/Yaml/Parsing/YamlLessonParser.cs
Infrastructure/Yaml/Parsing/ExerciseTypeMap.cs
Infrastructure/Yaml/Validation/YamlLessonValidator.cs
```

The exact lesson wrapper must be documented from the repository.

At minimum, the current model includes concepts for:

- lesson identity / metadata
- exercise collection
- unlock rules
- completion rules
- prerequisites
- difficulty
- tempo metadata
- exercise-specific properties

The agent must copy one **real, valid current lesson** into the final report as the canonical example.

Do not fabricate missing fields.

---

# 5. Current Exercise Types

## 5.1 `noteReading`

Current capabilities:

- natural-note questions
- treble clef
- bass clef
- exact pitch/octave evaluation
- limited grand-clef behavior

Known relevant properties/concepts:

```yaml
type: noteReading
```

The concrete model also contains a `keySignature` concept, but the current generator does not fully honor accidentals from the configured key signature.

### Current limitation

The current implementation does **not** provide a complete notation/event representation for:

- chords
- simultaneous notes
- note durations
- rests
- ties
- continuous reading
- true simultaneous grand staff
- timed sight-reading passages

---

## 5.2 `rhythm`

Known relevant concepts:

```yaml
type: rhythm
timeSignature: "4/4"
noteValues:
  - quarter
  - eighth
```

The agent must verify the exact current property names from the concrete model and existing YAML files.

### Current behavior

The runtime currently behaves mostly like a **uniform pulse generator**.

Important limitation:

- `noteValues` exists in the schema, but the generator currently ignores its musical meaning.
- the time-signature denominator is not interpreted correctly
- the current generator primarily uses the numerator as pulse count

Therefore:

```yaml
timeSignature: "7/8"
```

does **not** currently mean a musically correct 7/8 exercise.

The current engine can count seven evenly spaced pulses, but it does not yet model:

```text
beat unit
grouping
note duration
rest duration
subdivision
tuplet
bar structure
```

---

## 5.3 `interval`

Current support includes:

- source note
- target note derived from interval
- ascending / descending interval behavior
- exact note answer

Current limitation:

The engine currently treats the result mostly as a **single target note**, not a full two-note melodic or harmonic interval performance.

The schema/runtime does not yet provide a generic representation for:

```text
two-note phrase
simultaneous harmonic interval
interval duration
part assignment
```

---

## 5.4 `octave`

Current support includes relative octave targets.

Example conceptual usage:

```yaml
type: octave
```

The agent must inspect the model for the exact octave-distance property.

Current limitation:

This is a target-note exercise, not a general simultaneous octave-passage model.

---

## 5.5 `mentalKeyboard`

Current support includes chromatic keyboard movement and black-key targets.

This is useful for:

- mental keyboard geography
- semitone movement
- accidental location

It is not yet a complete randomized keyboard-navigation curriculum.

---

## 5.6 `earTraining`

Known discriminator:

```yaml
type: earTraining
```

Current enum-level task concepts include:

```text
PitchMatching
IntervalRecognition
ChordRecognition
ScaleRecognition
MelodyDictation
```

However, only a subset currently has functional generators/evaluators.

Current working subset:

```text
PitchMatching
IntervalRecognition
```

Current unsupported subset:

```text
ChordRecognition
ScaleRecognition
MelodyDictation
```

Important:

A YAML file may currently contain one of these unsupported task values, but that does not mean the exercise is runnable.

The audit must check:

```text
YAML accepted?
Challenge generated?
Audio generated?
Expected inputs created?
Evaluator works?
UI works?
```

---

## 5.7 `speed`

Known discriminator:

```yaml
type: speed
```

The current schema/runtime supports:

- repetitions
- tempo configuration
- a small set of named pattern kinds

Current pattern behavior includes hard-coded musical material such as:

```text
scale fragment
arpeggio
repeated note
chromatic pattern
```

### Current limitation

The YAML does **not** currently define a general authored note sequence.

The pitches are partly hard-coded in generator logic.

This means the content author cannot yet freely write:

```yaml
sequence:
  - C4
  - D4
  - E4
  - G4
```

and expect the existing runtime to treat it as a generic speed exercise.

---

## 5.8 `fingerIndependence`

Known discriminator:

```yaml
type: fingerIndependence
```

Current concepts include:

- hand metadata
- finger-number instructions
- fixed note mapping

Conceptually, current YAML can describe metadata similar to:

```yaml
type: fingerIndependence
hand: right
fingerNumbers:
  - 1
  - 2
  - 3
  - 4
  - 5
```

The agent must verify the exact accepted property names and enum values.

### Important limitation

MIDI cannot determine which physical finger was used.

Therefore KeyForge can currently:

- instruct the player to use a finger
- verify the resulting note sequence

but it cannot prove that the requested finger was actually used.

---

# 6. Current Tempo Configuration

The repository has a typed tempo configuration.

Known supported concepts include:

```yaml
tempo:
  defaultBpm: 80
  allowUserChange: true
  minBpm: 50
  maxBpm: 140
  step: 5
```

The agent must verify the exact current model/property names.

Current system supports:

- selected BPM
- min/max bounds
- step
- user tempo selection
- target-tempo metadata

Current system does **not** yet support a true tempo map such as:

```text
80 BPM at bar 1
90 BPM at bar 5
120 BPM at bar 9
```

inside one exercise.

---

# 7. What the Current YAML Can Represent Well

The current schema is already suitable or close to suitable for:

### Basic note work

```text
single displayed note
exact pitch recognition
octave target
keyboard note location
simple accidental / chromatic navigation
```

### Simple interval drills

```text
find target note from source + interval
ascending / descending target
```

### Simple speed drills

```text
fixed scale fragment
fixed arpeggio
fixed chromatic pattern
fixed repeated-note pattern
```

### Simple rhythm drills

```text
uniform pulses
fixed BPM
basic hit timing
early / late feedback
```

### Basic ear-training subset

```text
pitch matching
melodic interval recognition
```

### Basic finger instruction

```text
hand metadata
finger-number instruction
fixed note sequence
```

---

# 8. What the Current YAML Cannot Faithfully Represent

The current schema cannot represent the complete desired piano / Progressive Metal training system.

The following concepts are missing or incomplete.

---

## 8.1 General authored note events

Missing:

```yaml
sequence:
  - ...
```

There is no general event sequence allowing arbitrary authored musical material.

This blocks or limits:

- custom scale exercises
- modal runs
- original riffs
- complex arpeggios
- Hanon-style patterns
- original Progressive Metal sequences
- melody reproduction

---

## 8.2 Simultaneous notes / chords

Missing representation:

```yaml
notes: [C4, E4, G4]
```

as one expected event.

This blocks:

- triads
- seventh chords
- chord inversions
- chord voicings
- harmonic intervals
- octave passages
- chords + melody
- two-hand simultaneous playing

---

## 8.3 Note duration

The current `duration` concept in lesson metadata is not a musical note duration.

Missing:

```yaml
duration: "1"
duration: "1/2"
duration: "1/4"
```

for musical events.

This blocks accurate evaluation of:

- whole notes
- half notes
- articulation
- legato
- staccato
- held notes
- ties
- pedal-aware sounding spans

---

## 8.4 Rests

Missing:

```yaml
rest: true
```

This blocks proper evaluation of:

- silence
- syncopation
- rests
- unwanted hits during a rest

---

## 8.5 Polyphonic parts / hands

Missing:

```yaml
parts:
  - id: right
    hand: right
  - id: left
    hand: left
```

This blocks faithful representation of:

- two-hand coordination
- bass + melody
- chords + melody
- left-hand ostinato + right-hand lead
- contrary-motion scales
- independent subdivisions
- polyrhythm

---

## 8.6 Musical timeline

Missing:

```yaml
timeline:
```

with:

- beat unit
- meter
- meter changes
- tempo changes
- grouped odd meters

Without this, advanced rhythm cannot be described musically.

---

## 8.7 Velocity targets

Missing:

```yaml
velocity:
  min: 65
  max: 90
```

This blocks YAML-authored dynamics such as:

```text
p
mp
mf
f
accent
crescendo
decrescendo
```

Velocity is captured from MIDI today, but target dynamics are not part of the content schema.

---

## 8.8 Sustain pedal targets

Missing:

```yaml
controlChange:
  controller: 64
  value: 127
```

Current MIDI presentation recognizes sustain state, but exercise content cannot author expected pedal behavior.

---

## 8.9 Pitch Bend / Modulation

Missing content representation for:

```text
Pitch Bend
CC1 modulation
generic CC targets
controller envelopes
```

This is required later for synth-performance exercises.

---

## 8.10 Configurable scoring

Current scoring behavior is mostly hard-coded.

Missing content such as:

```yaml
scoring:
  allowedEarlyMs: 35
  allowedLateMs: 45
  chordSpreadMs: 40
```

and dimension weights.

This becomes important for different exercise categories.

A note-identification exercise and a high-speed rhythmic exercise should not use identical scoring rules.

---

# 9. Proposed Backward-Compatible YAML Extension

> **STATUS: PROPOSED — NOT CURRENTLY ACCEPTED BY KEYFORGE**

The current eight exercise types should remain valid.

Do not replace them.

Extend them with optional reusable musical structures.

Example:

```yaml
- type: speed
  id: original-pattern
  title: Original coordination pattern

  tempo:
    defaultBpm: 80
    allowUserChange: true
    minBpm: 50
    maxBpm: 140
    step: 5

  timeline:
    beatUnit: quarter

    meters:
      - at: "0"
        signature: "7/8"
        grouping: [2, 2, 3]

      - at: "7/2"
        signature: "4/4"

    tempos:
      - at: "0"
        bpm: 80

      - at: "15/2"
        bpm: 90

  parts:
    - id: right
      hand: right

    - id: left
      hand: left

  sequence:
    - at: "0"
      duration: "1/2"
      part: right
      notes: [C4, E4, G4]
      fingers: [1, 3, 5]
      velocity:
        min: 65
        max: 90

    - at: "1/2"
      duration: "1/3"
      part: right
      notes: [D4]

    - at: "5/6"
      duration: "1/6"
      part: right
      rest: true

    - at: "1"
      controlChange:
        controller: 64
        value: 127

  scoring:
    allowedEarlyMs: 35
    allowedLateMs: 45
    chordSpreadMs: 40
```

Suggested rule:

> Musical `at` and `duration` values use rational quarter-note units.

Examples:

```text
1    = one quarter note
1/2  = one eighth note
1/3  = one third of a quarter-note unit
2    = two quarter-note units
```

Do not accumulate musical positions with floating-point arithmetic.

Use a small rational/fraction value type.

---

# 10. Proposed Core YAML Concepts

The agent should evaluate whether these should become reusable schema types.

## `sequence`

Represents authored musical events.

```yaml
sequence:
  - at: "0"
    notes: [C4]
    duration: "1"

  - at: "1"
    notes: [D4]
    duration: "1"
```

---

## `parts`

Represents voices/hands.

```yaml
parts:
  - id: left
    hand: left

  - id: right
    hand: right
```

Possible later metadata:

```yaml
range:
  min: C2
  max: B3

midiChannel: 1
```

Do not use channel-based hand detection unless explicitly configured.

---

## `timeline`

Represents musical time.

```yaml
timeline:
  beatUnit: quarter

  meters:
    - at: "0"
      signature: "7/8"
      grouping: [2, 2, 3]

  tempos:
    - at: "0"
      bpm: 90
```

---

## `scoring`

Represents exercise-specific evaluation.

```yaml
scoring:
  allowedEarlyMs: 40
  allowedLateMs: 50
  chordSpreadMs: 45
```

Possible later dimensions:

```yaml
dimensions:
  pitch: 0.7
  timing: 0.3
```

Do not require irrelevant dimensions.

For example, a beginner note-reading drill should not lose points because no sustain pedal was used.

---

## `tempoProgression`

Potential future syntax:

```yaml
tempoProgression:
  startBpm: 70
  increment: 5
  requiredScore: 90
  consecutivePasses: 3
  maxBpm: 140
```

This would support structured speed development.

---

# 11. Can the Desired Exercises Be Done With the Current Structure?

## Status legend

```text
YES
PARTIAL
NO — SCHEMA EXTENSION REQUIRED
NO — RUNTIME + SCHEMA EXTENSION REQUIRED
```

| Exercise Family | Current YAML | Runtime | Verdict |
|---|---|---|---|
| Single-note recognition | Good | Good | YES |
| Natural-note sight reading | Good | Good | YES |
| Accidentals / key signatures | Partial | Partial | PARTIAL |
| Keyboard geography | Partial | Partial | PARTIAL |
| Simple interval targets | Good | Good | YES |
| Melodic interval performance | Limited | Limited | PARTIAL |
| Harmonic intervals | Missing groups | Missing polyphony | NO |
| Octave target questions | Good | Good | YES |
| Simultaneous octave passages | Missing groups | Missing polyphony | NO |
| Basic pulse rhythm | Good enough | Existing | YES |
| Whole/half/eighth/etc. musical rhythm | Incomplete | Generator incomplete | PARTIAL |
| Rests | Missing | Missing | NO |
| Ties | Missing | Missing NoteOff evaluation | NO |
| Syncopation | Missing authored events | Offsets can help | PARTIAL |
| Triplets / tuplets | Missing | Missing rational timeline | NO |
| Major scale fragments | Existing | Hard-coded subset | PARTIAL |
| Full scales in arbitrary keys | Missing generic sequence/formula | Extendable | PARTIAL |
| Minor scales / modes | Missing configurable formulas | Extendable | PARTIAL |
| Multi-octave scales | Missing authored range | Extendable | PARTIAL |
| Chords | Missing | Missing chord evaluator | NO |
| Chord inversions | Missing | Missing | NO |
| Chord progressions | Missing | Missing harmonic timeline | NO |
| Arpeggios | Hard-coded subset | Existing sequence evaluator | PARTIAL |
| Hanon-style exercises | No generic sequence | Sequence foundation exists | PARTIAL |
| Trills | No configurable pattern | Sequence foundation exists | PARTIAL |
| Repeated notes | Existing fixed pattern | Partial | PARTIAL |
| Two-hand coordination | Missing parts | Missing concurrent timeline | NO |
| Bass + melody | Missing parts | Missing concurrent timeline | NO |
| Left ostinato + right lead | Missing parts | Missing concurrent timeline | NO |
| Velocity exercises | Missing targets | Velocity captured | PARTIAL |
| Crescendo / decrescendo | Missing envelope | Missing evaluator | PARTIAL |
| Sustain exercises | Missing CC targets | Presentation only | PARTIAL |
| Ear pitch matching | Existing | Existing but audio currently needs repair | PARTIAL |
| Interval recognition | Existing | Existing but audio currently needs repair | PARTIAL |
| Chord recognition | Enum only | Unsupported challenge | NO |
| Scale recognition | Enum only | Unsupported challenge | NO |
| Melody reproduction | Missing | Missing | NO |
| BPM progression | Metadata only | Manual tempo only | PARTIAL |
| 5/4 | String representable | Pulse-count subset | PARTIAL |
| 7/8 | String representable | Denominator semantics wrong | PARTIAL |
| 9/8 | String representable | Compound grouping absent | PARTIAL |
| 11/8 | String representable | Grouping absent | PARTIAL |
| 13/8 | String representable | Grouping absent | PARTIAL |
| Changing time signatures | Missing | Missing timeline map | NO |
| Odd-meter grouping | Missing | Missing | NO |
| Polyrhythm | Missing parts/timeline | Missing | NO |
| Polymeter | Missing | Missing | NO |
| Metric modulation | Missing | Missing tempo relation | NO |
| Fast synth lead patterns | Hard-coded subset | Sequence base exists | PARTIAL |
| Modal runs | Missing configurable formula | Extendable | PARTIAL |
| Complex arpeggio sequences | Missing generic sequence | Extendable | PARTIAL |
| Rhythmic displacement | Missing timeline semantics | Missing | NO |
| Pitch Bend exercises | Missing | MIDI bytes discarded | NO |
| Mod Wheel exercises | Missing | CC1 discarded | NO |

---

# 12. Important MIDI Constraint

The Alesis V61 MKII can provide useful performance data including:

```text
NoteOn
NoteOff
MIDI note number
velocity
CC64 sustain
pitch bend
CC1 modulation
other CC messages
```

However, the current normalized exercise runtime does not preserve all of them.

Current important gaps:

```text
browser MIDI timestamp → currently discarded for evaluation
NoteOff → presentation only
CC64 → presentation only
pitch bend → discarded
CC1 → discarded
MIDI channel → discarded downstream
```

Therefore a YAML field should **not** be added until the matching runtime event can reach evaluation.

---

# 13. Required Runtime Evolution

The correct direction is to extend the existing runtime incrementally.

Do not build a second practice engine.

Reuse:

```text
PracticeSession
RuntimeExercise
IExerciseChallengeGenerator
IExerciseEvaluator
IExerciseScorer
MidiInputRouter
MidiInputAdapter
attempt/progress pipeline
```

Extend normalized input to include:

```text
event kind
NoteOn / NoteOff
controller number
controller value
pitch bend
channel
device identity
browser-relative timestamp
sequence number
optional part attribution
```

Then add a small **Musical Timeline Compiler**.

Conceptually:

```text
YAML musical events
        ↓
Musical Timeline Compiler
        ↓
Expected musical events
        ↓
tempo/meter conversion
        ↓
Expected elapsed timestamps
        ↓
existing runtime/evaluation pipeline
```

---

# 14. Agent Audit Task

The agent must now verify the exact current YAML syntax directly from the repository.

## Step 1 — Extract canonical YAML

Read:

```text
Content/Lessons/Lesson01.yaml
Content/Lessons/Lesson02.yaml
Content/Lessons/Lesson03.yaml
```

Produce:

```text
Current canonical lesson structure
Current canonical exercise examples
Current property names
Current enum values
Current defaults
```

Do not infer from this document when the repository can answer directly.

---

## Step 2 — Map YAML → C# models

For every YAML property, identify:

```text
YAML field
C# property
C# type
required / optional
default
validator
generator consumer
evaluator consumer
UI consumer
```

Recommended output table:

| YAML Path | C# Type | Used By | Actually Effective? |
|---|---|---|---|

This is especially important because some fields currently parse but do not influence runtime behavior.

Examples already known:

```text
rhythm.noteValues
noteReading.keySignature
```

---

## Step 3 — Detect fake schema support

Find properties that:

```text
parse successfully
but
do not affect generation/evaluation
```

Mark them explicitly:

```text
PARSED_BUT_NOT_FUNCTIONAL
```

Do the same for enum values accepted by the parser but unsupported by challenge generation.

---

## Step 4 — Test representability

Attempt to model the following exercises using only current valid YAML.

Do not modify the schema yet.

### Test A — Beginner note

```text
Show C4
Player must press C4
```

Expected: should work.

### Test B — C major scale

```text
C4 D4 E4 F4 G4 A4 B4 C5
80 BPM
```

Determine whether this can be authored entirely in YAML or requires a hard-coded generator.

### Test C — C major chord

```text
C4 + E4 + G4 simultaneously
```

Expected from current audit: cannot be represented faithfully.

### Test D — Basic rhythm

```text
quarter quarter eighth eighth quarter
```

Determine whether current `noteValues` actually changes expected timing.

### Test E — 7/8

```text
7/8 grouped 2+2+3
```

Determine whether current YAML can distinguish:

```text
2+2+3
3+2+2
2+3+2
```

Expected: current structure cannot.

### Test F — Two-hand exercise

```text
Left:
C3 G3 C3 G3

Right:
C4 E4 G4 E4
```

with independent timing.

Expected: current structure cannot represent independent concurrent parts.

### Test G — Sustain

```text
Press C major chord
Press sustain
Release keys
Release sustain one bar later
```

Expected: current YAML/runtime cannot evaluate this.

### Test H — Progressive Metal rhythm

```text
7/8 → 4/4 → 5/8 → 6/8
```

Expected: current schema cannot represent meter changes.

### Test I — Polyrhythm

```text
Left hand: 3
Right hand: 2
over same time span
```

Expected: requires concurrent rational timelines.

### Test J — Synth expression

```text
Play sustained note
bend +2 semitones
apply modulation
return bend to center
```

Expected: current YAML/runtime cannot represent/evaluate this.

---

# 15. Required Final Decision

The agent must answer each desired exercise with one of:

```text
SUPPORTED_NOW
SUPPORTED_WITH_CURRENT_SCHEMA_BUT_RUNTIME_CHANGE_REQUIRED
SCHEMA_EXTENSION_REQUIRED
SCHEMA_AND_RUNTIME_EXTENSION_REQUIRED
NOT_REALISTIC_WITH_MIDI_ALONE
```

Important example:

```text
finger correctness
```

must be categorized as:

```text
NOT_REALISTIC_WITH_MIDI_ALONE
```

because MIDI reports the resulting note, not which physical finger pressed it.

---

# 16. Acceptance Criteria for the Schema

The future YAML architecture is acceptable only if adding a normal new exercise usually means:

```text
add/edit YAML
```

and **not**:

```text
add a new hard-coded pitch array
add exercise-ID-specific C#
add another one-off evaluator
```

The schema/runtime must eventually support generic reusable musical structures.

A good future test is:

> Can an author add a completely new original Progressive Metal keyboard exercise without changing C#?

For ordinary combinations of already-supported concepts, the answer should eventually be **YES**.

---

# 17. Recommended Schema Strategy

Do **not** replace the current exercise-type system.

Keep:

```yaml
type: noteReading
type: rhythm
type: interval
type: octave
type: mentalKeyboard
type: earTraining
type: speed
type: fingerIndependence
```

and extend exercise definitions with reusable optional blocks:

```yaml
tempo:
timeline:
parts:
sequence:
scoring:
tempoProgression:
```

This provides backward compatibility while allowing advanced exercises.

---

# 18. Final Architectural Verdict

Based on the existing audit:

## Application architecture

**Suitable. No rewrite required.**

## Current YAML architecture

**Insufficient for the complete training system.**

It works for:

```text
single-note drills
simple questions
limited fixed sequences
simple pulse rhythm
basic ear training
basic speed patterns
```

It does not yet faithfully model:

```text
polyphony
chords
note duration
rests
ties
multiple concurrent parts
true two-hand exercises
pedal targets
velocity targets
changing meters
tuplets
polyrhythm
polymeter
tempo maps
controller envelopes
general authored musical sequences
```

## Correct direction

Extend the current schema with:

```text
sequence
parts
timeline
scoring
tempoProgression
```

and extend the existing runtime/input contract rather than creating a new engine.

---

# 19. Required Final Agent Report Format

Return the repository audit as:

```text
1. Current Canonical YAML
2. YAML → C# Mapping
3. Parsed-but-Ineffective Fields
4. Current Exercise Representability
5. Current MIDI Event Coverage
6. Missing Schema Concepts
7. Minimal Backward-Compatible Schema Extension
8. Runtime Changes Required
9. Exercise Capability Matrix
10. Proposed YAML Examples
11. Migration / Compatibility Risks
12. Recommended Implementation Order
13. Final Verdict
```

For every claim, include actual file/type references.

Do not mark the audit complete until the current `Lesson01.yaml`–`Lesson03.yaml` files and all eight concrete exercise-definition models have been inspected.

---

# 20. Final Question the Agent Must Answer

At the end, answer clearly:

> **Can KeyForge become a YAML-driven practice engine where beginner piano, sight reading, rhythm, scales, chords, two-hand coordination, ear training, dynamics, sustain, odd meters, polyrhythm, and Progressive Metal exercises can all be authored primarily as content instead of hard-coded C#?**

Answer one of:

```text
YES — CURRENTLY
YES — AFTER INCREMENTAL SCHEMA/RUNTIME EXTENSION
ONLY PARTIALLY
NO
```

Then explain the exact blockers.

