using FileLens.Application.DTO;
using FileLens.Application.Interfaces;

namespace FileLens.Infrastructure.Filesystem;

/// <summary>
/// Provides a Windows-specific placeholder implementation of the folder scanning contract.
/// </summary>
public sealed class WindowsFolderScanner : IFolderScanner
{
    /// <summary>
    /// Scans the specified folder path and constructs a directory-only folder tree.
    /// </summary>
    /// <param name="folderPath">The absolute or relative path of the folder to scan.</param>
    /// <param name="cancellationToken">A token that can cancel the scan operation.</param>
    /// <returns>A task that represents the asynchronous scan operation and its current scan result.</returns>
    public Task<ScanResult> ScanAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        return Task.Run(
            () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var rootFolder = BuildFolderNode(folderPath, cancellationToken);
                var summary = CalculateSummary(rootFolder, cancellationToken);

                return new ScanResult(
                    rootFolder,
                    summary.TotalFolderCount,
                    summary.TotalFileCount,
                    summary.TotalSize);
            },
            cancellationToken);
    }

    private static FolderNode BuildFolderNode(string folderPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var childFolders = new List<FolderNode>();
        var files = new List<FileNode>();

        foreach (var childDirectoryPath in EnumerateChildDirectories(folderPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            childFolders.Add(BuildFolderNode(childDirectoryPath, cancellationToken));
        }

        foreach (var filePath in EnumerateFiles(folderPath))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (TryCreateFileNode(filePath, out var fileNode))
            {
                files.Add(fileNode);
            }
        }

        return new FolderNode(
            folderPath,
            childFolders,
            files);
    }

    private static IEnumerable<string> EnumerateChildDirectories(string folderPath)
    {
        try
        {
            return Directory.EnumerateDirectories(folderPath);
        }
        catch (UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
        catch (DirectoryNotFoundException)
        {
            return Array.Empty<string>();
        }
        catch (IOException)
        {
            return Array.Empty<string>();
        }
    }

    private static IEnumerable<string> EnumerateFiles(string folderPath)
    {
        try
        {
            return Directory.EnumerateFiles(folderPath);
        }
        catch (UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
        catch (DirectoryNotFoundException)
        {
            return Array.Empty<string>();
        }
        catch (IOException)
        {
            return Array.Empty<string>();
        }
    }

    private static bool TryCreateFileNode(string filePath, out FileNode fileNode)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);

            fileNode = new FileNode(
                filePath,
                fileInfo.Extension,
                fileInfo.Length,
                fileInfo.CreationTimeUtc,
                fileInfo.LastWriteTimeUtc,
                fileInfo.Attributes);

            return true;
        }
        catch (UnauthorizedAccessException)
        {
            fileNode = null!;
            return false;
        }
        catch (FileNotFoundException)
        {
            fileNode = null!;
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            fileNode = null!;
            return false;
        }
        catch (IOException)
        {
            fileNode = null!;
            return false;
        }
    }

    private static ScanSummary CalculateSummary(FolderNode rootFolder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var totalFolderCount = 1;
        var totalFileCount = rootFolder.Files.Count;
        var totalSize = rootFolder.Files.Sum(file => file.FileSize);

        foreach (var childFolder in rootFolder.ChildFolders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var childSummary = CalculateSummary(childFolder, cancellationToken);
            totalFolderCount += childSummary.TotalFolderCount;
            totalFileCount += childSummary.TotalFileCount;
            totalSize += childSummary.TotalSize;
        }

        return new ScanSummary(totalFolderCount, totalFileCount, totalSize);
    }

    private readonly record struct ScanSummary(
        int TotalFolderCount,
        int TotalFileCount,
        long TotalSize);
}
