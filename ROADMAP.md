# Roadmap

Version milestones describe planned capabilities, not released or fully implemented features. `SPRINT.md` defines the active scope; future sprint numbers remain provisional and require separate planning and approval.

## Sprint Sequence

| Sprint | Planned Focus |
|--------|---------------|
| 2 | Scanner policy definition, reliability improvements, IntegrationTests, Bootstrap/Host + DI, Scan Use Case, Application UnitTests, full integration verification |
| 3 | Folder Picker, Scan / Cancel interaction, Progress / Status, result summary, basic large-file sorting / filtering |
| 4 | Duplicate detection, SHA-256 candidate hashing, duplicate groups and results |
| 5 | Storage visualization and large-scale validation; performance, virtualization, and streaming if measurements justify them |
| 6 | First AI recommendation feature, minimal `IAIProvider`, one provider implementation |
| 7 | SQLite scan history / persistence and migration planning |
| 8 | Safe file operations, operation history, persistent undo / recovery |

AI follows usable local analysis. SQLite remains the selected persistence technology, but actual persistence is deferred to its planned stage. File modifications must not be exposed before confirmation, operation history, and undo / recovery support are ready.

The existing SQLite `NU1903` warning is tracked as separate maintenance task MAINT-001 in `TASKS.md`, subject to separate approval. It does not introduce persistence functionality.

---

## v0.1

- Project setup
- Folder scanning
- Metadata collection
- Runtime scan integration and basic scan interaction / results (Sprints 2-3)

---

## v0.2

- Duplicate detection
- Large file detection (basic sorting / filtering starts in Sprint 3; duplicate detection targets Sprint 4)

---

## v0.3

- Storage visualization
- Large-scale validation and measurement-driven performance work (Sprint 5)

---

## v0.4

- AI recommendations
- Minimal provider abstraction and one provider implementation (Sprint 6)

---

## v0.5

- Undo
- History
- SQLite persistence (Sprint 7)
- Safe file operations, operation history, and persistent undo / recovery (Sprint 8)

---

## v1.0

First Public Release
