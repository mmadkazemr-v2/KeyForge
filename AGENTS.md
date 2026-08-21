# AGENTS.md — KeyForge

## 1. Project Overview

KeyForge is a .NET 10 Blazor Web App for structured piano/music training.

The application is designed around:

- Data-driven lessons defined in YAML
- Progressive lesson unlocking
- Exercise-based practice sessions
- Exercise evaluation and scoring
- Lesson progress tracking
- In-memory persistence during development
- A future optional database/persistence layer
- Future MIDI integration

The project is currently transitioning from backend development to UI development.

---

# 2. Current Project Status

## Backend

Backend is considered COMPLETE for the MVP.

Current status:

- Backend completion: approximately 95%
- Build: 0 warnings, 0 errors
- Tests: 225 total
- Passed: 225
- Failed: 0
- Skipped: 0

The remaining backend items are intentionally deferred:

- MIDI integration
- Optional persistence replacement
- Additional lesson content

DO NOT block UI development because of these items.

---

# 3. Technology Stack

- .NET 10
- C#
- Blazor Web App
- net10.0
- YamlDotNet 18.1.0
- xUnit
- Bootstrap
- In-Memory persistence

The project currently has only one external NuGet dependency:

- YamlDotNet

Do not introduce additional dependencies unless there is a clear and justified requirement.

---

# 4. Architecture

The project uses a simple feature-based architecture inside a single Blazor Web App.

Conceptual dependency direction:

Presentation
    ↓
Infrastructure
    ↓
Contracts / Services
    ↓
Domain

Domain models must remain framework-independent.

Do not introduce unnecessary architecture.

## Explicitly DO NOT introduce unless there is a real requirement

- CQRS
- MediatR
- Repository Pattern
- Unit of Work
- Generic Repository
- Event Sourcing
- Domain Event infrastructure
- Complicated dependency injection abstractions
- Unnecessary factories
- Unnecessary handlers
- Unnecessary DTO layers

Prefer simple interfaces and focused services.

---

# 5. Core Architectural Principles

## Data-driven lessons

Lessons and exercises must be defined through YAML.

Do not hard-code lesson or exercise definitions in C# production code.

Example:

```text
Content/Lessons/
    Lesson01.yaml
    Lesson02.yaml
    Lesson03.yaml
````

Adding a new lesson should normally require adding/editing YAML rather than modifying business logic.

---

## Domain purity

Domain models must remain pure POCOs.

Do not add:

* Blazor dependencies
* ASP.NET dependencies
* Entity Framework dependencies
* YamlDotNet dependencies
* Infrastructure dependencies

to domain models.

---

## Infrastructure isolation

Infrastructure is responsible for:

* YAML parsing
* File system access
* Persistence implementations
* In-memory storage

Business logic must not directly depend on:

* File paths
* YAML parsing APIs
* concrete persistence implementations

Use interfaces where infrastructure crosses into application/business logic.

---

# 6. Folder Structure

Current important structure:

```text
src/KeyForge/
├── Features/
│   ├── Exercises/
│   │   └── Models/
│   │
│   ├── Lessons/
│   │   ├── Models/
│   │   └── Services/
│   │
│   ├── Practice/
│   │   ├── Models/
│   │   └── Services/
│   │
│   └── Progress/
│       ├── Models/
│       └── Services/
│
├── Infrastructure/
│   ├── Yaml/
│   │   ├── Parsing/
│   │   ├── Validation/
│   │   └── Exceptions/
│   │
│   ├── Content/
│   │   └── Lessons/
│   │
│   ├── Persistence/
│   │   └── Progress/
│   │
│   ├── Progress/
│   │   └── InMemory/
│   │
│   └── Practice/
│       └── InMemory/
│
├── Content/
│   └── Lessons/
│
├── Components/
│   ├── Layout/
│   └── Pages/
│
└── wwwroot/
```

Tests:

```text
tests/KeyForge.Tests/
├── UnitTest/
│   ├── Features/
│   └── Infrastructure/
│
└── ProcessTest/
    └── Features/
