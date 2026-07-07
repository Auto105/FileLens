# FileLens Sprint Board

> This document tracks the current active sprint.
>
> Unlike `ROADMAP.md`, which describes the long-term direction, and `TASKS.md`, which contains the overall backlog, this file represents the work currently in progress.
>
> Only one sprint should be active at a time.

---

# Sprint Information

| Item | Value |
|------|-------|
| Sprint | Sprint 2 |
| Status | Planned |
| Start Date | TBD |
| Target | Composition Root and Scan Use Case |

---

# Sprint Goal

Complete the runtime integration of the scanning pipeline introduced in Sprint 1.

Sprint 2 focuses on introducing a dedicated composition root, enabling Dependency Injection, and exposing the completed scanner through an Application use case.

Sprint 2 builds upon the completed Sprint 1 foundation without introducing new file analysis features.

---

# Scope

## Included

- Dedicated Bootstrap/Host composition root
- Runtime Dependency Injection registration
- Scan use case
- Basic scan validation

---

## Explicitly Out of Scope

The following features must **not** be implemented during this sprint:

- Duplicate detection
- AI recommendations
- File moving
- File deletion
- File renaming
- SQLite persistence changes
- UI redesign

---

# Sprint Tasks

## Planning

- [ ] Sprint 2 planning
- [ ] Architecture review
- [ ] Scope validation

---

## Bootstrap

- [ ] Design the Composition Root architecture
- [ ] Create the dedicated Bootstrap/Host project
- [ ] Configure Generic Host
- [ ] Compose Application registrations
- [ ] Compose Infrastructure registrations
- [ ] Register `IFolderScanner`
- [ ] Register `WindowsFolderScanner`

---

## Application

- [ ] Design the scan use case
- [ ] Implement the scan use case
- [ ] Connect the use case to `IFolderScanner`
- [ ] Validate scan request flow

---

## Testing

- [ ] Verify solution build
- [ ] Validate Dependency Injection
- [ ] Validate basic folder scanning

---

# Current Focus

**Next Task**

Design the Composition Root architecture.

Target Deliverable:

- Approved Bootstrap/Host project structure.
- Approved Dependency Injection strategy.
- Approved implementation order for Sprint 2.

This planning task must be completed before any runtime Dependency Injection is implemented.

---

# Task History

- Sprint 1 archived to `docs/Sprint/Sprint-01.md`.
- Sprint 2 template prepared.

---

# Blockers

- Runtime Dependency Injection remains intentionally deferred.
- A dedicated Bootstrap/Host composition root must be introduced before Infrastructure can be composed.
- The UI project must continue to avoid direct references to Infrastructure in accordance with the project's Clean Architecture rules.

---

# Notes

Sprint 2 starts from the completed Sprint 1 scanning pipeline.

The primary objective is integration rather than new scanning functionality.

Strict Clean Architecture remains in effect:

- The UI project must not reference Infrastructure directly.
- Runtime composition will be performed through a dedicated Bootstrap/Host project.

---

# Sprint Completion Criteria

Sprint 2 is complete when:

- [ ] A dedicated Bootstrap/Host project exists.
- [ ] Dependency Injection is fully configured.
- [ ] The scan use case is implemented.
- [ ] The completed scanning pipeline can be executed through the Application layer.
- [ ] The solution builds successfully.
- [ ] Strict Clean Architecture remains preserved.

---

Last Updated: 2026-07-07
