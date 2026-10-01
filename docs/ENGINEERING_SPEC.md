> This document is the primary engineering reference for both human developers and AI coding assistants. Follow the documentation priority defined in `AGENTS.md`: `DECISIONS.md` > `ENGINEERING_SPEC.md` > `PRD.md` > `SPRINT.md` > `README.md`. Accepted architectural decisions take precedence over this specification. Supporting architecture, AI design, roadmap, and backlog documents must remain consistent with these references.

# Engineering Specification

# FileLens

> Developer Guide & Engineering Specification

---

# Document Information

| Item | Value |
|------|-------|
| Project | FileLens |
| Version | 0.1.0 |
| Status | Active |
| Last Updated | 2026-10-01 |

---

# Purpose

This document defines how FileLens should be engineered.

Unlike the PRD, which describes **what** the product should do, this document describes **how** it must be implemented.

This document is considered the primary engineering reference for developers, AI coding assistants (Codex, Cursor, Claude Code), and future contributors.

---

# Guiding Principles

## AI assists.

AI provides:

- Analysis
- Explanations
- Recommendations

AI never becomes the decision maker.

---

## Humans decide.

Every potentially destructive action requires explicit user confirmation.

Never surprise the user.

---

## Local First

Core functionality must work without internet access.

AI is an enhancement, not a dependency.

---

## Clean Architecture

The project must remain maintainable for years.

Favor:

- Composition
- Interfaces
- Dependency Injection
- Separation of Concerns

Avoid shortcuts.

---

# Architecture

FileLens follows a strict Clean Architecture.

```

                +----------------------+
                |   Presentation(UI)   |
                +----------+-----------+
                           |
                           v
                +----------------------+
                |    Application       |
                +----------+-----------+
                           |
                           v
                +----------------------+
                |       Domain         |
                +----------^-----------+
                           |
                +----------+-----------+
                |   Infrastructure     |
                +----------------------+

```
Infrastructure implements interfaces defined by the Domain or Application layers.

Dependency direction always points toward the Domain.

The Domain must never depend on Infrastructure.

---

# Solution Structure

```

src/

FileLens.Bootstrap

FileLens.UI

FileLens.Application

FileLens.Domain

FileLens.Infrastructure

FileLens.Shared

tests/

FileLens.BootstrapTests

FileLens.UnitTests

FileLens.IntegrationTests

```

The six `src/` projects and three test projects exist. `FileLens.UnitTests` targets net10.0 and directly references only Application. Bootstrap is the executable composition root; UI is a WPF library. See `ARCHITECTURE.md` for production references and runtime ownership. Scanner IntegrationTests reference Application and Infrastructure without WPF; BootstrapTests reference Bootstrap for production composition checks.

---

# Project References

The solution follows a strict one-way dependency rule.

| Project | References |
|----------|------------|
| FileLens.Bootstrap | FileLens.UI, FileLens.Application, FileLens.Infrastructure |
| FileLens.UI | FileLens.Application, FileLens.Shared |
| FileLens.Application | FileLens.Domain, FileLens.Shared |
| FileLens.Infrastructure | FileLens.Application, FileLens.Domain, FileLens.Shared |
| FileLens.Domain | None |
| FileLens.Shared | None |

Rules

- Circular project references are strictly prohibited.
- UI must never reference Infrastructure directly.
- Domain must never reference UI or Infrastructure.
- Infrastructure implements interfaces defined by Domain or Application.

---

# Project Responsibilities

## FileLens.Bootstrap

Owns executable startup, Generic Host composition, and WPF / Host lifecycle coordination.
It references UI and Infrastructure only as the top-level composition root, with no direct
Domain or Shared reference. It contains no use case, scan business logic, filesystem operations,
persistence, UI state workflow, or feature background service. UI retains its existing references.

## FileLens.UI

Responsibilities

- Views
- ViewModels
- Navigation
- Dialogs
- Themes
- User interaction

Must NOT

- Access SQLite directly
- Access OpenAI directly
- Perform filesystem operations

---

## FileLens.Application

Responsibilities

- Use Cases
- Commands
- Queries
- Service Interfaces
- DTOs

Coordinates the application.

Does not contain business rules.

---

## FileLens.Domain

Responsibilities

- Business Rules
- Entities
- Value Objects
- Interfaces
- Validation

Must contain no UI code.

