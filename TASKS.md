# Backlog

`SPRINT.md` owns the active sprint tasks and completion criteria. Future sprint numbers below are provisional and require separate planning and approval. Historical completion details remain in `docs/Sprint/`.

# Sprint 0

## Project Setup

- [x] Create solution
- [x] Configure MVVM
- [x] Configure DI
- [x] Configure SQLite
- [x] Configure Logging

These completed setup tasks refer to package, configuration, and registration scaffolding. They do not establish runtime composition or actual SQLite persistence.

---

# Sprint 1

## Folder Scanner

- [x] Recursive traversal and folder tree foundation
- [x] File metadata extraction
- [x] In-memory scan summary calculation

Folder selection, scan interaction, progress, and result presentation are deferred to Sprint 3. Scanner reliability, cancellation validation, and basic scanning validation move to Sprint 2. See `docs/Sprint/Sprint-01.md` for the preserved task record and follow-up dispositions.

---

# Sprint 2

## Composition Root and Scan Use Case

- [x] Define scanner behavior and policies
- [x] Improve scanner reliability according to approved policies
- [x] Add minimal `FileLens.IntegrationTests` and scanner fixtures
- [x] Implement dedicated Bootstrap/Host composition root and runtime DI
- [x] Implement Scan Use Case and input validation
- [x] Add `FileLens.UnitTests` for the Application path
- [ ] Complete full sprint integration verification

Scanner policies and the reliability, IntegrationTests, Bootstrap/Host, and Application Scan Use Case / UnitTests implementations were separately approved and completed. UnitTests executed 26 cases with all passed; BootstrapTests passed all 7 tests; scanner IntegrationTests ran with 23 passed, 0 failed, and 3 environment-dependent skips. WPF startup / shutdown integration passed earlier desktop smoke checks. UI scan execution is not connected. Scanner verification gaps, active scan shutdown coordination, and full sprint integration verification remain open and require their own approved scope. Detailed policy topics, test coverage, and completion criteria are in `SPRINT.md`.

---

# Sprint 3

## Scan Interaction and Basic Large-File Results

- [ ] Folder Picker
- [ ] Scan / Cancel interaction
- [ ] Progress / Status
- [ ] Result summary
- [ ] Basic large-file sorting / filtering
- [ ] Define the initial large-file threshold behavior

---

# Sprint 4

## Duplicate Detector

- [ ] Candidate selection before SHA-256 hashing
- [ ] Duplicate groups
- [ ] Duplicate result UI and validation

---

# Sprint 5

## Storage Visualization and Large-Scale Validation

- [ ] Storage visualization
- [ ] Validate 50,000+ files and representative directory shapes
- [ ] Measure memory, responsiveness, and cancellation latency
- [ ] Apply performance / virtualization / streaming changes if measurements justify them

---

# Sprint 6

## First AI Recommendation Feature

- [ ] Design and implement minimal `IAIProvider` for the first recommendation use case
- [ ] Implement one provider, selected during separate feature planning
- [ ] Validate explanations, responses, and opt-in metadata transmission

---

# Sprint 7

## SQLite History / Persistence

- [ ] Define scan history requirements and storage model
- [ ] Implement scoped repository interfaces and SQLite persistence
- [ ] Plan migrations and validate restart behavior

---

# Sprint 8

## Safe File Operations and Recovery

- [ ] Define confirmation and file revalidation behavior
- [ ] Implement operation history and interrupted-operation recovery
- [ ] Implement persistent undo and conflict handling
- [ ] Expose approved file operations only when the safety pipeline is ready

---

# Maintenance

## MAINT-001 — SQLite NU1903 Security Maintenance

**Status:** Planned; separate implementation approval required.

The existing restore assets report `NU1903` for `SQLitePCLRaw.lib.e_sqlite3 2.1.11`, transitively referenced by `Microsoft.Data.Sqlite 10.0.0` through `SQLitePCLRaw.bundle_e_sqlite3 2.1.11`.

Advisory: [GHSA-2m69-gcr7-jv3q](https://github.com/advisories/GHSA-2m69-gcr7-jv3q).

- [ ] Review a minimal compatible update within the .NET 10 package line
- [ ] Obtain approval for the exact package changes and validation scope
- [ ] Perform a fresh restore and inspect the resolved transitive dependencies
- [ ] Confirm NU1903 is resolved without warning suppression
- [ ] Build the entire solution and report all warnings / errors
- [ ] Verify basic SQLite behavior if required by the approved update

This maintenance task is separate from Sprint 2 feature work and SQLite persistence implementation. No packages are changed by the documentation alignment. Resolve the warning before actual SQLite activation or distribution of affected binaries.
