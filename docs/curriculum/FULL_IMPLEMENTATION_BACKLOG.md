# LentoVivo Full-Course Implementation Backlog

This plan preserves canonical session order while grouping work by shared dependencies. It is a gated backlog: later batches remain unscheduled until their source manifest entries are verified. Source references and confidence live in [the manifest](LENTOVIVO_SOURCE_MANIFEST.md); no guessed content is authorized by this plan.

## Global traceability record

Each planned activity should have a lightweight record linking:

`session number → stable source URL → chapter timestamp/page → evidence/confidence → activity class → verified target facts → KeyForge lesson/exercise ID → YAML path → generator/evaluator expectation → tests → manual acceptance`

Keep copyrighted notation, MIDI, and audio out of the repository unless redistribution rights are established. Link the learner to their authorized book/audio where needed.

## Dependency-ordered batches

| Batch | Sessions | Required work and capabilities | Candidate content files | Tests / verification | Source gates and acceptance |
|---|---|---|---|---|---|
| B0 — Course source index | 01–75, canonical order | Collect all stable LentoVivo session URLs, titles, descriptions and chapter markers. Reconcile duplicate/unavailable videos and record evidence confidence. No engine or lesson authoring. | `docs/curriculum/LENTOVIVO_SOURCE_MANIFEST.md` | Link reachability/title/session check; count positions 1–75; independent review of Persian titles/translation | Creator playlist/index or complete public channel source list. Accepted when every position has verified URL or explicit documented gap and topics are sourced. |
| B1 — Requirements baseline | 03–05, then other verified entries in order | Convert source metadata to activity rows (MIDI, guided, theory, ear, source-linked). Reinspect existing adapted content and tests; do not label it exact. | `docs/curriculum/FULL_COURSE_MATRIX.md` | Cross-check each row to manifest and existing YAML/test IDs | Readable authorized pages/video frames for note-level targets. Accepted when each claim links to evidence and uncertainty is explicit. |
| B2 — Shared event/notation contract (conditional) | Only sessions proven to need it; preserve order in content batches | Reconcile scored pitch/onset/duration/voice/rest data with notation; add only required missing semantics (e.g. ties or simultaneous voice representation) | Existing performance/notation model; separate narrow design task before course YAML | Unit tests for event conversion, notation alignment, NoteOn/Off and scoring; process tests for one representative source-backed piece | Run only if source evidence and capability matrix show the requirement. Accepted when display and evaluator share the same authored timeline. |
| B3 — First verified content batch | 03–05, in order | Reconcile Session03 worksheet and Session04/05 pages/videos. Keep guided/source-linked work separate from objectively scored targets. | Existing `src/KeyForge/Content/Lessons/LentoVivo/Session03.yaml`–`Session05.yaml` or new versioned source-backed YAML IDs | YAML parse/catalog; generator expected targets; evaluator positive/negative cases; progress completion; manual notation comparison | Readable source pages/frames, exact student part, and hardware acceptance for applicable MIDI. Accepted only with traceability and no guessed pitches/rhythm. |
| B4 — Book 1 content batches | Verified sessions 06 onward through the last source-confirmed Book 1 activity; retain session ordering | Implement content using existing capabilities first; batch by source-verified shared concepts, not assumed chapter boundaries | `LentoVivo/SessionNN.yaml` after confirming repository schema/path convention | Per-lesson content tests; selected end-to-end MIDI/process tests per activity type; manual source comparison | Each session source/topic/page verified. Add engine slice before dependent YAML if capability is missing; never leapfrog unverified canonical sessions in completion reporting. |
| B5 — Book transition and Book 2 batches | Remaining verified sessions in canonical order | Re-evaluate requirements at transition; add only demonstrated capabilities (e.g. chord/inversion/texture if present in verified source) | `LentoVivo/SessionNN.yaml` | Reuse B4 test requirements; representative tests for new event types | Verify which sessions use Book 2; do not infer from total session count. Publisher support pages do not establish LentoVivo mapping. |
| B6 — Course coverage and release readiness | 01–75 | Audit source-to-content completeness, guided activity visibility, unlocking/progress rules, copyright, accessibility and physical practice acceptance | Documentation plus all approved LentoVivo lesson files | Full Release build/test, content catalog/generator audit, selected process flows, manual review checklist | Every session classified implemented/guided/source-linked/unavailable with evidence; unresolved source gaps prevent a “complete course” claim. |
| B7 — Regular-practice feature freeze | Entire approved course | Freeze scope to required regular piano practice workflows. Fix correctness defects only; no new exercise families without an explicit course requirement and review. | `FEATURE_FREEZE_CRITERIA.md` | Frozen regression suite and manual hardware checklist | Entry criteria in feature-freeze document met and Task66 hardware acceptance closed separately. |

## Canonical session batching rule

Batches may share implementation work, but the course map and user-facing order must remain Session 01, 02 … 75. Mark each position as `source gap`, `source verified / requirements mapped`, `guided content ready`, `scored content ready`, `automated verified`, and `manually verified` as distinct states. YAML presence alone cannot advance a session to complete.

## Immediate next development dependency

Do not begin Session06 exercise authoring. First acquire the creator's 75-session playlist/index and capture its session URLs/chapters. Then obtain readable licensed source material for the already represented Sessions03–05 and reconcile existing adapted content. After that, choose the earliest canonical session with verified activity requirements and check the capability matrix before planning code or YAML changes.
