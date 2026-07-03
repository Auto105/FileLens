# FileLens Agent Guide

## Project Philosophy

AI assists.

Humans decide.

Transparency over automation.

Never surprise the user.

The application must remain useful without AI.

---

## Architecture

.NET 8

C#

WPF

MVVM

SQLite

Dependency Injection

OpenAI API

---

## Development Rules

- Never delete user files automatically.
- Always require user confirmation before modifying files.
- Keep AI logic separated from filesystem logic.
- Every Service must have an interface.
- Use async/await whenever appropriate.
- Avoid static global state.
- Public APIs require XML documentation.

---

## AI Rules

AI only:

- Analyze
- Explain
- Recommend

AI never:

- Delete automatically
- Move automatically
- Rename automatically

---

## Coding Principles

Readable over clever.

Maintainability over shortcuts.

Safety over convenience.

User trust is the highest priority.