# KeyForge Project Status

## Last Reviewed
2026-08-20

## Overall Status
Backend foundation is solid. Domain models, YAML parsing, lesson catalog, lesson progression, lesson queries, progress storage, and exercise attempt recording are all implemented and tested. The UI remains default Blazor template. Backend MVP is ready for the first UI task.

## Current Architecture
Single-project Blazor Web App with feature-based folder structure. Dependency direction: Presentation -> Infrastructure -> Contracts -> Domain. All domain models are pure POCOs with zero framework dependencies. Infrastructure isolates YAML, filesystem, and persistence concerns.

## Folder Structure
```
KeyForge/
├── global.json                          (SDK 10.0.103)
├── KeyForge.slnx
├── NuGet.config
├── README.md
│
├── src/KeyForge/
│   ├── Program.cs                       (DI + pipeline)
│   ├── GlobalUsings.cs
│   ├── appsettings.json
│   ├── KeyForge.csproj                  (net10.0, YamlDotNet 18.1.0)
│   │
│   ├── Features/
│   │   ├── Exercises/Models/            (16 files: base + 8 concrete types + enums)
│   │   ├── Lessons/Models/              (6 files: LessonDefinition, UnlockRule, CompletionRule, etc.)
│   │   ├── Lessons/Services/            (5 files: 3 interfaces + 2 implementations)
│   │   ├── Practice/Models/             (1 file: ExerciseAttempt)
│   │   ├── Practice/Services/           (1 file: IExerciseAttemptRecorder)
│   │   ├── Progress/Models/             (1 file: LessonProgress)
│   │   ├── Progress/Services/           (1 file: IProgressStore)
│   │   ├── Midi/Models/                 (empty)
│   │   └── Midi/Services/               (empty)
│   │
│   ├── Infrastructure/
│   │   ├── Yaml/Parsing/                (5 files: parser, exercise parser, exceptions)
│   │   ├── Content/Lessons/             (5 files: catalog, options, exceptions)
│   │   ├── Persistence/Progress/        (2 files: JSON store, options)
│   │   ├── Progress/InMemory/           (1 file: InMemoryProgressStore)
│   │   ├── Practice/InMemory/           (1 file: InMemoryExerciseAttemptRecorder)
│   │   └── Midi/                        (empty)
│   │
│   ├── Content/Lessons/                 (3 YAML files: Lesson01-03.yaml)
│   ├── Components/                      (Blazor: App, Routes, Layout, Pages)
│   └── wwwroot/                         (static assets, Bootstrap)
│
└── tests/KeyForge.Tests/
    ├── Features/Lessons/                (3 test files)
    ├── Features/Practice/Models/        (1 test file)
    ├── Features/Practice/Services/      (1 test file)
    ├── Infrastructure/Yaml/Parsing/     (1 test file)
    ├── Infrastructure/Content/Lessons/  (1 test file)
    ├── Infrastructure/Persistence/Progress/ (1 test file)
    ├── Infrastructure/Progress/InMemory/    (1 test file)
    └── Infrastructure/Practice/InMemory/    (1 test file)
```

## Backend Status

### Lessons
- [x] LessonDefinition model
- [x] ExerciseDefinition model (abstract + 8 concrete sealed types)
- [x] Lesson enums (LessonLevel, UnlockMode, CompletionRule)
- [x] Exercise enums (ExerciseType, Difficulty, ClefType, etc.)
- [x] Lesson catalog interface (ILessonCatalog)
- [x] YAML parser (YamlLessonParser + YamlExerciseParser)
- [x] Lesson content file system catalog (FileSystemLessonCatalog)
- [x] Lesson catalog configuration (LessonCatalogOptions)
- [x] Lesson catalog exceptions (Duplicate, NotFound, LoadFailure)
- [x] Lesson progression unlock evaluation (ILessonProgressionService.IsUnlocked)
- [x] Lesson completion evaluation (ILessonProgressionService.IsCompleted)
- [x] Lesson progress query service (ILessonProgressQueryService.GetLessons)
- [x] Lesson list view model (LessonListItem)
- [ ] Lesson content YAML files (3 exist as sample data)
- [ ] Additional lesson content (not required for MVP)