Must contain no database code.

Must contain no OpenAI code.

---

## FileLens.Infrastructure

Responsibilities

- SQLite
- OpenAI
- DPAPI
- Logging
- Filesystem
- Repository implementations

Infrastructure is replaceable.

---

## FileLens.Shared

Contains

- Constants
- Common models
- Extensions

Avoid placing business logic here.

---

# Dependency Rules

| From | Can Reference |
|------|---------------|
| UI | Application, Shared |
| Application | Domain, Shared |
| Infrastructure | Application, Domain, Shared |
| Domain | Nothing |
| Shared | Nothing |


Forbidden

UI → Infrastructure

UI → SQLite

UI → OpenAI

Domain → Infrastructure

Domain → SQLite

Domain → WPF

---

## AI Provider Architecture

FileLens must remain independent of any specific AI provider.

All AI capabilities shall be accessed through an abstraction layer (`IAIProvider`), ensuring that business logic never depends on a vendor-specific SDK or API.

```text
 Application recommendation use case
                  |
                  v
       IAIProvider (planned contract)
                  |
                  v
   Selected Infrastructure provider
```

This is a planned runtime call flow, not a project dependency diagram. Provider implementations will implement the Application contract; no provider depends on another provider.

### Design Principles

- Business logic must not depend on any AI vendor.
- AI providers must be replaceable without modifying the Application or Domain layers.
- Each provider must implement the same abstraction (`IAIProvider`).
- Provider-specific SDKs and APIs belong only in the Infrastructure layer.
- Adding a new AI provider must not require changes to existing business logic.

### Initial Scope

Sprint 0, Sprint 1, and Sprint 2 **do not** implement any AI provider.

Only the architectural direction is documented at this stage. Neither the `IAIProvider` interface nor a concrete provider exists in the current source tree.

Provisional Sprint 6 targets the first AI recommendation feature, a minimal `IAIProvider`, and one provider implementation after usable local analysis and large-scale validation. Provider selection and the exact contract require separate planning and approval. Additional providers, such as OpenAI, NVIDIA NIM, Ollama, Anthropic, or Google Gemini, remain future extensions under ADR-0008; they are not all required for the first implementation.

---

# MVVM Rules

Every View has:

- View
- ViewModel

Never place business logic inside Views.

ViewModels communicate through services.

No code-behind except UI-specific behavior.

---

# Dependency Injection

Every service must have an interface.

Example

```

IFileScanner

↓

FileScanner

```

Never instantiate services manually.

Use constructor injection.

Avoid Service Locator.

---

# Service Lifetime Rules

Singleton

- SettingsService
- Logging
- AI Provider
- ThemeService

Scoped

(Not used in WPF)

Transient

- FileScanner
- RecommendationEngine
- DuplicateDetector

Rules

Choose the shortest valid lifetime.

Avoid Singleton unless state sharing is required.

The desktop App is singleton because WPF requires one Application per process. MainWindow and
MainWindowViewModel are transient and constructed together on STA through DI. Infrastructure
registers transient IFolderScanner / WindowsFolderScanner. No scoped desktop lifetime is introduced.

---

# Approved NuGet Packages

Current approved packages

- CommunityToolkit.Mvvm
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Logging
- Serilog
- Serilog.Extensions.Hosting
- Serilog.Sinks.File
- Microsoft.Data.Sqlite
- Microsoft.NET.Test.Sdk 18.10.1 (IntegrationTests, BootstrapTests, and UnitTests)
- MSTest.TestFramework 4.4.1 (IntegrationTests, BootstrapTests, and UnitTests)
- MSTest.TestAdapter 4.4.1 (IntegrationTests, BootstrapTests, and UnitTests)

Rules

Never introduce new packages without updating this document.

Favor Microsoft-supported libraries whenever possible.

---

# Folder Structure

Example

```

Application/

Commands/

Queries/

DTO/

Interfaces/

Services/

Validators/

Domain/

Entities/

ValueObjects/

Services/

Infrastructure/

Database/

Repositories/

AI/

Security/

Logging/

Filesystem/

UI/

Views/

ViewModels/

Controls/

Converters/

Themes/

Dialogs/

```

---

# Folder Conventions

General Rules

- One class per file.
- One responsibility per class.
- One ViewModel per View.
- One interface per implementation.
- Keep folder depth as shallow as practical.

