# FileLens

> Explainable AI-powered file management for Windows.

![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-10-purple)
![License](https://img.shields.io/badge/License-MIT-green)
![Status](https://img.shields.io/badge/Status-Sprint%201%20complete-success)

FileLens is an open-source Windows desktop application that helps users understand, organize, and manage their files through transparent AI recommendations.

Unlike traditional "one-click cleaners," FileLens never performs actions automatically.

**AI recommends. Users decide.**

---

# Philosophy

FileLens is built on three core principles.

- **AI recommends**
- **Users decide**
- **Nothing happens automatically**

Every recommendation is explainable.

Every file operation requires explicit user approval.

Users always remain in control.

---

# Features (Planned)

- Intelligent folder analysis
- Storage visualization
- Duplicate file detection
- Explainable AI recommendations
- Natural language file assistant (Version 3.0 roadmap; outside Version 1)
- Undo and operation history
- Privacy-first architecture
- Native Windows desktop experience

---

# Technology Stack

| Category | Technology |
|-----------|------------|
| Language | C# |
| Framework | .NET 10 |
| UI | WPF |
| Architecture | Clean Architecture |
| MVVM | CommunityToolkit.Mvvm |
| Logging | Serilog |
| Database | SQLite |
| Dependency Injection | Microsoft.Extensions.Hosting |

---

## Project Status

Current Progress

- Core documentation established
- Project architecture established
- Sprint 0 completed
- Sprint 1 scanning foundation completed
- Sprint 2 in progress: scanner policies and reliability implementation completed; Bootstrap/Host, runtime DI, and WPF startup / shutdown connected
- BootstrapTests implemented: 7 passed, 0 failed, 0 skipped
- IntegrationTests implemented and run: 23 passed, 0 failed, 3 environment-dependent skips; scanner verification gaps remain documented

The Application Scan Use Case, Application UnitTests, active scan shutdown coordination, and user-facing scan interaction remain pending. Logging is connected through the Host; SQLite remains configuration scaffolding without persistence. No AI contract or provider is implemented yet. See `SPRINT.md` and the test project READMEs for current verification limits.

---

# Repository Structure

```text
src/
    FileLens.Application
    FileLens.Bootstrap
    FileLens.Domain
    FileLens.Infrastructure
    FileLens.Shared
    FileLens.UI

docs/
    PRD
    ENGINEERING_SPEC
    AI_DESIGN
    ARCHITECTURE
    UI_UX
    Sprint/
        Sprint-00.md
        Sprint-01.md

tests/
    FileLens.BootstrapTests
    FileLens.IntegrationTests
```

---

# Development Philosophy

This project emphasizes:

- Maintainability over shortcuts
- Explainability over automation
- User safety over convenience
- Long-term architecture over rapid prototyping

---

# Roadmap

## Sprint 0

- Project foundation
- Clean Architecture
- MVVM
- Dependency Injection scaffolding
- Logging scaffolding
- SQLite package and configuration scaffolding
- Documentation

## Sprint 1

- Folder scanner
- File metadata
- Scan summary calculation

## Sprint 2

- Scanner behavior and policy definition
- Scanner reliability improvements and IntegrationTests
- Bootstrap/Host composition root
- Runtime Dependency Injection registration
- Scan use case and Application UnitTests
- Full integration verification

## Sprint 3

- Folder Picker and Scan / Cancel interaction
- Progress / Status and result summary
- Basic large-file sorting and filtering

Sprint 4 targets duplicate detection; Sprint 5 targets storage visualization and large-scale validation, with performance, virtualization, or streaming work justified by measurements. Sprint 6 targets the first AI recommendation feature with a minimal `IAIProvider` and one provider implementation. Sprint 7 targets SQLite history / persistence, followed by safe file operations, operation history, and undo / recovery in Sprint 8.

Future sprint numbers are provisional and require separate planning and implementation approval. See `ROADMAP.md` for milestones and `SPRINT.md` for the active scope. The SQLite `NU1903` warning is tracked as separate maintenance task MAINT-001 in `TASKS.md`; package updates are not part of this documentation alignment.

---

# Contributing

This project is currently under active personal development.

Contributions, ideas, bug reports, and architecture discussions will be welcomed after the core architecture reaches a stable state.

---

# License

MIT License