```

---

# 7. Testing Rules

Testing is mandatory for implementation tasks.

Every production-code change must have appropriate tests.

## UnitTest

Use:

```text
tests/KeyForge.Tests/UnitTest/
```

Unit tests should verify isolated behavior.

Examples:

* Service behavior
* Model behavior
* Validation
* Evaluation
* Scoring
* Progress calculations
* Infrastructure components

Do not make unit tests dependent on the full application pipeline.

---

## ProcessTest

Use:

```text
tests/KeyForge.Tests/ProcessTest/
```

Process tests verify meaningful multi-service flows.

Examples:

```text
Start Session
    ↓
Submit Attempt
    ↓
Evaluate
    ↓
Score
    ↓
Record Attempt
    ↓
Update Progress
    ↓
Evaluate Lesson Completion
```

Do not duplicate every unit test as a process test.

Process tests should focus on important end-to-end business flows.

---

# 8. Test Expectations

Current baseline:

```text
Total: 225
Passed: 225
Failed: 0
Skipped: 0
```

Never intentionally reduce test coverage.

After implementation:

```text
dotnet build
dotnet test
```

must pass.

Do not consider a task complete if existing tests fail.

If behavior changes intentionally, update the affected tests and explain why.

---

# 9. Current Backend Services

Important services currently implemented:

```text
IYamlLessonParser
YamlLessonParser

ILessonCatalog
FileSystemLessonCatalog

IProgressStore
InMemoryProgressStore
JsonProgressStore

IExerciseAttemptRecorder
InMemoryExerciseAttemptRecorder

IExerciseCompletionEvaluator
ExerciseCompletionEvaluator

ILessonProgressionService
LessonProgressionService

ILessonProgressQueryService
LessonProgressQueryService

IExerciseEvaluator
ExerciseEvaluator

IExerciseScorer
ExerciseScorer

IPracticeSessionService
PracticeSessionService

IProgressUpdateService
ProgressUpdateService
```

These services form the current backend foundation.

Do not recreate functionality that already exists.

Inspect existing interfaces and implementations before adding new ones.

---

# 10. Lesson System

Lessons contain:

* Metadata
* Unlock rules
* Completion rules
* Exercises

Current unlock modes include:

```text
Immediate
PreviousLessonCompleted
PrerequisitesCompleted
```

Completion supports:

```text
RequireAllExercises
MinimumScore
```

Lesson progression is data-driven.

The progression service is responsible for determining:

* Whether a lesson is unlocked
* Whether a lesson is completed

---

# 11. Exercise System

Exercise definitions use polymorphism.

Base:

```text
ExerciseDefinition
```

Concrete exercise types currently include:

```text
Rhythm
NoteReading
EarTraining
Interval
Octave
FingerIndependence
MentalKeyboard
Speed
```

Do not use lesson/exercise ID-specific conditionals.

Avoid:

```csharp
if (exercise.Id == "lesson-01")
```

or:

```csharp
switch (exercise.Id)
```

Business behavior should be based on exercise type/configuration, not specific IDs.

---

# 12. Practice Flow

Current practice pipeline:

```text
StartSession
    ↓
Validate lesson
    ↓
Validate unlock state
    ↓
Create PracticeSession
    ↓
SubmitAttempt
    ↓
Validate exercise
    ↓
Evaluate attempt
    ↓
Calculate score
    ↓
Set IsSuccessful
    ↓
Record ExerciseAttempt
    ↓
Update lesson progress
    ↓
Evaluate exercise completion
    ↓
Evaluate lesson completion
```

Important:

`SubmitAttempt()` does NOT automatically advance the session.

The caller/UI is responsible for calling:

```text
Next()
```

after displaying the attempt result.

This is intentional.

The UI should be able to show feedback before moving to the next exercise.

---

# 13. Exercise Evaluation

Current evaluation behavior is intentionally simple.

An attempt is successful when:

* `CompletedAt != null`
* A valid score exists

Exercise-type-specific evaluation is not implemented yet.

Do not invent complex evaluation logic unless explicitly requested.

Future MIDI-based evaluation will be implemented separately.

---

# 14. Scoring

`ExerciseScorer` currently:

* Converts null score to 0
* Clamps score to the valid range
* Produces the numeric exercise score

Do not duplicate scoring logic elsewhere.

---

# 15. Exercise Completion

`ExerciseCompletionEvaluator` determines exercise completion from recorded successful attempts.

An exercise is considered completed when at least one successful attempt exists for its exercise ID.

Multiple attempts are allowed.

Example:

```text
Failed
Failed
Successful
Failed
```

The exercise remains completed because a successful attempt exists.

`RequireAllExercises` is evaluated using this mechanism.

---

# 16. Progress

Current progress model:

```text
LessonProgress
    LessonId
    IsCompleted
    BestScore
    AttemptCount