Application Layer

Commands/

Queries/

DTO/

Interfaces/

Services/

Validators/

Domain Layer

Entities/

ValueObjects/

Enums/

Events/

Infrastructure Layer

AI/

Database/

Repositories/

Filesystem/

Logging/

Security/

Presentation Layer

Views/

ViewModels/

Controls/

Converters/

Dialogs/

Themes/

---

# Naming Convention

Interfaces

```

IFileScanner

IAIProvider

IHistoryRepository

```

Services

```

FileScanner

OpenAIProvider

HistoryRepository

```

ViewModels

```

DashboardViewModel

ScanViewModel

SettingsViewModel

```

Views

```

DashboardView

ScanView

```

---

# Namespace Convention

Namespaces should reflect the folder structure.

Examples

FileLens.UI.Views

FileLens.UI.ViewModels

FileLens.Application.Services

FileLens.Application.Interfaces

FileLens.Domain.Entities

FileLens.Infrastructure.Repositories

Rules

- One namespace per folder.
- Keep namespace hierarchy shallow.
- Avoid abbreviations.

---

# Coding Standards

Prefer

Small classes.

Single Responsibility.

Readable code.

Explicit naming.

Avoid

Magic strings.

Deep inheritance.

Large methods.

Hidden side effects.

---

# C# Language Rules

Language Version

C# 14.0

Analysis Level

10.0

Nullable Reference Types

Enabled

Implicit Usings

Enabled

File-scoped namespaces

Required

One public type per file

Required

XML Documentation

Required for all public APIs.

Prefer

- records for DTOs
- init properties
- expression-bodied members when readable

Avoid

- regions
- unnecessary partial classes
- static mutable state

---

# Async Rules

Filesystem operations

↓

Always async.

Database

↓

Always async.

AI requests

↓

Always async.

Long-running operations must support

CancellationToken.

Never block the UI thread.

---

# Thread Safety

UI

Dispatcher thread only.

Filesystem

Background thread.

SQLite

Background thread.

AI

Background thread.

Never block the UI thread.

---

# File System Rules

Never delete automatically.

Never rename automatically.

Never overwrite automatically.

Allowed actions

- Scan
- Analyze
- Move
- Archive

All actions require confirmation.

---

# File Operation Pipeline

Every file modification follows the same workflow.

User

↓

Confirmation Dialog

↓

FileOperationService

↓

UndoManager

↓

Repository

↓

Logger

↓

Refresh UI

No operation may bypass this pipeline.

---

# Undo Rules

Every move operation must generate undo information.

Undo data survives application restart.

Undo must never modify unrelated files.

---

# AI Rules

AI never accesses the filesystem directly.

AI never performs file operations.

AI only receives metadata.

AI only returns:

- Observations
- Recommendations
- Explanations

Every recommendation must include reasoning.

---

# Prompt Rules

Prompts must never be hardcoded inside ViewModels.

All prompts should be centralized.

Future providers should reuse the same prompt format.

---

# Privacy Rules

Default upload

- Filename
- Extension
- Size
- Timestamps

Never upload

- File contents
- Images
- PDFs
- Binary data

Content analysis requires explicit consent.

---

# Logging Rules

Log

- Scan start
- Scan finish
- Errors
- AI requests
- AI responses
- User actions

Never log

- API Keys
- File contents
- Sensitive user data

---

# Logging Levels

Debug

Development diagnostics.

Information

Normal application flow.

Warning

Recoverable issues.

Error

Operation failed.

Fatal

Application cannot continue.

Logs should always contain enough information for troubleshooting without exposing sensitive user data.

---

# Error Handling

Errors must never crash the application.

If one file fails

Continue scanning.

Report the failure.

Every exception should contain actionable information.

---

# Exception Strategy

Recoverable Exceptions

- Log
- Continue
- Notify User

Fatal Exceptions

- Log
- Display Error Dialog
- Shutdown gracefully

The application should never terminate because a single file operation failed.

---

# Performance Rules

Target

50,000+ files.

Prefer streaming.

Avoid loading all metadata into memory.

Hash only candidate duplicate groups.

Never hash unnecessarily.

---

# Scan Pipeline

Folder

↓

Recursive Scanner

↓

Metadata Extraction

↓

Duplicate Detection

↓

Large File Detection

↓

Recommendation Engine