### Progress
- [x] LessonProgress model
- [x] IProgressStore contract (GetProgress, GetAllProgress, SaveProgress)
- [x] InMemoryProgressStore (Singleton, thread-safe)
- [x] JsonProgressStore (JSON file-backed, not registered in DI)
- [x] ProgressStoreOptions configuration
- [ ] Progress update service (not yet required - no UI triggers it)

### Practice
- [x] ExerciseAttempt domain model
- [x] IExerciseAttemptRecorder contract (Record)
- [x] InMemoryExerciseAttemptRecorder (Singleton, thread-safe)
- [ ] Exercise evaluation logic
- [ ] Scoring algorithms
- [ ] Practice session management
- [ ] Exercise execution engine

### MIDI
- [ ] MIDI abstraction layer
- [ ] MIDI device detection
- [ ] MIDI input handling
- [ ] Note event processing
- [ ] Real-time feedback

## Domain Status
- **24 model files** across 4 feature areas (Exercises, Lessons, Practice, Progress)
- All domain models are pure POCOs with no framework dependencies
- Exercise types use polymorphic design with abstract base + 8 concrete sealed subclasses
- Lesson progression rules are data-driven (YAML) and evaluated by services
- ExerciseAttempt captures attempt data without scoring logic

## Infrastructure Status
- **14 implementation files** across 5 subdirectories
- YAML parsing isolated in Infrastructure/Yaml/Parsing/
- Lesson catalog isolated in Infrastructure/Content/Lessons/
- Persistence isolated in Infrastructure/Persistence/Progress/
- In-memory implementations for development/testing
- All infrastructure implementations are registered as Singletons in DI

## UI Status
All UI is still default Blazor template:
- `Home.razor` - "Hello, world!" template
- `Counter.razor` - Template counter demo
- `Weather.razor` - Template weather demo
- `MainLayout.razor` - Default layout with sidebar
- `NavMenu.razor` - Template navigation (Home, Counter, Weather)
- No KeyForge-specific UI exists yet

## Persistence Status
In-Memory implementations are intentional. Database integration is deferred until the final integration phase. Current DI registrations:
- `IProgressStore` -> `InMemoryProgressStore` (Singleton)
- `IExerciseAttemptRecorder` -> `InMemoryExerciseAttemptRecorder` (Singleton)

JSON persistence exists (`JsonProgressStore`) but is NOT registered in DI. It can be swapped in later without changing consumer code.

## Test Status
**Total: 121 tests, all passing, 0 failures**

| Test File | Tests | Coverage |
|-----------|-------|----------|
| YamlLessonParserTests.cs | 20 | YAML parsing, exercise types, metadata, error handling |
| FileSystemLessonCatalogTests.cs | 14 | Catalog loading, ordering, duplicates, edge cases |
| LessonProgressionServiceTests.cs | 24 | Unlock modes, completion rules, prerequisite chains |
| LessonProgressQueryServiceTests.cs | 13 | Lesson list queries, metadata, ordering |
| LessonProgressionModelTests.cs | 10 | Model defaults, data independence |
| JsonProgressStoreTests.cs | 8 | JSON persistence, retrieval, edge cases |
| InMemoryProgressStoreTests.cs | 8 | In-memory storage, DI resolution |
| InMemoryExerciseAttemptRecorderTests.cs | 8 | Attempt recording, ordering, DI resolution |
| ExerciseAttemptRecorderTests.cs | 7 | Contract behavior, thread safety |
| ExerciseAttemptTests.cs | 8 | Model properties, defaults, immutability |

All tests are unit tests using in-memory fakes. No integration or process-level tests exist yet.

## Build Status
- **SDK**: 10.0.400 (runtime), global.json pins 10.0.103 with rollForward: latestFeature
- **Target Framework**: net10.0
- **Build Result**: 0 warnings, 0 errors
- **Test Result**: 121 passed, 0 failed, 0 skipped

## Architecture Review

### Critical
None.

### High
None.

### Medium
- **Known limitation**: `RequireAllExercises` in `CompletionRule` currently always returns `false` from `IsCompleted` because `LessonProgress` lacks per-exercise completion data. This is by design and documented in tests. Will be resolved when exercise attempt tracking connects to lesson completion.

