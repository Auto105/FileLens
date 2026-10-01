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
| Status | In Progress |
| Start Date | 2026-10-01 |
| Target | Composition Root and Scan Use Case |

---

# Sprint Goal

Complete the runtime integration of the scanning pipeline introduced in Sprint 1.

Sprint 2 focuses on introducing a dedicated composition root, enabling Dependency Injection, and exposing the Sprint 1 scanner foundation through an Application use case. Scanner policy definition, reliability improvements, and tests support this integration goal.

Sprint 2 builds upon the completed Sprint 1 foundation without introducing new file analysis features.

---

# Scope

## Included

- Dedicated Bootstrap/Host composition root
- Runtime Dependency Injection registration
- Scan use case
- Scanner behavior / policy definition
- Scanner reliability improvements according to subsequently approved policies
- Minimal scanner IntegrationTests and Application UnitTests
- Full sprint integration verification

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
- Folder Picker, user-facing Scan / Cancel interaction, progress, and result screens (planned for Sprint 3)
- AI contract / provider implementation
- Package security maintenance (tracked separately as MAINT-001 in `TASKS.md`)
- Full streaming replacement or speculative performance optimization

---

# Sprint Tasks

## Planning

- [x] Post-audit Sprint 2 planning alignment
- [x] Architecture audit and confirmation of existing decisions
- [x] Documentation change scope validation and approval

The completed planning items record the audit and approved documentation alignment. They do not approve scanner policy details, create projects, or authorize functional implementation. Every implementation task still requires its own plan, file list, and explicit approval under `AGENTS.md`.

---

## 1. Scanner Behavior / Policy Definition

- [x] Review current `IFolderScanner`, DTOs, and `WindowsFolderScanner` behavior
- [x] Define root path failure behavior, including missing paths, invalid input, and inaccessible roots
- [x] Define partial scan failure behavior and continuation boundaries
- [x] Define cancellation propagation, result behavior, and validation conditions
- [x] Define symbolic link / junction / reparse point handling, including cycles and root boundaries
- [x] Define relative path normalization behavior
- [x] Define protected / inaccessible directory handling and intentional exclusions
- [x] Define incomplete scan diagnostics, skipped-entry reporting, and diagnostic retention limits
- [x] Document result semantics, including root folder counting and logical file-size totals
- [x] Obtain approval for the policy and required contract changes before implementation

Delivered: the policy and acceptance-case matrix were reviewed and separately approved on 2026-10-01, followed by approval of the nine-file reliability implementation scope. See `docs/ENGINEERING_SPEC.md` for the approved policy. The earlier documentation alignment itself did not authorize these policies or functional implementation.

---

## 2. Scanner Reliability Improvements

- [x] Plan and obtain approval for the required scanner / result contract changes
- [x] Implement approved root validation and path normalization behavior
- [x] Handle recoverable failures during actual directory enumeration and metadata extraction
- [x] Implement approved partial-result diagnostics and exclusion reporting
- [x] Apply the approved link / protected-directory traversal policy
- [x] Preserve cancellation propagation and review checks during traversal and summary calculation

Retain `IFolderScanner` / `WindowsFolderScanner` and the current `Task.Run` approach. An immediate streaming rewrite is outside this sprint.

Implementation and solution build are complete. Minimal IntegrationTests now verify available cases, including deterministic cancellation / deletion and diagnostic truncation; complete environment-dependent validation is not established. Protected path boundaries, ACL failures, unavailable symbolic-link privileges / mapped drives, and cloud behavior remain verification gaps. Reparse / cloud exclusions are an explicit MVP support limitation. The internal 1,000-detail default remains adjustable; full counters continue beyond it.

---

## 3. Minimal IntegrationTests

- [x] Plan the `FileLens.IntegrationTests` project and obtain approval for its packages / references
- [x] Create the test project and isolated filesystem fixtures
- [x] Verify directory structure, file metadata, summary totals, and empty folders
- [x] Verify tested root failures (missing / file / invalid input), partial failures, and incomplete diagnostics against the approved policy
- [ ] Verify links / reparse points, path normalization, and protected / inaccessible directory behavior
- [x] Verify pre-cancelled and in-progress cancellation under documented test conditions
- [x] Record environment requirements or explicit skips for permission-dependent fixtures

Fixtures must use test-owned temporary roots and restrict cleanup to those roots. `samples/FolderTree` is not currently present and is not an existing validation prerequisite.

Initial suite validation on 2026-10-01: 26 discovered, 23 passed, 0 failed, 3 skipped from inconclusive outcomes. Symbolic file / directory links lacked creation privileges; no mapped network drive existed. Path normalization, Hidden/System inclusion, actual Windows-root rejection, junction cycles / outside targets, reparse root / ancestor rejection, ownership escape protection, and cleanup preserving the target passed. Both counters continued beyond the current internal 1,000-detail default. The combined link / protected / inaccessible task remains open because symbolic links, ACL denial, cloud behavior, and exact Windows / Windows.old boundary coverage are incomplete. OS-generated enumeration errors and cancellation latency remain unverified. See `tests/FileLens.IntegrationTests/README.md` for limitations and commands.