↓

AI Provider (optional)

↓

ViewModel

↓

UI

This diagram describes planned analysis stages, not the current implementation or mandatory feature delivery order. Basic large-file sorting / filtering targets Sprint 3, duplicate detection Sprint 4, visualization / large-scale validation Sprint 5, and the first AI recommendation Sprint 6. `ROADMAP.md` owns the provisional milestone mapping.

Scanning should remain responsive throughout the pipeline.

## Current Scanner State and Sprint 2 Planning

Sprint 1 implemented `IFolderScanner`, the `FileNode` / `FolderNode` / `ScanResult` DTOs, and `WindowsFolderScanner` traversal, metadata extraction, and in-memory summary calculation. The scanner currently builds the complete tree on a `Task.Run` worker and then calculates totals. These are implementation foundations, not evidence that reliability or the 50,000+ file target has been validated.

Sprint 2 reliability implementation now adds root validation, invocation-time path normalization, recoverable-error handling during actual enumeration and metadata reads, policy exclusions, completion status, and bounded diagnostic details. Minimal scanner IntegrationTests, Bootstrap runtime DI composition, and the Application Scan Use Case with UnitTests exist. Actual production scanning through the Use Case remains pending full integration verification. Build success alone does not validate scanner reliability; environment-dependent gaps are recorded in the IntegrationTests README.

Scanner policies, reliability implementation, the initial IntegrationTests scope, Bootstrap / Host implementation, and the Application Scan Use Case / UnitTests were separately approved on 2026-10-01. The approved scanner behavior is recorded below; full integration verification and active scan shutdown coordination remain separate tasks.

Retain `IFolderScanner` / `WindowsFolderScanner` and the existing `Task.Run` approach while addressing approved reliability work. The streaming / memory performance goals above remain goals, not current guarantees. Provisional Sprint 5 will measure large-scale behavior and justify any performance, virtualization, or streaming changes.

### Approved Sprint 2 Scanner Policy

- Null, empty, or whitespace-only input is rejected. A file is not a valid scan root. Root absence, access failure, I/O failure, or detected disappearance faults the task without returning a result.
- Ordinary relative paths use the base directory captured when `ScanAsync` is invoked. Infrastructure normalizes DTO paths to absolute paths, preserving path casing and drive roots. Ambiguous drive-relative / root-relative inputs and explicit device paths are unsupported.
- UNC paths and mapped network drives are outside the Sprint 2 MVP scope. Mapped drives are identified using actual `DriveInfo.DriveType`, not a drive-letter pattern. Unsupported roots throw `NotSupportedException`; diagnostic contracts use the general `UnsupportedPath` category instead of format-specific categories.
- Root and ancestor directories are validated before traversal and again before returning the result. This detects observed loss or unsupported changes but does not provide an atomic snapshot or eliminate filesystem races.
- Recoverable descendant access / not-found / I/O failures are recorded and traversal continues where possible. A folder that fails before yielding entries is omitted; a folder whose enumeration fails after yielding entries retains collected data. Failed mandatory file metadata causes that file to be omitted, not represented with zero size.
- Cancellation propagates as `OperationCanceledException` without a partial result. Checks surround traversal, metadata work, summary loops, and final return. Synchronous OS calls are not forcibly interruptible; the PRD cancellation target needs measured validation.
- All file and directory reparse points are intentionally excluded in the MVP, including symbolic links and junctions. Roots with excluded ancestors are unsupported. This is an explicit support limitation: OneDrive and other cloud-backed entries, even locally downloaded ones, may be excluded. Entries flagged for remote recall on data access are also excluded as `UnsupportedPath`. Metadata-only scanning does not guarantee the absence of provider-triggered network activity.
- Protected exclusions cover the actual Windows installation directory and its descendants, plus the scanned volume's `System Volume Information` and `$Recycle.Bin` trees. Whole ancestor paths are compared; `Windows.old` is not a child of `Windows`. Program Files / ProgramData and Hidden / System attributes alone do not cause blanket exclusion. Ordinary access denial remains a failure, and no automatic elevation is attempted.
- `ScanResult` retains tree and summary, adding `CompletionStatus`, `FailureCount`, `ExcludedEntryCount`, and `Diagnostics`. Status is `Partial` when observed recoverable failures exist, otherwise `Complete`, even with policy exclusions. Complete means completion within supported scope, not inclusion of every physical entry.
- `TotalFolderCount` includes the returned root. Only retained nodes contribute to summary. `TotalSize` is retained files' logical length sum, not allocated or reclaimable space. Hard links are counted by path; count / size overflow is not silently accepted.
- Failure counts represent observed events; excluded-entry counts represent observed entries, not their unknown descendants. Each event contributes one diagnostic before retention limits. Details contain absolute path, failure / exclusion kind, category, optional exception type, and HRESULT; no raw exception object, message, or stack trace is retained. User-facing messages should be derived from categories, and sensitive paths must not be logged or uploaded implicitly.
- Diagnostic retention uses an Infrastructure-internal MVP default of 1,000 details per scan. This is adjustable after large-scale measurement, not a public or permanent product limit. Full failure / exclusion counters continue after detail retention stops. Truncation is detectable when detail count is less than the sum of the two counters. Scan state is isolated per invocation.
- History identifiers / timestamps, streaming, progress reporting, and a new public filesystem abstraction are not added by this reliability task. Deep recursion, large-scale memory use, permission-dependent behavior, network-drive detection, and cloud behavior remain validation items.

