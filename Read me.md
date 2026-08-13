\# KeyForge



KeyForge is a data-driven music practice and learning application built with \*\*.NET 10\*\* and \*\*Blazor\*\*.



The goal of KeyForge is to provide a structured environment for learning and practicing keyboard/piano, with support for areas such as:



\- Rhythm

\- Note Reading

\- Ear Training

\- Intervals

\- Octaves

\- Finger Independence

\- Mental Keyboard

\- Speed

\- MIDI Keyboard practice



The main idea behind KeyForge is that \*\*lessons and exercises should be data-driven rather than hard-coded into the application\*\*.



Instead of implementing every new lesson in C#, lessons are defined using YAML files. KeyForge reads these files, parses them into strongly typed domain models, and makes them available to the application.



\---



\## Project Goals



KeyForge is being developed as a personal music-learning and practice platform, with a focus on making deliberate and measurable progress.



The long-term goal is to build an application that can:



1\. Provide structured music lessons.

2\. Combine multiple types of exercises in a single lesson.

3\. Track the user's progress.

4\. Lock/unlock lessons based on completion rules.

5\. Evaluate exercise performance.

6\. Support MIDI keyboards/controllers.

7\. Provide real-time practice feedback.

8\. Allow new lessons to be added without modifying application code.

9\. Eventually support more advanced music-learning capabilities.



\---



\## Core Philosophy



The most important architectural principle of KeyForge is:



> \*\*Content should be data, not code.\*\*



Adding a new lesson should normally require creating a new YAML file rather than modifying C# code.



For example:



```text

Content/

└── Lessons/

&#x20;   ├── Lesson01.yaml

&#x20;   ├── Lesson02.yaml

&#x20;   ├── Lesson03.yaml

&#x20;   └── Lesson04.yaml

````



Adding `Lesson04.yaml` should not require adding a new C# class, changing the UI, or modifying the lesson catalog.



The application is responsible for understanding the lesson schema and executing it.



\---



\# Technology Stack



\## Runtime



\* .NET 10

\* C#

\* ASP.NET Core

\* Blazor Web App



The project currently targets:



```text

net10.0

```



The repository uses `global.json` to pin the SDK version:



```text

10.0.103

```



This is important because the development environment may also contain other .NET SDK versions.



\---



\## UI



The application uses:



\* Blazor Web App

\* Razor Components

\* HTML

\* CSS



The default Blazor template currently exists as the initial application scaffold and will gradually be replaced by the KeyForge-specific UI.



The planned UI will focus on:



\* Lesson catalog

\* Lesson progression

\* Exercise execution

\* Practice feedback

\* Progress tracking

\* Keyboard/MIDI interaction



\---



\## Lesson Content



Lessons are defined using:



```text

YAML

```



YamlDotNet is currently used for parsing YAML content.



The YAML dependency is intentionally isolated inside Infrastructure so that the domain does not depend on YAML or serialization libraries.



\---



\# Architecture



KeyForge currently uses a lightweight layered architecture inside a single application project.



The project is intentionally \*\*not split into multiple projects\*\* because the current size and complexity do not justify that overhead.



The current dependency direction is:



```text

Presentation

&#x20;    ↓

Infrastructure

&#x20;    ↓

Contracts

&#x20;    ↓

Domain

```



More specifically:



```text

┌──────────────────────────────────────────┐

│              Presentation                │

│                                          │

│        Blazor Components / Program       │

└────────────────────┬─────────────────────┘

&#x20;                    │

&#x20;                    ▼

┌──────────────────────────────────────────┐

│             Infrastructure               │

│                                          │

│ YAML Parsing                             │

│ Lesson Catalog                            │

│ Persistence                               │

│ MIDI infrastructure                       │

└────────────────────┬─────────────────────┘

&#x20;                    │

&#x20;                    ▼

┌──────────────────────────────────────────┐

│              Contracts                   │

│                                          │

│ ILessonCatalog                            │

│ Future service abstractions               │