```

Current persistence:

```text
IProgressStore
    ↓
InMemoryProgressStore
```

The in-memory implementation is intentionally used during development.

Data loss on application restart is expected.

DO NOT add a database yet.

---

# 17. Database Rule

Database integration is explicitly deferred.

Do NOT:

* Add EF Core
* Add migrations
* Add DbContext
* Add SQL tables
* Add repository implementations
* Add database configuration

until the UI and required application flows are complete.

Database integration will be a final phase.

---

# 18. JSON Persistence

`JsonProgressStore` already exists and is tested.

It is currently NOT registered in DI.

Do not replace the in-memory implementation unless explicitly requested.

The architecture should allow persistence to be swapped later without changing consumers.

---

# 19. MIDI

MIDI is NOT required for MVP.

MIDI folders currently exist as placeholders.

Do not implement MIDI during the current UI phase unless explicitly requested.

Future MIDI responsibilities may include:

* Device detection
* MIDI input
* Note events
* Real-time feedback
* Exercise-specific MIDI evaluation

---

# 20. UI Status

The backend is ready for UI development.

Current UI is still mostly the default Blazor template.

Current UI pages include template pages such as:

* Home
* Counter
* Weather

No meaningful KeyForge UI has been implemented yet.

---

# 21. Current UI Goal

The next phase is UI development.

The first UI features should be:

1. Lesson catalog
2. Lesson status
3. Lesson detail
4. Exercise list
5. Practice session
6. Attempt submission
7. Result display
8. Navigation between exercises
9. Progress display

Use the existing backend services.

Do not duplicate backend business logic inside Razor components.

---

# 22. UI Architecture Rule

Blazor components should primarily handle:

* Presentation
* User interaction
* Navigation
* Calling application services
* Displaying results

Do NOT put business rules directly inside `.razor` files when the rule belongs in a service.

For example, do not calculate lesson completion in the UI.

Use:

```text
ILessonProgressionService
ILessonProgressQueryService
IPracticeSessionService
IProgressUpdateService
```

instead.

---

# 23. UI Implementation Strategy

Build UI incrementally.

Recommended order:

```text
Task UI-01
Lesson Catalog

Task UI-02
Lesson Detail

Task UI-03
Practice Session UI

Task UI-04
Exercise Result / Feedback

Task UI-05
Progress Display

Task UI-06
Navigation and UX polish

