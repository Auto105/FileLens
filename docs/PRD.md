# Product Requirements Document (PRD)

# FileLens

> A modern Windows file manager enhanced by explainable AI recommendations.

---

## Document Information

| Item | Value |
|------|-------|
| Project | FileLens |
| Version | 1.0 |
| Status | Initial Draft |
| License | MIT |
| Platform | Windows 10 / Windows 11 |
| Tech Stack | .NET 10, C#, WPF (MVVM), SQLite, OpenAI API |
| Project Type | Open Source |

---

## Document History

| Version | Date | Author | Notes |
|----------|------|--------|-------|
|0.1.0|2026-07-03|Auto|Initial PRD|

---

# Executive Summary

FileLens is a native Windows desktop application that helps users understand, organize, and maintain their files safely through intelligent local analysis and explainable AI recommendations.

Unlike traditional file managers that simply display files, FileLens analyzes folders, detects duplicates, identifies storage issues, summarizes folder contents, and provides AI-assisted recommendations while ensuring the user remains in complete control.

The application prioritizes:

- User safety
- Transparency
- Privacy
- Explainability
- Maintainability
- Long-term extensibility

---

# Product Vision

Create the most trustworthy AI-assisted file management application for Windows by combining fast local analysis with explainable AI recommendations while ensuring users always remain in control of their files.

---

# Design Philosophy

FileLens is **not** an AI-powered automation tool.

Instead, it is a professional file management application that uses AI to provide better insights and recommendations.

Core philosophy:

- AI assists.
- Humans decide.
- Data stays local whenever possible.
- Never surprise the user.
- Transparency over automation.
- Safety over convenience.
- Explain before acting.
- The application must remain useful even without AI.

---

# Goals

## Primary Goals

- Reduce folder clutter
- Help users understand storage usage
- Detect duplicate files
- Detect unusually large files
- Recommend organizational improvements
- Make file organization safer and easier

## Secondary Goals

- Modular architecture
- Future AI provider support
- Local LLM compatibility
- Open-source community contributions
- Full offline functionality for non-AI features

---

# Non-Goals (Version 1)

The following features are intentionally excluded from Version 1:

- AI chat assistant
- Background monitoring
- Scheduled scans
- Automatic deletion
- Automatic AI execution
- Cloud storage integration
- Multi-device synchronization
- File tagging
- Metadata editing
- Document content analysis

---

# Target Users

| Persona | Primary Need |
|----------|--------------|
| Students | Organize coursework and downloads |
| Developers | Clean project folders and archives |
| Office Workers | Manage documents and presentations |
| Gamers | Remove installers and duplicate media |
| General Windows Users | Keep storage organized |

Expected technical skill:

- Comfortable using Windows
- No programming knowledge required

---

# Problem Statement

Many Windows users accumulate thousands of files across Downloads, Desktop, and Documents.

Manual organization is:

- Time-consuming
- Repetitive
- Error-prone

Windows File Explorer provides navigation but lacks:

- Intelligent categorization
- Duplicate detection
- Storage insights
- AI-assisted recommendations

---

# Product Principles

## Safety First

No destructive action occurs automatically.

---

## AI Assists

AI analyzes.

AI explains.

AI recommends.

Users always make the final decision.

---

## User Control

Users remain in control of every file operation.

Every potentially destructive action requires explicit confirmation.

---

## Local First

Core functionality works completely offline.

AI is an optional enhancement.

---

## Explainability

Every AI recommendation must explain:

- Why it exists
- What evidence supports it
- Why the suggestion is useful

---

## Maintainability

The project favors:

- Modular architecture
- Interface-driven design
- Clean separation of responsibilities

---

# Success Metrics

| KPI | Target |
|------|--------|
| Successful folder scans | >99% |
| Duplicate detection accuracy | 100% (SHA-256) |
| Undo success rate | 100% |
| Scan cancellation | <1 second |
| Large folder support | 50,000+ files |
| Startup time | <3 seconds |
| Responsive UI | Always |
| User confirmation before file operations | 100% |

---

# User Stories

## Folder Analysis

As a user,

I want to scan a folder,

so that I can understand its contents.

---

## Duplicate Detection

As a user,

I want duplicate files identified,

so that I can recover storage space safely.

---

## Large File Detection

As a user,

I want unusually large files highlighted,

so I understand where storage is being consumed.

---

## AI Recommendations

As a user,

I want AI-generated explanations,

so that I understand what should be organized and why.

---

## Safe Organization

As a user,

I want confirmation before any file operation,

so nothing unexpected happens.

---

## Undo

As a user,

I want organization actions to be reversible,

so mistakes can be recovered.

---

# MVP Features

Version 1 includes:

- Folder selection
- Recursive folder scanning
- Metadata collection
- Duplicate detection (SHA-256)
- Large file detection
- Storage visualization
- AI recommendations
- Safe organization actions
- Undo support
- Scan history
- Offline mode

---

# Technology Stack

| Component | Technology |
|-----------|------------|
| Language | C# |
| Framework | .NET 10 |
| UI | WPF |
| Pattern | MVVM |
| Database | SQLite |
| Dependency Injection | Microsoft.Extensions.DependencyInjection |
| AI | OpenAI API |
| Security | Windows DPAPI |
| Logging | Serilog |

---

# Privacy Principles

Default uploads:

- File name
- Extension
- Size
- Timestamps

Never uploaded by default:

- File contents
- Binary data
- Images
- Documents

Content analysis requires explicit user consent.

---

# Security Principles

- Standard user permissions
- No Administrator rights by default
- Protected Windows folders excluded
- Secure API key storage (DPAPI)
- No automatic destructive actions

---

# Release Roadmap

## Version 1.0

- Folder scanning
- Duplicate detection
- Large file detection
- Storage visualization
- AI recommendations
- Undo
- Offline mode

## Version 2.0

- Local LLM support
- Backup mode
- Plugin architecture
- Advanced recommendation engine

## Version 3.0

- AI chat assistant
- Cloud storage
- Background monitoring
- Scheduled scans

---

# References

Detailed engineering implementation is documented separately:

- ENGINEERING_SPEC.md
- ARCHITECTURE.md
- AI_DESIGN.md
- DATABASE.md (To be added)
- UI_UX.md

---

# Project Status

Current Phase:

> Planning & Architecture

Next Milestone:

Sprint 0 – Project Initialization

---

## Guiding Principle

The primary goal of FileLens is not to automate file management.

Its purpose is to help users understand their files and make informed decisions through explainable AI recommendations while keeping users in complete control.

---

### AI Provider Abstraction

The AI subsystem must be provider-agnostic.

All AI integrations shall be accessed through an abstraction layer (IAIProvider), allowing support for multiple providers such as OpenAI, NVIDIA NIM, Anthropic, Google Gemini, Ollama, and future providers without affecting business logic.