---

## 4. Bootstrap/Host + Dependency Injection

- [x] Design the Composition Root architecture
- [x] Obtain approval for the Bootstrap/Host project, references, and WPF startup ownership
- [x] Create the dedicated Bootstrap/Host project
- [x] Configure Generic Host
- [x] Compose Application registrations
- [x] Compose Infrastructure registrations
- [x] Register `IFolderScanner` with `WindowsFolderScanner`
- [x] Connect the WPF entry point, shell window, and ViewModel through the composition root
- [ ] Validate startup failure handling, shutdown, active scan cancellation, and resource disposal
- [x] Add appropriate DI integration checks without using UI components in scanner tests

Approved Bootstrap implementation completed on 2026-10-01. UI is a WPF library; Bootstrap owns
the STA App initialization, async Host startup, DI shell creation, and normal-close shutdown /
disposal. Default Host lifetime passed verification; no custom IHostLifetime was introduced.
BootstrapTests: 7 discovered, 7 passed, 0 failed, 0 skipped. Scanner regression: 26 discovered,
23 passed, 0 failed, 3 existing environment skips. Solution build: 0 errors, 8 existing NU1903
occurrences across restore / build and four consumers. Process-assisted desktop smoke checks
with visual captures passed from two working directories: one window, normal exit code 0,
no remaining process, and consistent user-local logs. Startup failure handling is implemented,
but failure-dialog injection and active scan cancellation are not verified by these checks.
The combined lifecycle / active scan task remains open; active scan shutdown coordination is
explicitly outside this approved implementation. See the BootstrapTests README for boundaries.

---

## 5. Scan Use Case

- [x] Design the scan use case
- [x] Obtain approval for its interface, input validation, and result behavior
- [x] Implement the scan use case
- [x] Connect the use case to `IFolderScanner`
- [x] Validate scan request flow

Approved on 2026-10-01 and implemented as transient `IScanFolderUseCase -> ScanFolderUseCase`.
The sole dependency is IFolderScanner. Cancellation-first pure validation rejects null / empty /
whitespace input without invoking the scanner. Original paths, tokens, results, and exceptions
are preserved. Request-flow validation uses the controlled Application fake; real scanning
through the production Use Case remains a separate full integration check. No UI wiring was added.

---

## 6. Application UnitTests

- [x] Plan the `FileLens.UnitTests` project and obtain approval for its packages / references
- [x] Create the test project with a controlled `IFolderScanner` test double
- [x] Verify Scan Use Case execution and input validation
- [x] Verify cancellation token forwarding and cancellation behavior
- [x] Verify result and failure propagation through the Application path without WPF or real filesystem access

UnitTests target net10.0 and directly reference only Application, reusing existing MSTest packages.
List-tests reports 19 entries; one exception test expands at execution into 8 data rows, yielding
26 executed cases: 26 passed, 0 failed, 0 skipped. All required path / token / result / failure /
cancellation cases passed without filesystem access, Sleep, or a production test seam. Regression:
IntegrationTests 26 discovered, 23 passed, 0 failed, 3 existing environment skips; BootstrapTests
7 discovered, 7 passed, 0 failed, 0 skipped. Restore and solution build succeeded with 0 errors
and 8 occurrences of the existing NU1903 warning across restore / build and four consumers.

---

## 7. Full Sprint Integration Verification

- [ ] Verify solution build
- [ ] Run Application UnitTests and scanner IntegrationTests
- [ ] Validate complete Dependency Injection composition
- [ ] Validate actual scanning through the Application use case
- [ ] Verify failure, incomplete-result, and cancellation behavior against approved acceptance cases
- [ ] Report all build warnings / errors, including the status of separate MAINT-001
- [ ] Review architecture boundaries and complete the Sprint Finish workflow

---

# Current Focus

**Next Task**

Plan full Sprint 2 integration verification and obtain separate approval.

Target Deliverable:

- A reviewable verification plan for production Host composition and actual scanning through the Application Use Case.
- Explicit acceptance cases, environment-dependent gaps, and lifecycle / cancellation verification boundaries.
- Separate approval for any additional fixtures or implementation changes needed by that verification.

Bootstrap/Host, Application Scan Use Case, and UnitTests are implemented and tested. UI scan execution is not connected. Full production Application-path integration, active scan shutdown coordination, and environment-dependent scanner gaps remain open; do not infer full pipeline validation from the separate test suites. Sprint 2 remains in progress.

---

# Task History

