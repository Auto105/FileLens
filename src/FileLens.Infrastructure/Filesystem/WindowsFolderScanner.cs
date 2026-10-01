using FileLens.Application.DTO;
using FileLens.Application.Interfaces;

namespace FileLens.Infrastructure.Filesystem;

/// <summary>
/// Scans Windows directories without following reparse points or reading file contents.
/// </summary>
/// <remarks>
/// Network and device paths are outside the Sprint 2 MVP scope. Reparse exclusions may
/// omit OneDrive and other cloud-backed entries, including locally downloaded items.
/// </remarks>
public sealed class WindowsFolderScanner : IFolderScanner
{
    // An internal MVP default, adjustable after large-scale validation, not a contract limit.
    private const int DefaultDiagnosticDetailLimit = 1_000;
    private const FileAttributes RecallOnDataAccess = (FileAttributes)0x00400000;
    private readonly Action<string>? entryObserved;

    /// <summary>Creates a scanner without test checkpoints.</summary>
    public WindowsFolderScanner()
    {
    }

    // Only the friend test assembly can synchronize observed entries with cancellation/deletion.
    internal WindowsFolderScanner(Action<string> entryObserved)
    {
        this.entryObserved = entryObserved;
    }

    /// <inheritdoc />
    public async Task<ScanResult> ScanAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(folderPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);

        if (!OperatingSystem.IsWindows())
        {
            throw new NotSupportedException("This scanner supports Windows directories only.");
        }

        // Capture and resolve relative input before queuing work; the process directory can change.
        var basePath = Environment.CurrentDirectory;
        var rootPath = NormalizeRootPath(folderPath, basePath);