---

# SQLite Rules

No SQL inside UI.

Repositories own persistence.

Prepare for future migrations.

Database should remain replaceable.

SQLite remains the selected technology. Current code provides package / configuration scaffolding only; actual history / persistence targets provisional Sprint 7. Operation history and persistent undo / recovery must be ready before exposing the safe file operations planned for Sprint 8. No persistence feature is added in Sprint 2.

The existing SQLite dependency warning is tracked separately as MAINT-001 in `../TASKS.md`. A minimal .NET 10 package update, fresh restore, warning resolution, full build, and any necessary SQLite smoke validation require separate maintenance approval. Warning suppression is not a resolution, and no package update is part of the documentation alignment.

---

# Repository Rules

Every database operation must go through a Repository.

Repositories expose interfaces only.

Never execute SQL outside Infrastructure.

Repositories must never contain business logic.

---

# OpenAI Integration

Always communicate through

IAIProvider.

Never call OpenAI directly from UI.

Future providers

- Ollama
- Azure OpenAI
- Claude
- Gemini

must require minimal changes.

---

# AI Provider Strategy

The application communicates only through IAIProvider.

Current status: design only. No `IAIProvider` interface or provider implementation exists yet.

First implementation target: provisional Sprint 6, with a minimal Application contract and one Infrastructure provider selected during feature planning.

Future provider examples include OpenAI, NVIDIA NIM, Ollama, Anthropic, Gemini, and Azure OpenAI. These are alternative implementations of the same abstraction, not an implementation chain or a commitment to deliver them all in the first AI sprint.

The remainder of the application must never know which provider is active.

Providers should be replaceable through Dependency Injection.

---

# Configuration

Current Bootstrap composes the existing options callbacks; it does not implement the future
SettingsService. Host content root is the executable directory. `FileLens:Logging:LogDirectory`
can override the existing logging directory through Host configuration. Relative values are
resolved under the user's local FileLens application-data directory; absolute values are retained.
SQLite options remain scaffolding and no database is created by Bootstrap.

Configuration belongs in

SettingsService.

Never hardcode

- API Keys
- Paths
- Thresholds

---

# Settings Management

Settings should be grouped into:

- General
- AI
- Scanning
- Performance
- Appearance

Settings must support future migration without breaking existing user configurations.

---

# Definition of Done

A feature is complete when

- Code builds successfully
- Unit tests pass
- No architecture rules are violated
- Documentation updated
- Logging added where appropriate
- Errors handled
- UI remains responsive
- Feature reviewed

---

# AI Coding Rules

When generating code:

Always

- Respect Clean Architecture
- Respect MVVM
- Use constructor injection
- Prefer interfaces
- Keep methods small
- Write maintainable code

Never

- Bypass architecture
- Add unnecessary NuGet packages
- Access Infrastructure from UI
- Mix UI and business logic
- Introduce breaking architectural changes

If unsure,

prefer consistency over cleverness.

---

# Code Review Checklist

Before completing any feature, verify:

- Architecture rules are respected.
- No forbidden dependencies exist.
- UI remains responsive.
- Async methods support cancellation.
- Logging is appropriate.
- Exceptions are handled.
- Public APIs are documented.

