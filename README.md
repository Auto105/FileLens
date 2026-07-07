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
- Natural language file assistant
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

- Documentation completed
- Project architecture established
- Sprint 0 completed
- Sprint 1 completed
- Sprint 2 planned

---

# Repository Structure

```text
src/
    FileLens.Application
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
- Dependency Injection
- Logging
- SQLite
- Documentation

## Sprint 1

- Folder scanner
- File metadata
- Scan summary calculation

## Sprint 2

- Bootstrap/Host composition root
- Runtime Dependency Injection registration
- Scan use case

## Sprint 3

- Duplicate detection
- Large file detection
- AI recommendation engine

---

# Contributing

This project is currently under active personal development.

Contributions, ideas, bug reports, and architecture discussions will be welcomed after the core architecture reaches a stable state.

---

# License

MIT License
