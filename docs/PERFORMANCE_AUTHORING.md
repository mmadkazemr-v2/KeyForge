# Performance authoring

These extensions are optional fields on `type: sequence`. Existing ordered sequences and scales retain their behavior. Examples below are original instructional content, not LentoVivo transcriptions.

## Timed notes, rests and parts

```yaml
id: original-phrase
title: Original phrase
order: 1000
unlock: { mode: immediate }
completion: { requireAllExercises: true }
exercises:
  - id: two-parts
    type: sequence
    title: Sustain bass with moving melody
    tempo: { defaultBpm: 60 }
    performance:
      timingReference: firstAttack
      scoringMode: keyHold
      onsetToleranceMilliseconds: 150
      releaseToleranceMilliseconds: 150
      events:
        - { pitch: C3, onsetBeats: 0, durationBeats: 4, voice: bass, hand: left }
        - { pitch: C4, onsetBeats: 0, durationBeats: 1, voice: melody, hand: right }
        - { pitch: D4, onsetBeats: 1, durationBeats: 2, voice: melody, hand: right }
        - { onsetBeats: 3, durationBeats: 1, voice: melody, hand: right }
```

All beats use a quarter-note pulse; durations 1,2,3,4 represent quarter, half, dotted-half and whole notes at the selected BPM. Omitted pitch represents a rest. Multiple pitches may share an onset and durations may overlap. Duplicate identical pitch/onset targets are rejected because one piano key cannot identify separate simultaneous instances. Voice/hand labels are instructions; MIDI cannot determine actual hand or fingering.

`firstAttack` is the default timing reference. It assesses relative timing from the learner's first attack. It does not assess leading-rest duration or alignment to REAPER's metronome. `workspaceStart` is an explicit browser-start reference; it also does not establish REAPER synchronization.

Scoring modes:

- `onsetOnly` (default): pitch/attack timing; NoteOff is not required.
- `keyHold`: physical key duration, paired per MIDI device/channel/pitch. Pedal does not change mechanical hold duration.
- `pedalSustain`: release endpoints include CC64-latched sustain until pedal-up. This is a MIDI-based endpoint, not measurement of acoustic decay. A correctly held note can also meet the target without pedal; this mode does not require a specific pedal gesture.

Release tolerance applies to the difference between actual hold/sustain length and authored duration. A missing NoteOff or missing final pedal-up can produce a missing release. Extra attacks reduce score. Incomplete simultaneous target groups cannot succeed solely because the aggregate score reaches 80 percent. Feedback retains the authored part and hand instruction.

The current workspace shows the exact event table and explicit Start, Finish/evaluate and Restart actions. Finish records an attempt through the existing pipeline. Restart/disconnect clears the interrupted capture without recording an attempt. This is not yet complete multi-voice score engraving.

## Calibrated dynamics

```yaml
performance:
  velocityCalibration: { soft: 30, loud: 90, tolerance: 0.15 }
  events:
    - { pitch: C4, onsetBeats: 0, durationBeats: 1, relativeVelocity: 0 }
    - { pitch: D4, onsetBeats: 1, durationBeats: 1, relativeVelocity: 1 }
```

The values 30/90 are examples, not universal thresholds. Measure comfortable soft/loud MIDI velocities on the actual keyboard/setup before authoring calibration. Soft/loud must be ordered values in 1–127. Relative targets range from0(soft) to1(loud); tolerance is in that relative range. Missing measured velocity cannot satisfy an authored velocity target. Without a target, velocity is not scored. Calibration is authored in YAML; an automatic calibration wizard is not included.

## Arpeggios and transposition

```yaml
- id: original-inversion
  type: sequence
  title: Original first-inversion arpeggio
  hand: right
  arpeggio:
    root: C4
    quality: major
    inversion: 1
    octaves: 1
    direction: ascendingDescending
  transposeSemitones: 2
```

Arpeggio quality is `major` or `minor`, inversion0–2, octaves1–4; directions use the existing `ascending`, `descending`, `ascendingDescending` names. Generation rejects MIDI pitches outside0–127. Fingering, when supplied, must match the generated sequence length and remains instructional.

Exactly one of `sequence`, `scale`, `arpeggio` or `performance` should be authored. Existing explicit ordered pitches, major/natural-minor scales and their direction/octave contracts remain available. `transposeSemitones` applies to any Sequence source, including performance pitches, without moving onset/duration values or changing IDs. It accepts -48 through48 and rejects out-of-range results. The existing pitch model normalizes chromatic notes to sharps rather than preserving enharmonic spelling.

## Reading notation

`type: noteReading` can use explicit `notes: [C#4, F#4, G#4]` with `clef: treble`, `bass` or `grand`. Grand-staff reading renders both clefs and places a single question on the written staff; it does not identify the physical playing hand. This is separate from multi-voice performance engraving.

## MusicXML import

Open `/import`, choose an uncompressed `.musicxml`/`.xml` score, explicitly select the learner's part, provide a stable lesson ID and a fallback quarter-note BPM, then validate/preview/download the generated YAML. Review it, save it in the configured lesson directory and restart the application. The static filesystem catalog then exposes it through the normal library/Practice route.

Supported: score-partwise documents, explicit divisions and note durations, rests, chord onsets, voices, backup/forward cursor movement, constant tempo and explicit pitched octaves. An imported alteration is converted to the existing MIDI pitch representation. Declared ordinary measures must fill their meter; explicitly implicit pickups may be shorter.

Unsupported constructs fail explicitly: ties, grace/cue/unpitched notes, repeats/endings/navigation, transposing parts, pedal instructions, performance attack/release adjustments and tempo changes. SMF MIDI and compressed MXL import are not implemented. The importer does not infer hand, fingering or exact notation spelling. Its default generated scoring is onset-only; author keyHold explicitly if hold-duration assessment is intended.

No automatic copyrighted course download or shared server-library write occurs. Use material you own or are authorized to import. Keep lesson and exercise IDs stable when revising content to preserve progress identity.

## Audio and acceptance

REAPER supplies piano sound and audible metronome. KeyForge supplies MIDI processing, presentation, evaluation and progress. New performance/import features do not initialize or play an internal instrument. Existing audio components are preserved. Matching BPM does not synchronize beat phase.

Automated tests verify synthetic MIDI and rendering contracts. Final browser layout, Alesis V61 MKII releases/CC64, actual velocity calibration and REAPER timing/audio require manual acceptance. See [development report](CURRICULUM_DEVELOPMENT_REPORT.md) for exact results and remaining gaps.