└────────────────────┬─────────────────────┘

&#x20;                    │

&#x20;                    ▼

┌──────────────────────────────────────────┐

│                Domain                    │

│                                          │

│ Lessons                                   │

│ Exercises                                │

│ Progression rules                         │

│ Exercise definitions                      │

└──────────────────────────────────────────┘

```



The architecture deliberately avoids unnecessary patterns such as:



\* CQRS

\* MediatR

\* Repository pattern

\* Event bus

\* Microservices

\* Database abstraction layers that do not have a real use case



The goal is to keep the application \*\*simple, testable, maintainable, and extensible\*\*.



\---



\# Project Structure



The current structure is approximately:



```text

KeyForge/

│

├── global.json

├── NuGet.config

├── KeyForge.slnx

├── README.md

├── setup-folders.ps1

│

├── src/

│   └── KeyForge/

│       │

│       ├── Program.cs

│       ├── appsettings.json

│       ├── KeyForge.csproj

│       │

│       ├── Features/

│       │   │

│       │   ├── Lessons/

│       │   │   ├── Models/

│       │   │   ├── Services/

│       │   │   └── Components/

│       │   │

│       │   ├── Exercises/

│       │   │   ├── Models/

│       │   │   ├── Services/

│       │   │   └── Components/

│       │   │

│       │   ├── Practice/

│       │   ├── Progress/

│       │   └── Midi/

│       │

│       ├── Infrastructure/

│       │   │

│       │   ├── Yaml/

│       │   │   └── Parsing/

│       │   │

│       │   ├── Content/

│       │   │   └── Lessons/

│       │   │

│       │   ├── Persistence/

│       │   ├── Midi/

│       │   └── Yaml/

│       │       └── Validation/

│       │

│       ├── Content/

│       │   └── Lessons/

│       │

│       ├── Components/

│       │

│       └── wwwroot/

│

└── tests/

&#x20;   └── KeyForge.Tests/

```



\---



\# Domain Model



The domain is intentionally kept free from infrastructure concerns.



There are no dependencies from the domain models to:



\* YamlDotNet

\* ASP.NET Core

\* Blazor

\* Filesystem APIs

\* Configuration

\* Database libraries



The domain contains strongly typed models representing lessons and exercises.



\---



\## Lesson



A lesson contains metadata and a collection of exercises.



Conceptually:



```text

Lesson

│

├── Id

├── Title

├── Description

├── Level

├── Order

├── EstimatedMinutes

│

├── UnlockRule

├── CompletionRule

│

└── Exercises

&#x20;     │

&#x20;     ├── RhythmExercise

&#x20;     ├── NoteReadingExercise

&#x20;     ├── EarTrainingExercise

&#x20;     ├── IntervalExercise

&#x20;     ├── OctaveExercise

&#x20;     ├── FingerIndependenceExercise

&#x20;     ├── MentalKeyboardExercise

&#x20;     └── SpeedExercise

```



\---



\# Exercise Types



KeyForge currently supports the following exercise types:



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



The exercise model uses polymorphism.



The base model is:



```text

ExerciseDefinition

```



and each exercise type has its own strongly typed definition.



For example:



```text

ExerciseDefinition

&#x20;       │

&#x20;       ├── RhythmExerciseDefinition

&#x20;       ├── NoteReadingExerciseDefinition

&#x20;       ├── EarTrainingExerciseDefinition

&#x20;       ├── IntervalExerciseDefinition

&#x20;       ├── OctaveExerciseDefinition

&#x20;       ├── FingerIndependenceExerciseDefinition

&#x20;       ├── MentalKeyboardExerciseDefinition

&#x20;       └── SpeedExerciseDefinition

```



This allows the YAML schema to represent different exercise types without introducing dynamic objects or dictionaries into the domain.



\---



\# Lesson Progression



Lessons are not simply a flat list.



KeyForge supports progression rules that determine when a lesson becomes available and when it is considered completed.



A lesson can define an unlock rule such as:



```text