---

# Testing Strategy

Application Scan Use Case and UnitTests were implemented under separate approval.
`IScanFolderUseCase.ExecuteAsync(string folderPath, CancellationToken cancellationToken = default)`
returns the existing ScanResult. Sealed ScanFolderUseCase has only an IFolderScanner dependency
and is registered as Transient by AddApplicationServices. Cancellation precedes null / empty /
whitespace validation. The original path and token are forwarded without trimming or normalization.
Filesystem validation remains the scanner's responsibility. Complete / Partial results and
exceptions propagate unchanged; the async contract reports validation errors through its Task.
There is no request DTO, result wrapper, Task.Run, private token source, retry, or timeout.
The Use Case does not override a scanner result with a post-completion cancellation check.

UnitTests use a small instance fake, controlled task completion, and no Sleep synchronization.
They verify exact / relative / space-containing paths, token forwarding, result identity,
input rejection without scanner invocation, cancellation-first behavior, running cancellation,
unwrapped synchronous / faulted-task errors, constructor validation, and Application-only DI.
Production UI wiring and actual filesystem scanning through the Use Case are not established
by these tests. No scanner seam or existing IntegrationTests / BootstrapTests was changed.

Use Case validation on 2026-10-01: UnitTests list-tests reported 19 entries; the exception test
expanded during execution into 8 data rows, giving 26 executed cases with 26 passed, 0 failed,
0 skipped. Scanner regression discovered 26 tests: 23 passed, 0 failed, 3 existing environment
skips (symbolic-link privileges and absent mapped network drive). Bootstrap regression discovered
7 tests: 7 passed, 0 failed, 0 skipped. Restore / solution build succeeded with 0 errors and
8 occurrences of the existing NU1903 warning across restore / build and four consumers; UnitTests
add no SQLite dependency. MAINT-001 remains unchanged. Full sprint integration is not complete.

Bootstrap runtime validation is separate from scanner fixtures. `FileLens.BootstrapTests` uses
the existing central MSTest versions, validated production Host construction, transient scanner
and ViewModel resolution, Host Start / Stop, log flush / file-handle release, static logger
preservation, and relative / absolute logging options. One isolated STA check constructs shell
windows and verifies compiled XAML / DataContext, without creating Application or showing windows.
No Application.Run lifecycle or UI automation is added to the test runner.

Validation on 2026-10-01: BootstrapTests discovered 7 tests, with 7 passed, 0 failed, 0 skipped.
Scanner regression discovery found 26 tests: 23 passed, 0 failed, 3 skipped from the existing
symbolic-link privilege and absent mapped-drive conditions. The full solution build succeeded
with 0 errors and 8 occurrences of the existing NU1903 warning: restore and build each report
Infrastructure, IntegrationTests, Bootstrap, and BootstrapTests. This is the existing MAINT-001
dependency warning propagated to the two new consumers, not a new package or suppressed warning.

Actual executable desktop smoke validation used process-assisted launching / normal window
close and captured windows for visual inspection. From both the repository root and an ignored
temporary directory, exactly one FileLens window appeared, no startup error was shown, normal
close returned exit code 0, and no launched process remained. Both runs used
`%LOCALAPPDATA%/FileLens/logs`, with Host startup / shutdown entries. This is separate desktop
verification, not test-runner UI automation or a claim of comprehensive human interaction testing.

Unit Tests

- Scan Use Case and Application input validation
- Cancellation forwarding, result propagation, and Application behavior
- Domain
- Recommendation Engine
- Duplicate Detection

Integration Tests

- Scanner behavior, file metadata, summaries, and failure diagnostics
- Approved link / reparse point, path normalization, and protected / inaccessible directory policies
- Cancellation under documented test conditions
- Dependency Injection integration where appropriate
- SQLite
- File Operations
- AI Providers

UI Tests

Planned after Version 1.0.

Business logic should be testable without requiring UI components.

Sprint 2 introduced `FileLens.IntegrationTests` targeting `net10.0-windows`, referencing Application and Infrastructure, with the three approved test packages pinned through Central Package Management. `FileLens.UnitTests` reuses those packages, targets net10.0, and directly references only Application; its existing .gitkeep is retained. No mocking, coverage, or helper framework is directly added. Test-only XML documentation generation is disabled; production documentation rules are unchanged.

