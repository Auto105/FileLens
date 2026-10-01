# FileLens BootstrapTests

This Windows-only project verifies the production Bootstrap host, validated service registration,
transient scanner and ViewModel resolution, default Host startup / shutdown, Serilog disposal,
and logging path configuration. It reuses the centrally pinned MSTest packages.

An isolated STA thread constructs MainWindow instances and checks their injected DataContexts
and compiled XAML. It does not construct a WPF Application, display windows, or run the
Application lifecycle. Tests are nonparallel. Test-owned logs are retained for review under
the existing ignored `temp/FileLens.BootstrapTests/<guid>/` directory.

## Automated verification

```powershell
dotnet restore FileLens.slnx -v minimal
dotnet build FileLens.slnx -v minimal
dotnet test tests/FileLens.BootstrapTests/FileLens.BootstrapTests.csproj --no-build --no-restore --list-tests
dotnet test tests/FileLens.BootstrapTests/FileLens.BootstrapTests.csproj --no-build --no-restore --logger "trx;LogFileName=bootstrap.trx" --results-directory TestResults/BootstrapTests -v normal
```

## Manual desktop smoke validation

Run `src/FileLens.Bootstrap/bin/Debug/net10.0-windows/FileLens.Bootstrap.exe` in an interactive
Windows session. Confirm exactly one FileLens window, no startup error, normal close, Host
shutdown entries, and no remaining process. Repeat from a different working directory and
confirm logs still use `%LOCALAPPDATA%/FileLens/logs`.

Override the log directory using `--FileLens:Logging:LogDirectory=<absolute directory>` when
isolated logs are needed. Relative overrides use `%LOCALAPPDATA%/FileLens` as their base.
Host content root is the executable directory, independent of the working directory.

Actual window display / close, dispatcher shutdown, and startup failure dialogs require the
desktop smoke check; these automated tests do not establish those behaviors. Active scan
shutdown coordination, Scan Use Case, persistence, scanner reliability changes, UI automation,
and MAINT-001 are outside this task.

## Validation record

On 2026-10-01, restore and solution build succeeded with 0 errors. The full build reported
8 occurrences of the existing NU1903 warning across restore / build and four consumers;
MAINT-001 remains open. Discovery found 7 BootstrapTests: 7 passed, 0 failed, 0 skipped.
Scanner regression discovery found 26 tests: 23 passed, 0 failed, 3 environment-dependent skips.

Process-assisted desktop smoke checks launched the actual executable from the repository root
and an ignored temporary directory. Captured windows were visually inspected: exactly one
FileLens shell appeared in each run, with no startup error dialog. Normal window-close requests
returned exit code 0 and left no launched process. Both runs wrote Host startup / shutdown
entries under `%LOCALAPPDATA%/FileLens/logs`. These are desktop smoke observations, not full
human interaction coverage or an automated Application lifecycle test. Startup failure-dialog
injection and active scan shutdown coordination remain unverified.