Immediate

PreviousLessonCompleted

PrerequisitesCompleted

```



It can also define completion requirements such as:



```text

MinimumScore

RequireAllExercises

```



For example:



```yaml

unlock:

&#x20; mode: previousLessonCompleted



completion:

&#x20; minimumScore: 70

&#x20; requireAllExercises: true

```



This allows the learning path to be controlled by lesson content rather than hard-coded UI logic.



\---



\# Data-Driven Lesson System



The lesson pipeline is:



```text

YAML File

&#x20;   │

&#x20;   ▼

YamlLessonParser

&#x20;   │

&#x20;   ▼

LessonDefinition

&#x20;   │

&#x20;   ▼

Lesson Catalog

&#x20;   │

&#x20;   ▼

Application Services

&#x20;   │

&#x20;   ▼

Blazor UI

```



The UI does not know where lessons came from.



It only works with the application contract:



```text

ILessonCatalog

```



This means the UI does not need to know about:



\* YAML

\* Files

\* directories

\* YamlDotNet

\* lesson file names



\---



\# YAML Content



Lesson files live under:



```text

src/KeyForge/Content/Lessons/

```



Example:



```text

Lesson01.yaml

Lesson02.yaml

Lesson03.yaml

```



A simplified lesson could look like:



```yaml

id: lesson-01

title: Rhythm Basics

description: Introduction to basic rhythmic patterns

level: Beginner

order: 1

estimatedMinutes: 15



unlock:

&#x20; mode: immediate



completion:

&#x20; minimumScore: 70

&#x20; requireAllExercises: true



exercises:

&#x20; - id: rhythm-01

&#x20;   type: rhythm

&#x20;   title: Basic Quarter Notes

&#x20;   duration: 60

&#x20;   tempo: 60

&#x20;   difficulty: easy

&#x20;   timeSignature: 4/4

&#x20;   noteValues:

&#x20;     - quarter

&#x20;   patternCount: 4

```



The exact YAML schema is maintained separately from the application logic and can evolve as the exercise system becomes more capable.



\---



\# Lesson Catalog



The lesson catalog is responsible for discovering and loading lesson content.



Its contract is:



```text

ILessonCatalog

```



The current implementation is:



```text

FileSystemLessonCatalog

```



Its responsibilities include:



1\. Discovering YAML files.

2\. Reading lesson files.

3\. Parsing YAML.

4\. Detecting duplicate lesson IDs.

5\. Ordering lessons.

6\. Providing lesson lookup.

7\. Exposing lessons as read-only content.



The catalog does not contain UI logic or exercise execution logic.



\---



\# Progress Storage



For the first usable version of KeyForge, the project intentionally avoids introducing a database.



Progress will initially use a simple replaceable persistence mechanism.



The important architectural rule is:



```text

UI / Application

&#x20;       │

&#x20;       ▼

Progress abstraction

&#x20;       │

&#x20;       ▼

Persistence implementation

```



The rest of the application should not depend directly on a specific storage technology.



A database such as SQLite may be introduced later if the application's requirements justify it.



\---



\# MIDI



MIDI support is one of the main long-term goals of KeyForge.



The planned architecture is:



```text

MIDI Keyboard

&#x20;     │

&#x20;     ▼

MIDI Input

&#x20;     │

&#x20;     ▼

MIDI Abstraction

&#x20;     │

&#x20;     ▼

Practice Engine

&#x20;     │

&#x20;     ▼

Exercise Evaluation

&#x20;     │

&#x20;     ▼

Score / Feedback

&#x20;     │

&#x20;     ▼

Progress

```



The MIDI implementation will be kept behind an abstraction so that the rest of the application does not depend directly on a specific MIDI library or device.



\---



\# Testing



Testing is an important part of the development process.



The test project is:



```text

tests/KeyForge.Tests

