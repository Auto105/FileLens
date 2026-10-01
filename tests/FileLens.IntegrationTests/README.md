# FileLens Scanner IntegrationTests

This Windows-only `net10.0-windows` project validates `IFolderScanner.ScanAsync` against isolated filesystem fixtures. It references Application and Infrastructure without WPF, Bootstrap/Host, or runtime DI verification.

## Packages

Central Package Management pins these stable packages:

- [Microsoft.NET.Test.Sdk 18.10.1](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/18.10.1)
- [MSTest.TestFramework 4.4.1](https://www.nuget.org/packages/MSTest.TestFramework/4.4.1)
- [MSTest.TestAdapter 4.4.1](https://www.nuget.org/packages/MSTest.TestAdapter/4.4.1)

The official NuGet framework tables include computed .NET 10 compatibility. Restore, compilation, discovery, and execution provide repository-specific validation. Framework and adapter versions are kept aligned. No additional mocking, coverage, or helper framework is referenced directly.

## Run from the repository root

```powershell
dotnet restore FileLens.slnx -v minimal
dotnet build FileLens.slnx -v minimal
dotnet test tests/FileLens.IntegrationTests/FileLens.IntegrationTests.csproj --no-build --no-restore --list-tests
dotnet test tests/FileLens.IntegrationTests/FileLens.IntegrationTests.csproj --no-build --no-restore --logger "trx;LogFileName=scanner.trx" --results-directory TestResults/IntegrationTests -v normal
```

`TestResults/` and `temp/` are already ignored. The repository location is discovered from the test output directory; fixtures refuse to run outside the repository or through reparse-point ancestors within it. Tests must run on a supported local Windows filesystem. No elevation or network mapping is requested.

## Fixture ownership and cleanup

Each fixture owns `temp/FileLens.IntegrationTests/<guid>/`, with its ownership marker outside the scanned `scan/` directory. Files and folders are created with explicit names and sizes. Relative-path tests do not change the process current directory. Metadata expectations use values read back from the filesystem after setting timestamps.

Cleanup checks the exact owned path, GUID marker, ancestor attributes, and descendant boundaries. It visits normal directories explicitly and removes reparse entries themselves without recursively following them. Outside-root link targets are also test-owned. Cleanup errors propagate rather than being ignored; ambiguous ownership prevents deletion. Empty fixture-base directories may remain, but individual owned containers must be removed. Cleanup assumes test-owned fixtures are not being replaced by an unrelated concurrent process; it is not an adversarial filesystem sandbox.

## Deterministic checkpoint

The normal public scanner constructor leaves the checkpoint unset. An internal constructor accessible only to this friend test assembly accepts an instance callback after an entry is observed and before processing. This synchronizes deletion and cancellation without exposing a public filesystem abstraction or changing production DI.

The running-cancellation test waits for a checkpoint signal, cancels the token, releases the worker, and awaits termination before cleanup. Timeouts only bound stalled tests; arbitrary sleep and large timing-dependent trees are not used. Callback failures are test failures, not swallowed scanner diagnostics.

## Coverage and environment conditions

- Normal tree, empty root/file, file metadata, root-inclusive counts, logical size, normalized absolute DTO paths, relative input, trailing separator, input/root failure, and Hidden/System inclusion.
- Observed file deletion and root disappearance, Partial/Complete status, failure/exclusion categories, exact counters, and independent concurrent scan state.
- Current internal 1,000-detail MVP default: 1,005 observed failures and 1,005 junction exclusions verify that full counters continue. This is an implementation regression test, not a permanent public limit or a performance benchmark.
- Pre-canceled and checkpoint-synchronized running cancellation.
- Symbolic links, junction cycles, outside-root targets, reparse roots/ancestors, and cleanup preserving an outside target.
- Existing mapped network drives, if present, are rejected without creating or removing mappings.
- The actual Windows installation directory is tested only for root rejection; no files are created or modified inside it.

Unavailable symbolic-link/junction fixture capability is reported with `Assert.Inconclusive` and a reason, not as a passed test. The default MSTest adapter maps inconclusive results to skipped runner results. Once a fixture is established, incorrect scanner behavior is a failure. Test execution is explicitly nonparallel at assembly level; the independent-state test deliberately starts two scans itself.

## Remaining verification

| Case | Status / strategy |
|------|-------------------|
| Real ACL access denial | Not automated in this initial suite; requires an owned ACL fixture with verified denial and guaranteed permission restoration. No user/system ACL is changed. |
| OS-generated failure inside enumeration `MoveNext` | Not verified by checkpoint deletion; OS enumeration buffering is nondeterministic. Requires a dedicated reproducible environment or separately approved internal error seam. |
| OneDrive / cloud-backed entries | Not verified; needs a separately owned cloud fixture. Reparse exclusions remain an explicit MVP limitation. No cloud settings are changed. |
| Windows versus Windows.old protected-path boundary | Not exercised with real system fixtures. No Windows.old directory is created for testing. |
| Mapped-drive rejection | Conditional on an existing mapping; no automatic mapping or elevation. |
| Cancellation latency / synchronous OS calls | Cancellation semantics are tested; the PRD latency target is not measured here. |
| Deep recursion, 50,000+ files, memory / streaming | Deferred to large-scale validation; not implied by these tests. |

SQLite/AI/UI tests, Scan Use Case UnitTests, Host/DI verification, and MAINT-001 security maintenance are outside this task. The existing SQLite NU1903 warning is not suppressed or resolved by this test project.

## Validation record

On 2026-10-01, package restore and solution build succeeded. The solution build reported 0 errors and 4 occurrences of the existing NU1903 warning across Infrastructure and IntegrationTests.

Discovery found 26 tests. Execution: **23 passed, 0 failed, 3 skipped**. The three skipped tests were inconclusive because file / directory symbolic-link creation lacked privileges and no mapped network drive was available. They were not counted as passes.

Junction cycles, outside-root exclusion and cleanup preserving the target, unsupported reparse roots / ancestors, both 1,005-event diagnostic regressions, and checkpoint-synchronized deletion / cancellation passed. No production defect was exposed by the executed cases. The remaining verification table still applies; build and this suite do not establish complete scanner reliability.