### Low
- `GlobalUsings.cs` includes infrastructure namespaces (`KeyForge.Infrastructure.Content.Lessons`, `KeyForge.Infrastructure.Practice.InMemory`, `KeyForge.Infrastructure.Progress.InMemory`). This is convenient for the current single-project architecture but could be tightened if the project grows.
- `JsonProgressStore` and `ProgressStoreOptions` exist but are not registered in DI. They are ready for future use.

### Info
- `LessonProgressionService` uses a private `IsPrerequisiteMet` method (renamed from `IsLessonCompleted` to avoid conflict with public `IsCompleted`).
- `LessonListItem` uses `required` properties with `init` setters (read-only view model).
- Thread safety is implemented via `lock` in both `InMemoryProgressStore` and `InMemoryExerciseAttemptRecorder`.

## Hard-Coding Audit
**No issues found.**

- No hard-coded lesson IDs in production code (only in XML doc examples)
- No hard-coded exercise IDs in production code (only in XML doc examples)
- No switch statements based on lesson/exercise IDs
- No if statements based on specific lesson/exercise IDs
- No absolute filesystem paths in production code
- No hard-coded content locations (configurable via `LessonCatalogOptions`)
- No hard-coded progression rules (all data-driven via YAML)

## Completed Work
1. **Task 01**: Domain models (ExerciseDefinition + 8 concrete types, LessonDefinition, enums)
2. **Task 02**: Progression models (UnlockRule, CompletionRule, UnlockMode, LessonLevel)
3. **Task 03**: YAML Parser (IYamlLessonParser, YamlLessonParser, 20 tests)
4. **Task 04**: Lesson Content Catalog (ILessonCatalog, FileSystemLessonCatalog, 14 tests)
5. **Architecture Review**: GOOD verdict (9.2/10)
6. **Task 05**: Progress Storage (LessonProgress, IProgressStore, JsonProgressStore, 8 tests)
7. **Task 06**: InMemoryProgressStore (8 tests)
8. **Task 07**: Lesson Unlock Evaluation (ILessonProgressionService.IsUnlocked, 13 tests)
9. **Task 08**: Lesson Completion Evaluation (ILessonProgressionService.IsCompleted, 11 tests)
10. **Task 09**: Lesson Progress Query Service (ILessonProgressQueryService, LessonListItem, 13 tests)
11. **Task 10**: ExerciseAttempt model + IExerciseAttemptRecorder contract (15 tests)
12. **Task 11**: InMemoryExerciseAttemptRecorder implementation (8 tests)

## Remaining Backend Work
The core backend is complete. The remaining backend tasks are:
- Exercise evaluation logic (determines if an exercise attempt was successful)
- Scoring algorithms (calculates numeric scores from attempt data)
- Practice session management (orchestrates a sequence of exercises)
- Progress update service (connects attempt recording to lesson progress updates)
- Exercise completion tracking (resolves `RequireAllExercises` limitation)

These are higher-level features that build on the existing foundation.

## Recommended Next Task
**BACKEND MVP READY FOR UI**

The backend has sufficient functionality for a usable MVP:
- Lessons load from YAML and are discoverable via `ILessonCatalog`
- Lesson list with status (locked/unlocked/completed) via `ILessonProgressQueryService`
- Progress can be stored and retrieved via `IProgressStore`
- Exercise attempts can be recorded via `IExerciseAttemptRecorder`
- Lesson progression rules are evaluated by `ILessonProgressionService`

A first usable UI would replace the template pages with:
1. A lesson catalog page showing all lessons with lock/completion status
2. A lesson detail page showing exercises in a lesson
3. Basic navigation between pages

This would make KeyForge practically usable for browsing lessons and seeing their status.

## Development Roadmap
1. ~~Complete backend foundation~~ (DONE)
2. **Build first usable UI** (NEXT - lesson catalog + lesson details)
3. Connect UI to existing backend services
4. Add practice session UI (exercise display + attempt recording)
5. Implement exercise evaluation and scoring
6. Process/functional tests for user flows
7. Final database integration (if needed)

## Rules To Preserve
- Data-driven lessons (YAML, not C#)
- No hard-coded lesson/exercise IDs
- Pure domain models (no framework dependencies)
- Infrastructure isolation (YAML, filesystem, persistence)
- In-Memory persistence until final phase
- Small focused tasks
- Tests required for every implementation task
- No unnecessary architecture (no CQRS, MediatR, Repository pattern)