```



Tests currently cover areas such as:



\* Lesson progression models

\* YAML parsing

\* Polymorphic exercise parsing

\* Invalid YAML

\* Unknown exercise types

\* Lesson catalog behavior

\* Lesson ordering

\* Duplicate lesson IDs

\* Missing lesson IDs

\* Content loading failures

\* Read-only lesson collections



The project follows a simple rule:



> \*\*Every meaningful development task should include appropriate tests.\*\*



New functionality should not be considered complete until its relevant tests pass.



\---



\# Development Principles



KeyForge follows several important development principles.



\## 1. Keep the tasks small



Development is intentionally divided into small steps.



Each step should:



1\. Have a clear goal.

2\. Make a small architectural change.

3\. Include tests.

4\. Build successfully.

5\. Keep existing tests passing.

6\. Be independently reviewable.



This prevents the project from becoming difficult to debug as it grows.



\---



\## 2. Avoid hard-coded lessons



Do not write code such as:



```csharp

if (lesson.Id == "lesson-01")

{

&#x20;   ...

}

```



or:



```csharp

switch (lesson.Id)

{

&#x20;   case "lesson-01":

&#x20;       ...

}

```



Lessons belong in YAML.



The application should interpret the lesson definition.



\---



\## 3. Keep the domain pure



Domain models should not know about:



```text

YAML

Filesystem

Blazor

ASP.NET

Database

MIDI libraries

```



Infrastructure concerns belong in Infrastructure.



\---



\## 4. Prefer strongly typed models



Avoid using:



```text

dynamic

Dictionary<string, object>

JObject

JsonNode

```



for representing exercises or lessons when a strongly typed model can represent the concept.



\---



\## 5. Don't over-engineer



KeyForge is intentionally not being designed as a large enterprise system.



We introduce abstractions when they solve a real problem.



We do not introduce patterns simply because they are popular.



\---



\# Current Status



The project currently has a working foundation for the lesson/content system.



Implemented:



\* .NET 10

\* Blazor Web App

\* YAML lesson schema

\* Strongly typed lesson models

\* Strongly typed exercise models

\* 8 exercise types

\* Lesson progression data

\* YAML parser

\* YAML validation

\* Lesson catalog

\* Configurable lesson content path

\* Data-driven lesson discovery

\* Lesson ordering

\* Duplicate lesson detection

\* Read-only lesson catalog

\* Automated tests

\* .NET SDK pinning using `global.json`



The architecture has also been reviewed and currently follows the intended dependency direction.



Current architectural assessment:



```text

Domain Separation       10/10

Infrastructure          9/10

Feature Organization    9/10

Testability             9/10

Extensibility           9/10

Configuration           9/10

Content Architecture    9/10

Dependency Direction    10/10

Maintainability         9/10

```



Overall architectural assessment:



```text

9.2 / 10

