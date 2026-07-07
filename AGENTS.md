# FileLens Agent Guide

## Project Philosophy

AI assists.

Humans decide.

Transparency over automation.

Never surprise the user.

The application must remain useful without AI.

---

## Architecture

.NET 10

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

---

# Documentation Priority

When documentation conflicts exist, follow this priority order:

1. DECISIONS.md
2. ENGINEERING_SPEC.md
3. PRD.md
4. SPRINT.md
5. README.md

If a conflict cannot be resolved from the documentation, stop implementation and report the conflict before making any changes.

---

# Sprint Workflow

For every implementation task:

1. Read all required project documentation.
2. Explain the implementation plan.
3. List every file that will be created or modified.
4. Wait for explicit approval.
5. Implement only the approved scope.
6. Build the solution.
7. Update `SPRINT.md`.
8. Report the implementation summary.
9. Stop.

Never skip the approval step.

---

# SPRINT.md Maintenance

After every completed implementation task:

- Mark completed tasks.
- Update **Current Focus**.
- Update **Task History**.
- Keep the Sprint Goal unchanged.
- Keep the Sprint Scope unchanged.

Do not modify future sprint planning.

---

# Sprint Finish Workflow

When completing a sprint:

1. Complete all approved sprint tasks.
2. Build the entire solution.
3. Report all build warnings.
4. Archive the completed sprint to `docs/Sprint/`.
5. Prepare the next active `SPRINT.md`.
6. Update repository documentation if required to reflect the completed sprint.
7. Report any remaining architectural inconsistencies.
8. Stop.

Do not begin implementation of the next sprint.

The next sprint always begins with a separate approval and planning phase.

---

# Scope Rules

Implement only the approved scope.

If additional improvements are discovered:

- Do not implement them.
- Report them separately after the requested work is complete.

Never expand the scope without approval.

---

# Architecture Conflicts

If the requested implementation conflicts with the documented architecture:

- Stop before making changes.
- Explain the conflict.
- Present valid implementation options.
- Wait for explicit approval.

Never resolve architectural conflicts by introducing undocumented exceptions.

---

# Commit Policy

Never create commits.

Never push to Git.

Never modify Git history.

Only prepare the repository for review.

---

# Build Policy

Every implementation task must end with:

- A successful solution build.
- A report of any warnings.
- A summary of created files.
- A summary of modified files.

---

# Reporting Format

Before implementation:

- Explain the plan.
- List files to be changed.
- Wait for approval.

After implementation:

- Build result
- Created files
- Modified files
- Summary of changes
- Outstanding issues (if any)

---

# Implementation Philosophy

Prefer incremental implementation.

Break large features into small, reviewable tasks whenever practical.

Each task should introduce one meaningful architectural change while preserving a working build.

---

# Forbidden Actions

Unless explicitly requested:

- Do not modify unrelated files.
- Do not rename files.
- Do not reorganize folders.
- Do not introduce new dependencies.
- Do not change architecture decisions.
- Do not change project documentation outside the requested scope.