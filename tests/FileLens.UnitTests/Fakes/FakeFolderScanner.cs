using FileLens.Application.DTO;
using FileLens.Application.Interfaces;

namespace FileLens.UnitTests.Fakes;

internal sealed class FakeFolderScanner(Func<string, CancellationToken, Task<ScanResult>> behavior) : IFolderScanner
{
    public int InvocationCount { get; private set; }

    public string? ReceivedFolderPath { get; private set; }

    public CancellationToken ReceivedCancellationToken { get; private set; }

    public Task<ScanResult> ScanAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        InvocationCount++;
        ReceivedFolderPath = folderPath;
        ReceivedCancellationToken = cancellationToken;
        return behavior(folderPath, cancellationToken);
    }
}
