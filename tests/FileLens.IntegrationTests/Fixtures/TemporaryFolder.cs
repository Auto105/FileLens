namespace FileLens.IntegrationTests.Fixtures;

internal sealed class TemporaryFolder : IDisposable
{
    private readonly string ownerId = Guid.NewGuid().ToString("N");
    private readonly string repositoryRoot;
    private readonly string fixtureBase;

    public TemporaryFolder()
    {
        repositoryRoot = FindRepositoryRoot();
        fixtureBase = Path.Combine(repositoryRoot, "temp", "FileLens.IntegrationTests");
        EnsureOrdinaryAncestors(fixtureBase);
        Directory.CreateDirectory(fixtureBase);
        ContainerPath = Path.Combine(fixtureBase, ownerId);
        if (Directory.Exists(ContainerPath) || File.Exists(ContainerPath))
        {
            throw new IOException("The fixture ownership path already exists.");
        }

        Directory.CreateDirectory(ContainerPath);
        File.WriteAllText(Path.Combine(ContainerPath, ".owner"), ownerId);
        RootPath = Path.Combine(ContainerPath, "scan");
        Directory.CreateDirectory(RootPath);
    }

    public string ContainerPath { get; }
    public string RootPath { get; }

    public string PathOf(string relativePath) => ResolveWithin(RootPath, relativePath);

    public string OwnedPath(string relativePath) => ResolveWithin(ContainerPath, relativePath);

    public string CreateDirectory(string relativePath)
    {
        var path = PathOf(relativePath);
        EnsureOrdinaryAncestors(path);
        Directory.CreateDirectory(path);
        return path;
    }

    public string CreateFile(string relativePath, int size = 0)
    {
        var path = PathOf(relativePath);
        EnsureOrdinaryAncestors(path);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, new byte[size]);
        return path;
    }

    public void Dispose()
    {
        EnsureOrdinaryAncestors(ContainerPath);
        if (!string.Equals(Path.GetDirectoryName(ContainerPath), fixtureBase, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(Path.GetFileName(ContainerPath), ownerId, StringComparison.Ordinal)
            || File.ReadAllText(Path.Combine(ContainerPath, ".owner")) != ownerId)
        {
            throw new IOException("Fixture ownership is unclear; cleanup was refused.");
        }

        DeleteOwnedEntry(ContainerPath);
    }

    private void DeleteOwnedEntry(string path)
    {
        _ = ResolveWithin(ContainerPath, Path.GetRelativePath(ContainerPath, path));
        var attributes = File.GetAttributes(path);
        var isDirectory = (attributes & FileAttributes.Directory) != 0;

        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            if (isDirectory)
            {
                Directory.Delete(path, false);
            }
            else
            {
                File.Delete(path);
            }

            return;
        }

        if (isDirectory)
        {
            foreach (var child in Directory.EnumerateFileSystemEntries(path))
            {
                DeleteOwnedEntry(child);
            }
        }

        File.SetAttributes(path, attributes & ~(FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System));
        if (isDirectory)
        {
            Directory.Delete(path, false);
        }
        else
        {
            File.Delete(path);
        }
    }

    private void EnsureOrdinaryAncestors(string path)
    {
        for (var current = path; current is not null; current = Directory.GetParent(current)?.FullName)
        {
            if (File.Exists(current) || Directory.Exists(current))
            {
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                {
                    throw new IOException("Fixture paths must not pass through reparse points.");
                }
            }

            if (string.Equals(current, repositoryRoot, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        throw new IOException("Fixture path is outside the repository.");
    }

    private static string ResolveWithin(string boundary, string relativePath)
    {
        var fullPath = Path.GetFullPath(relativePath, boundary);
        var relative = Path.GetRelativePath(boundary, fullPath);
        var segments = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (Path.IsPathRooted(relative) || segments.Any(segment => segment == ".."))
        {
            throw new IOException("Fixture operation would escape its ownership boundary.");
        }

        return fullPath;
    }

    private static string FindRepositoryRoot()
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current is not null; current = current.Parent)
        {
            if (File.Exists(Path.Combine(current.FullName, "FileLens.slnx"))
                && Directory.Exists(Path.Combine(current.FullName, "tests", "FileLens.IntegrationTests")))
            {
                return current.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the FileLens repository for owned fixtures.");
    }
}