- Sprint 1 archived to `docs/Sprint/Sprint-01.md`.
- Sprint 2 template prepared.
- 2026-10-01: Completed the architecture audit and reconfirmed existing architecture / provider decisions.
- 2026-10-01: Completed the approved nine-document consistency alignment and Sprint 2 replanning; no functional code, packages, or projects changed.
- 2026-10-01: Recorded MAINT-001 for separate NU1903 maintenance approval and aligned provisional Sprints 3-8.
- 2026-10-01: Approved scanner policies and the nine-file reliability implementation scope, including explicit reparse / cloud and network support limitations.
- 2026-10-01: Implemented scanner validation, recoverable enumeration / metadata handling, cancellation checks, policy exclusions, and result diagnostics. Solution build succeeded with 0 errors and 2 existing NU1903 warning occurrences; automated scanner behavior tests remain pending.
- 2026-10-01: Approved and implemented Minimal IntegrationTests, three centrally pinned test packages, and a minimal internal entry checkpoint with friend-assembly access. Restore / solution build succeeded with 0 errors and 4 existing NU1903 warning occurrences. Discovered 26 tests: 23 passed, 0 failed, 3 inconclusive outcomes mapped to skipped; environment-dependent gaps remain documented. No unrelated scanner fixes or MAINT-001 changes were made.
- 2026-10-01: Approved and implemented FileLens.Bootstrap composition, UI library conversion, transient scanner / shell registration, and WPF / Host lifecycle ownership. Retained default Host lifetime after successful verification. BootstrapTests: 7 passed; scanner regression: 23 passed, 3 skipped, 0 failed. Restore and solution build succeeded with 0 errors and 8 existing NU1903 occurrences across four consumers. Two process-assisted desktop smoke checks with captured-window inspection verified normal startup / close, no process leak, and consistent local application-data logs. No Scan Use Case, active scan shutdown coordination, new package versions, scanner changes, or MAINT-001 work was included.
- 2026-10-01: Approved and implemented IScanFolderUseCase / ScanFolderUseCase and net10.0 Application-only UnitTests. List-tests reports 19 entries, expanding to 26 executed cases: 26 passed, 0 failed, 0 skipped. IntegrationTests regression: 23 passed, 3 existing environment skips; BootstrapTests regression: 7 passed; all suites had 0 failures. Restore / solution build succeeded with 0 errors and 8 existing NU1903 occurrences. Preserved cancellation-first validation, exact path / token forwarding, original result / exception identity, and Transient DI. No UI, Bootstrap, scanner, existing test, package-version, or active-scan shutdown changes were made; existing RunCodex.bat staged deletion and local untracked file were left untouched.

---

# Blockers

- Full production Application-path integration verification and active scan shutdown coordination remain pending separate approval.
- Scanner reliability has initial automated coverage; remaining environment-dependent validation is recorded in the IntegrationTests README.
- All three test projects exist; current suites do not establish complete environment-dependent scanner coverage or full production Application-path integration.
- Runtime composition and the Application Use Case are implemented; UI scan execution is not connected.
- The UI project must continue to avoid direct references to Infrastructure in accordance with the project's Clean Architecture rules.

---

# Notes

Sprint 2 starts from the completed Sprint 1 scanning foundation. Scanner policies, reliability code, runtime composition, and the Application Scan Use Case are implemented; all three test suites have run. Full production Application-path integration remains pending; available tests do not establish all environment-dependent behavior, active scan shutdown coordination, or large-scale performance.

The primary objective is integration rather than new scanning functionality.

Strict Clean Architecture remains in effect:

- The UI project must not reference Infrastructure directly.
- Runtime composition is performed through the dedicated FileLens.Bootstrap project.

The Sprint Goal remains Composition Root and Scan Use Case. The scope additions above reflect the explicitly approved post-audit replanning, not permission to implement them during the documentation task.

Future sprint numbers are provisional. User-facing scanning is planned for Sprint 3, duplicate detection for Sprint 4, visualization / large-scale validation for Sprint 5, the first AI implementation for Sprint 6, SQLite persistence for Sprint 7, and safe operations / history / undo for Sprint 8.

---

# Sprint Completion Criteria

Sprint 2 is complete when:

- [x] A dedicated Bootstrap/Host project exists.
- [x] Dependency Injection is fully configured.
- [x] The scan use case is implemented.
- [x] Scanner behavior / policies have been separately approved and implemented.
- [ ] Scanner IntegrationTests verify metadata, summaries, failures, link behavior, and cancellation.
- [x] Application UnitTests verify input validation, use case behavior, and cancellation forwarding.
- [ ] The scanning pipeline can be executed and validated through the Application layer.
- [x] Actual WPF / Host startup and shutdown are connected and verified.
- [x] The solution builds successfully.
- [ ] All build warnings are reported; any remaining NU1903 warning is explicitly tracked under MAINT-001.
- [ ] Strict Clean Architecture remains preserved.

---

Last Updated: 2026-10-01
