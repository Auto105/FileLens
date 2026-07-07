> This document is the primary engineering reference for both human developers and AI coding assistants. When implementation details conflict with other documents, this specification takes precedence over all engineering decisions except the Product Requirements Document (PRD).

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
| Last Updated | 2026-07-03 |

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

FileLens.UI

FileLens.Application

FileLens.Domain

FileLens.Infrastructure

FileLens.Shared

tests/

FileLens.UnitTests

FileLens.IntegrationTests

```

---

# Project References

The solution follows a strict one-way dependency rule.

| Project | References |
|----------|------------|
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
                 Application
                       │
                       ▼
                IAIProvider
                       │
      ┌────────────────┼────────────────┐
      │                │                │
      ▼                ▼                ▼
 OpenAI Provider  NVIDIA Provider  Gemini Provider
                       │
                       ▼
               Ollama Provider
```

### Design Principles

- Business logic must not depend on any AI vendor.
- AI providers must be replaceable without modifying the Application or Domain layers.
- Each provider must implement the same abstraction (`IAIProvider`).
- Provider-specific SDKs and APIs belong only in the Infrastructure layer.
- Adding a new AI provider must not require changes to existing business logic.

### Initial Scope

Sprint 0, Sprint 1, and Sprint 2 **do not** implement any AI provider.

Only the architectural direction and abstraction are defined at this stage.

Concrete provider implementations (such as OpenAI, NVIDIA NIM, Ollama, Anthropic, or Google Gemini) are planned for a future sprint after the file analysis pipeline is complete.

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

Scanning should remain responsive throughout the pipeline.

---

# SQLite Rules

No SQL inside UI.

Repositories own persistence.

Prepare for future migrations.

Database should remain replaceable.

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

Current implementation

IAIProvider

↓

OpenAIProvider

Future implementations

↓

OllamaProvider

↓

ClaudeProvider

↓

GeminiProvider

↓

AzureOpenAIProvider

The remainder of the application must never know which provider is active.

Providers should be replaceable through Dependency Injection.

---

# Configuration

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

Unit Tests

- Domain
- Recommendation Engine
- Duplicate Detection

Integration Tests

- SQLite
- File Operations
- AI Providers

UI Tests

Planned after Version 1.0.

Business logic should be testable without requiring UI components.

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

Planned

**Prerequisite**

Sprint 1 completed the folder scanning pipeline, including folder traversal, file enumeration, file metadata extraction, and scan summary calculation.

**Planned Deliverables**

- Dedicated Bootstrap/Host composition root
- Runtime DI registration for the scanning pipeline
- Scan use case
- Basic scan validation

**Constraint**

Strict Clean Architecture remains in effect.

The UI project must not reference Infrastructure directly, so runtime composition must occur in a dedicated Bootstrap/Host project.

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

Initial engineering specification.

0.2.0

Folder Scanner.

0.3.0

Duplicate Detection.

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