Task UI-07
Important UI process tests
```

Keep each task small.

Do not implement the entire UI in one task.

---

# 24. Task Rules

Every task must be small and focused.

Each task should have:

```text
Task number
Objective
Scope
Files/components involved
Implementation requirements
Tests
Acceptance criteria
```

Do not combine unrelated features into one task.

---

# 25. Task Numbering

Continue the existing task numbering.

The backend currently reached:

```text
Task 19
```

Test refactoring was performed afterward.

Before starting a new implementation task, inspect the current repository and determine the correct next task number.

Do not invent conflicting task numbers.

---

# 26. Commit Rule

Do NOT create commits automatically.

The workflow is:

```text
1. Implement task
2. Run tests
3. Report results
4. Human reviews code
5. Human requests commit
6. Create commit
```

Never create a commit unless explicitly requested.

---

# 27. Code Review Rule

After each implementation task:

* Run tests
* Report changed files
* Explain architectural impact
* Report test count
* Report failures if any
* Stop and wait for review

Do not assume the task is approved.

---

# 28. Small Task Principle

Prefer:

```text
one feature
+
focused implementation
+
focused tests
```

over:

```text
large feature
+
multiple abstractions
+
large refactor
+
many unrelated changes
```

If a task becomes too large, stop and propose splitting it.

---

# 29. Existing Code First

Before implementing something new:

1. Search the existing repository.
2. Find existing interfaces/services/models.
3. Determine whether the behavior already exists.
4. Reuse existing abstractions.
5. Only create new code if necessary.

Do not duplicate existing functionality.

---

# 30. No Overengineering

KeyForge is an MVP-focused project.

Prefer:

```csharp
simple service
simple interface
simple model
```

over complex architectural machinery.

A small application does not need enterprise patterns merely for the sake of patterns.

---

# 31. Error Handling

Use clear validation and meaningful exceptions.

Existing validation patterns include:

```csharp
ArgumentNullException.ThrowIfNull(...)
ArgumentException.ThrowIfNullOrWhiteSpace(...)
```

Preserve the existing conventions.

Invalid operations must not partially mutate application state.

Example:

If an exercise ID is invalid:

```text
reject request
↓
do not record attempt
↓
do not update progress
```

---

# 32. State Mutation

Be careful with mutable state.

Especially:

* PracticeSession
* LessonProgress
* InMemory stores

Do not mutate lesson definitions or stored progress while performing read/query operations.

Existing tests explicitly verify this behavior.

---

# 33. Thread Safety

The in-memory stores are intended to be Singleton services.

Thread safety is therefore important.

Existing implementations use synchronization mechanisms such as:

```text
lock
ConcurrentQueue
```

Do not remove thread-safety guarantees.

---

# 34. Hard-Coding Rules

Production code must NOT contain:

* Hard-coded lesson IDs
* Hard-coded exercise IDs
* Absolute content paths
* Lesson-specific conditionals
* Exercise-ID-specific conditionals
* Hard-coded progression rules

Content paths must remain configurable.

---

# 35. Global Usings

Global usings currently simplify the single-project architecture.

Do not perform unnecessary cleanup/refactoring unless there is a concrete reason.

---

# 36. Build Requirements

Target framework:

```text
net10.0
```

Before declaring a task complete:

```bash
dotnet build
dotnet test
```

Expected baseline:

```text
Build: 0 errors
Tests: 225 passing
```

New tests may increase the test count.

---

# 37. When Tests Are Needed

Tests are required when:

* Adding business logic
* Changing business logic
* Adding validation
* Changing state transitions
* Adding service behavior
* Adding infrastructure behavior

Purely visual UI changes may not require unit tests, but important user flows should eventually receive process-level coverage.

---

# 38. Process Test Philosophy

Do not try to test every possible combination through process tests.

Use process tests for a small number of high-value flows.

The goal is confidence, not maximum test count.

Important flows should cover:

```text
Lesson discovery
Lesson unlock
Start practice
Submit successful attempt
Submit failed attempt
Retry
Advance exercise
Complete lesson
Update progress
```

---

# 39. Current Process Test Coverage

Current process tests cover:

* Complete lesson
* Failed attempt
* Failed then successful retry
* Partial completion
* Invalid attempt
* Retry + advance lifecycle

These are high-value flows and should be preserved.

---

# 40. Agent Behavior

When working on this project:

* Be conservative.
* Inspect before modifying.
* Keep tasks small.
* Reuse existing services.
* Write tests.
* Do not overengineer.
* Do not add database prematurely.
* Do not implement MIDI prematurely.
* Do not commit without explicit approval.
* Do not silently change architecture.
* Do not modify unrelated files.

If uncertain, stop and explain the uncertainty instead of making a large assumption.

---

# 41. Current Priority

Current priority is:

```text
BACKEND
    COMPLETE
        ↓
UI
    CURRENT PHASE
        ↓
PROCESS / FUNCTIONAL TESTING
        ↓
DATABASE
    FINAL PHASE
        ↓
MIDI
    FUTURE PHASE
```

Do not reverse this order unless explicitly requested.

---

# 42. Immediate Next Step

The backend is ready.

The next implementation task should focus on the first KeyForge-specific Blazor UI feature.

Recommended first feature:

```text
Lesson Catalog Page
```

It should use:

```text
ILessonProgressQueryService
```

and display:

* Lesson title
* Description
* Level
* Estimated duration
* Locked/unlocked state
* Completed state
* Best score

The UI should not reimplement lesson progression logic.

---

# 43. Final Principle

Keep KeyForge:

```text
Simple
Data-driven
Tested
Maintainable
Incremental
```

The goal is to reach a usable application quickly without sacrificing clean boundaries or test confidence.

```
این نسخه با وضعیت فعلی فایل گزارش تو هماهنگ است؛ مخصوصاً **225 تست، جداسازی `UnitTest/` و `ProcessTest/`، تکمیل Backend، شروع UI، و تأخیر Database**. :contentReference[oaicite:0]{index=0}
```