IntegrationTests use public scanner results / exceptions. A separately approved minimal internal per-instance entry-observed checkpoint synchronizes deletion and running cancellation. The public parameterless constructor leaves it unset; production DI and `IFolderScanner` are unchanged. `Properties/AssemblyInfo.cs` adds only `InternalsVisibleTo("FileLens.IntegrationTests")`; it does not duplicate SDK-generated assembly attributes or disable generation.

Fixtures own GUID containers under the repository's already-ignored `temp/` directory, validate ownership and path boundaries, and remove reparse entries without following their targets. Cleanup failures are reported. Tests do not change the process current directory, user/system ACLs, network mappings, or cloud settings. Environment capability failures use explicit inconclusive reasons. See `../tests/FileLens.IntegrationTests/README.md` for commands and remaining coverage.

Validation on 2026-10-01: restore and solution build succeeded (0 errors, 4 occurrences of the existing NU1903 warning across Infrastructure and its test consumer). Discovery found 26 tests; 23 passed, 0 failed, and 3 were skipped from inconclusive outcomes (file / directory symbolic-link privileges unavailable, no mapped network drive). Junction exclusion, cycle / outside-root behavior and safe cleanup, both 1,000-detail counter regressions, and deterministic deletion / cancellation passed. ACL denial, OS-generated enumeration errors, cloud entries, and the real Windows / Windows.old boundary remain unverified. These results do not establish complete scanner reliability or performance.

Filesystem fixtures must use isolated test-owned temporary roots with cleanup restricted to those roots. Permission-dependent tests must document environment requirements or explicit skip conditions. `samples/FolderTree` is absent from the current repository and must not be described as existing validation coverage. Large-scale performance verification targets provisional Sprint 5.

---

# Future Sections

The following sections will be expanded during development.

- Database Design
- Repository Pattern
- AI Prompt Specification
- Recommendation Engine
- Undo Engine
- File Organization Engine
- Plugin System
- Testing Strategy
- Release Pipeline

---

# Current Sprint

Sprint 2

## Sprint 2 – Composition Root and Scan Use Case

**Status**

In Progress

**Prerequisite**

Sprint 1 completed the scanning foundation: folder traversal, file enumeration, file metadata extraction, and in-memory scan summary calculation. Sprint 2 scanner policies, reliability code, Bootstrap runtime composition, and the Application Scan Use Case / UnitTests are implemented. Full production Application-path integration, active scan shutdown coordination, and environment-dependent scanner gaps require further verification. The UI is not connected to scan execution.

**Planned Deliverables**

1. Scanner behavior / policy definition and separate approval
2. Scanner reliability improvements according to the approved policies
3. Minimal IntegrationTests and filesystem fixtures
4. Dedicated Bootstrap/Host composition root and runtime DI registration
5. Scan Use Case and input validation
6. UnitTests for the Application path
7. Full sprint integration verification

**Constraint**

Strict Clean Architecture remains in effect.

The UI project must not reference Infrastructure directly, so runtime composition must occur in a dedicated Bootstrap/Host project.

The Sprint Goal remains Composition Root and Scan Use Case. Duplicate detection, AI contract / provider implementation, file modifications, actual SQLite persistence, user-facing scan screens, and a full streaming replacement are outside Sprint 2. MAINT-001 requires separate maintenance approval and is not a Sprint 2 feature. `SPRINT.md` defines the detailed task board and completion criteria; the scanner policy above was finalized through separate approval after the documentation alignment.

---

# Versioning

This project follows Semantic Versioning.

Major

Breaking architectural changes.

Minor

New features.

Patch

Bug fixes and documentation improvements.

Examples

0.1.0

Project foundation, folder scanning, and basic scan integration / interaction.

0.2.0

Duplicate Detection and large-file analysis.

0.3.0

Storage visualization and large-scale validation.

These examples follow the planned version milestones in `../ROADMAP.md`; they do not declare a released version or implemented capability.

---

# Engineering Principles

This specification exists to ensure long-term maintainability.

When multiple implementation approaches are possible:

1. Prefer readability over cleverness.
2. Prefer maintainability over optimization.
3. Prefer explicit behavior over implicit behavior.
4. Prefer composition over inheritance.
5. Prefer interfaces over concrete implementations.
6. Prefer user safety over convenience.

Every architectural decision should support these principles.
