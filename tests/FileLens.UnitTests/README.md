# FileLens Application UnitTests

This `net10.0` project directly references only FileLens.Application and reuses the centrally
pinned Test SDK / MSTest packages. It has no Infrastructure, Bootstrap, WPF, real filesystem,
mocking framework, or production test seam dependency. The existing .gitkeep is retained.

An instance-based FakeFolderScanner records calls, exact paths, and cancellation tokens. Its
delegate supplies a result, synchronous exception, faulted task, or controlled completion.
Tests cover pure validation, cancellation-first ordering, token forwarding, Complete / Partial
result identity, unwrapped errors, controlled running cancellation, and transient Application DI.
No Sleep-based synchronization is used. Relative paths and surrounding / internal spaces are
forwarded unchanged. Scanner policies and OS behavior remain IntegrationTests responsibilities.

## Run from the repository root

```powershell
dotnet restore FileLens.slnx -v minimal
dotnet build FileLens.slnx -v minimal
dotnet test tests/FileLens.UnitTests/FileLens.UnitTests.csproj --no-build --no-restore --list-tests
dotnet test tests/FileLens.UnitTests/FileLens.UnitTests.csproj --no-build --no-restore --logger "trx;LogFileName=application.trx" --results-directory TestResults/UnitTests -v normal
```

These tests validate the Application execution path with a fake scanner. They do not establish
production scanning through the Use Case, UI wiring, active scan shutdown coordination,
filesystem reliability, or Sprint 2 completion.

## Validation record

On 2026-10-01, restore and solution build succeeded with 0 errors and 8 occurrences of the
existing NU1903 warning across restore / build and the four existing SQLite consumers. UnitTests
introduce no SQLite dependency. No package versions were changed or warnings suppressed.

List-tests reported 19 entries. The exception test's Type-valued data rows are expanded at
execution: that one listed entry runs 8 cases, yielding **26 executed, 26 passed, 0 failed,
0 skipped**. IntegrationTests regression: 26 discovered, 23 passed, 0 failed, 3 existing
environment skips. BootstrapTests regression: 7 discovered, 7 passed, 0 failed, 0 skipped.
The symbolic-link privilege / absent mapped-drive skips remain explicit scanner verification gaps.