```



The project is ready to continue toward the progression and practice systems.



\---



\# Roadmap



The roadmap is intentionally incremental.



\## Phase 1 — Foundation



\* \[x] Create .NET 10 project

\* \[x] Create Blazor Web App

\* \[x] Define project structure

\* \[x] Define lesson YAML schema

\* \[x] Create domain models

\* \[x] Implement YAML parser

\* \[x] Implement lesson catalog

\* \[x] Add lesson progression rules

\* \[x] Add automated tests



\---



\## Phase 2 — Progress



\* \[ ] Define progress model

\* \[ ] Implement simple progress storage

\* \[ ] Implement lesson completion evaluation

\* \[ ] Implement lesson unlocking

\* \[ ] Track exercise results

\* \[ ] Track lesson results

\* \[ ] Add tests for progression engine



\---



\## Phase 3 — First Usable UI



\* \[ ] Replace template UI

\* \[ ] Create lesson catalog screen

\* \[ ] Show lesson status

\* \[ ] Show locked/unlocked lessons

\* \[ ] Create lesson details screen

\* \[ ] Create exercise screen

\* \[ ] Show exercise progress

\* \[ ] Show completion result

\* \[ ] Create basic practice experience



The goal of this phase is to reach the first genuinely usable version of KeyForge as quickly as possible.



\---



\## Phase 4 — Practice Engine



\* \[ ] Define practice session model

\* \[ ] Execute exercises

\* \[ ] Implement timing

\* \[ ] Implement tempo control

\* \[ ] Implement exercise evaluation

\* \[ ] Calculate scores

\* \[ ] Record results

\* \[ ] Connect results to lesson completion



\---



\## Phase 5 — Music Capabilities



\* \[ ] Rhythm exercises

\* \[ ] Note reading

\* \[ ] Interval training

\* \[ ] Octave recognition

\* \[ ] Ear training

\* \[ ] Finger independence

\* \[ ] Mental keyboard exercises

\* \[ ] Speed exercises



\---



\## Phase 6 — MIDI



\* \[ ] Define MIDI abstraction

\* \[ ] Detect MIDI devices

\* \[ ] Connect MIDI keyboard

\* \[ ] Receive note events

\* \[ ] Match played notes against exercises

\* \[ ] Evaluate timing

\* \[ ] Provide real-time feedback

\* \[ ] Integrate MIDI results with progress



\---



\# Long-Term Vision



KeyForge is intended to become more than a collection of music exercises.



The long-term vision is a personal music training environment capable of helping a musician progressively develop:



```text

Rhythm

&#x20;  +

Reading

&#x20;  +

Technique

&#x20;  +

Ear

&#x20;  +

Theory

&#x20;  +

Keyboard Visualization

&#x20;  +

Coordination

&#x20;  +

Speed

&#x20;  +

Musicality

```



The system should eventually be capable of adapting practice to the student's current level and weaknesses.



For example:



```text

Weak Rhythm

&#x20;    ↓

More Rhythm Exercises



Weak Interval Recognition

&#x20;    ↓

More Ear Training



Low Accuracy

&#x20;    ↓

Lower Tempo



Consistent Accuracy

&#x20;    ↓

Increase Tempo



Lesson Completed

&#x20;    ↓

Unlock Next Lesson

```



The ultimate goal is to create a system that supports deliberate practice rather than simply presenting a list of exercises.



\---



\# Contributing



KeyForge is currently primarily a personal learning and development project.



When adding new functionality:



1\. Keep the change small.

2\. Follow the existing architecture.

3\. Avoid hard-coding lesson content.

4\. Keep domain models infrastructure-free.

5\. Add tests for new behavior.

6\. Run the complete test suite.

7\. Make sure the application still builds.

8\. Prefer simple solutions over unnecessary abstractions.



\---



\# Building the Project



The repository pins the required .NET SDK through:



```text

global.json

```



Check the SDK:



```powershell

dotnet --version

```



Expected version:



```text

10.0.103

```



Restore:



```powershell

dotnet restore

```



Build:



```powershell

dotnet build

```



Run tests:



```powershell

dotnet test

```



Run the application:



```powershell

dotnet run --project src/KeyForge

```



\---



\# Project Status



KeyForge is currently under active development.



The core content architecture is implemented and tested.



The immediate focus is to build the \*\*Progress system and first usable practice UI\*\*, while keeping the architecture simple and the lesson content completely data-driven.



\---



\## License



This project currently does not define a public license.



```



\### GitHub Repository Description



برای قسمت \*\*Description\*\* خود GitHub بهتر است خیلی کوتاه باشد. من این را پیشنهاد می‌کنم:



> \*\*A data-driven music practice and learning platform built with .NET 10, Blazor, YAML, and MIDI support.\*\*



اگر بخواهی کمی شخصی‌تر و جذاب‌تر باشد:



> \*\*A data-driven music training platform for structured keyboard practice, ear training, rhythm, music reading, and MIDI — built with .NET 10 and Blazor.\*\*



من دومی را برای \*\*KeyForge\*\* بیشتر می‌پسندم، چون دقیق‌تر می‌گوید پروژه قرار است چه کاری انجام دهد.

```