        return await Task.Run(
            () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateDrive(rootPath);
                var context = new ScanContext(rootPath, DefaultDiagnosticDetailLimit, entryObserved);
                ValidateRoot(rootPath, context, cancellationToken);

                var rootFolder = BuildFolderNode(rootPath, true, context, cancellationToken)!;
                var summary = CalculateSummary(rootFolder, cancellationToken);

                // A detected loss of the selected root invalidates the entire result.
                ValidateRoot(rootPath, context, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                return new ScanResult(
                    rootFolder,
                    summary.TotalFolderCount,
                    summary.TotalFileCount,
                    summary.TotalSize,
                    context.FailureCount == 0 ? ScanCompletionStatus.Complete : ScanCompletionStatus.Partial,
                    context.FailureCount,
                    context.ExcludedEntryCount,
                    context.GetDiagnostics());
            },
            cancellationToken).ConfigureAwait(false);
    }

    private static string NormalizeRootPath(string folderPath, string basePath)
    {
        var windowsPath = folderPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        if (windowsPath.StartsWith(@"\\", StringComparison.Ordinal)
            || (Path.IsPathRooted(windowsPath) && !Path.IsPathFullyQualified(windowsPath)))
        {
            throw new NotSupportedException("The root uses an unsupported path form for the scanner MVP.");
        }

        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(windowsPath, basePath));
    }

    private static void ValidateDrive(string rootPath)
    {
        var driveType = new DriveInfo(Path.GetPathRoot(rootPath)!).DriveType;

        if (driveType == DriveType.Network)
        {
            throw new NotSupportedException("The root uses unsupported storage for the scanner MVP.");
        }

        if (driveType == DriveType.NoRootDirectory)
        {
            throw new DirectoryNotFoundException("The root drive is not available.");
        }

        if (driveType == DriveType.Unknown)
        {
            throw new NotSupportedException("The root storage type could not be established.");
        }
    }

    private static void ValidateRoot(string rootPath, ScanContext context, CancellationToken cancellationToken)
    {
        var ancestors = new Stack<string>();
        for (var path = rootPath; path is not null; path = Directory.GetParent(path)?.FullName)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ancestors.Push(path);
        }

        // Check ancestors before accessing the selected root through them.
        while (ancestors.TryPop(out var path))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (context.IsProtectedDirectory(path))
            {
                throw new NotSupportedException("The root is within a protected directory excluded by scanner policy.");
            }

            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(path);
            }
            catch (FileNotFoundException exception)
            {
                throw new DirectoryNotFoundException("The scan root or an ancestor no longer exists.", exception);
            }

            if ((attributes & FileAttributes.Directory) == 0)
            {
                throw new ArgumentException("The scan root must identify a directory.", "folderPath");
            }

            if (GetAttributeExclusion(attributes) is not null)
            {
                throw new NotSupportedException("The root or an ancestor requires unsupported filesystem behavior.");
            }
        }
    }

    private static FolderNode? BuildFolderNode(
        string folderPath,
        bool isRoot,
        ScanContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IEnumerator<FileSystemInfo> enumerator;

        try
        {
            // Do not silently skip hidden/system entries or swallow access failures.
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = false,
                IgnoreInaccessible = false,
                AttributesToSkip = 0,
                ReturnSpecialDirectories = false
            };
            enumerator = new DirectoryInfo(folderPath).EnumerateFileSystemInfos("*", options).GetEnumerator();
        }
        catch (Exception exception) when (!isRoot && IsRecoverable(exception))
        {
            context.RecordFailure(folderPath, exception);
            return null;
        }

        using (enumerator)
        {
            var childFolders = new List<FolderNode>();
            var files = new List<FileNode>();
            var observedEntry = false;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                FileSystemInfo entry;

                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }

                    entry = enumerator.Current;
                    observedEntry = true;
                }
                catch (Exception exception) when (!isRoot && IsRecoverable(exception))
                {
                    context.RecordFailure(folderPath, exception);
                    if (!observedEntry)
                    {
                        return null;
                    }

                    break;
                }

                context.ObserveEntry(entry.FullName);
                cancellationToken.ThrowIfCancellationRequested();
                ProcessEntry(entry, childFolders, files, context, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return new FolderNode(folderPath, childFolders.AsReadOnly(), files.AsReadOnly());
        }
    }

    private static void ProcessEntry(
        FileSystemInfo entry,
        List<FolderNode> childFolders,
        List<FileNode> files,
        ScanContext context,
        CancellationToken cancellationToken)
    {
        var path = entry.FullName;

        if (context.IsProtectedDirectory(path))
        {
            context.RecordExclusion(path, ScanDiagnosticCategory.ProtectedDirectoryExcluded);
            return;
        }

        // Recursive calls manage their own errors; cancellation is never absorbed.
        FileAttributes attributes;
        try
        {
            var exclusion = GetAttributeExclusion(entry.Attributes);
            if (exclusion is not null)
            {
                context.RecordExclusion(path, exclusion.Value);
                return;
            }

            attributes = File.GetAttributes(path);
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            context.RecordFailure(path, exception);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var currentExclusion = GetAttributeExclusion(attributes);
        if (currentExclusion is not null)
        {
            context.RecordExclusion(path, currentExclusion.Value);
            return;
        }

        if ((attributes & FileAttributes.Directory) != 0)
        {
            var childFolder = BuildFolderNode(path, false, context, cancellationToken);
            if (childFolder is not null)
            {
                childFolders.Add(childFolder);
            }
        }
        else
        {
            var fileNode = TryCreateFileNode(path, context, cancellationToken);
            if (fileNode is not null)
            {
                files.Add(fileNode);
            }
        }
    }

    private static FileNode? TryCreateFileNode(string filePath, ScanContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var fileInfo = new FileInfo(filePath);
            var attributes = fileInfo.Attributes;
            if (attributes == (FileAttributes)(-1))
            {
                throw new FileNotFoundException("The observed file no longer exists.", filePath);
            }

            var exclusion = GetAttributeExclusion(attributes);
            if (exclusion is not null)
            {
                context.RecordExclusion(filePath, exclusion.Value);
                return null;
            }

            var fileNode = new FileNode(
                filePath,
                fileInfo.Extension,
                fileInfo.Length,
                fileInfo.CreationTimeUtc,
                fileInfo.LastWriteTimeUtc,
                attributes);
            cancellationToken.ThrowIfCancellationRequested();
            return fileNode;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            context.RecordFailure(filePath, exception);
            return null;
        }
    }

    private static ScanDiagnosticCategory? GetAttributeExclusion(FileAttributes attributes)
    {
        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            return ScanDiagnosticCategory.ReparsePointExcluded;
        }

        // Directory enumeration itself may recall remote cloud contents.
        return (attributes & RecallOnDataAccess) != 0 ? ScanDiagnosticCategory.UnsupportedPath : null;
    }

    private static bool IsRecoverable(Exception exception) =>
        exception is UnauthorizedAccessException or IOException;

    private static ScanSummary CalculateSummary(FolderNode folder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var totalFolderCount = 1;
        var totalFileCount = folder.Files.Count;
        long totalSize = 0;

        checked
        {
            foreach (var file in folder.Files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                totalSize += file.FileSize;
            }

            foreach (var childFolder in folder.ChildFolders)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var childSummary = CalculateSummary(childFolder, cancellationToken);
                totalFolderCount += childSummary.TotalFolderCount;
                totalFileCount += childSummary.TotalFileCount;
                totalSize += childSummary.TotalSize;
            }
        }

        return new ScanSummary(totalFolderCount, totalFileCount, totalSize);
    }

    private sealed class ScanContext
    {
        private readonly int diagnosticDetailLimit;
        private readonly List<ScanDiagnostic> diagnostics = [];
        private readonly string[] protectedDirectories;
        private readonly Action<string>? entryObserved;

        public ScanContext(string rootPath, int diagnosticDetailLimit, Action<string>? entryObserved)
        {
            this.diagnosticDetailLimit = diagnosticDetailLimit;
            this.entryObserved = entryObserved;
            var driveRoot = Path.GetPathRoot(rootPath)!;
            var windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            protectedDirectories =
            [
                Path.Combine(driveRoot, "System Volume Information"),
                Path.Combine(driveRoot, "$Recycle.Bin"),
                .. string.IsNullOrEmpty(windowsDirectory)
                    ? Array.Empty<string>()
                    : new[] { Path.TrimEndingDirectorySeparator(Path.GetFullPath(windowsDirectory)) }
            ];
        }

        public long FailureCount { get; private set; }

        public long ExcludedEntryCount { get; private set; }

        public void ObserveEntry(string path) => entryObserved?.Invoke(path);

        public bool IsProtectedDirectory(string path)
        {
            // Compare whole ancestor paths; Windows.old is not a descendant of Windows.
            var currentPath = path;
            while (currentPath is not null)
            {
                if (protectedDirectories.Any(protectedPath =>
                    string.Equals(currentPath, protectedPath, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                currentPath = Directory.GetParent(currentPath)?.FullName;
            }

            return false;
        }

        public void RecordFailure(string path, Exception exception)
        {
            FailureCount = checked(FailureCount + 1);
            var category = exception switch
            {
                UnauthorizedAccessException => ScanDiagnosticCategory.AccessDenied,
                FileNotFoundException or DirectoryNotFoundException => ScanDiagnosticCategory.NotFound,
                _ => ScanDiagnosticCategory.IoError
            };

            if (diagnostics.Count < diagnosticDetailLimit)
            {
                diagnostics.Add(new ScanDiagnostic(
                    path, ScanDiagnosticKind.Failure, category, exception.GetType().FullName, exception.HResult));
            }
        }

        public void RecordExclusion(string path, ScanDiagnosticCategory category)
        {
            ExcludedEntryCount = checked(ExcludedEntryCount + 1);
            if (diagnostics.Count < diagnosticDetailLimit)
            {
                diagnostics.Add(new ScanDiagnostic(path, ScanDiagnosticKind.Excluded, category));
            }
        }

        public IReadOnlyList<ScanDiagnostic> GetDiagnostics() => diagnostics.AsReadOnly();
    }

    private readonly record struct ScanSummary(int TotalFolderCount, int TotalFileCount, long TotalSize);
}
