namespace FileLens.Application.DTO;

/// <summary>
/// Represents a scanned folder and its immediate scan-contract children.
/// </summary>
/// <param name="FolderPath">The folder path represented by this node.</param>
/// <param name="ChildFolders">The child folders contained within this folder.</param>
/// <param name="Files">The scanned file entries contained within this folder.</param>
public sealed record FolderNode(
    string FolderPath,
    IReadOnlyList<FolderNode> ChildFolders,
    IReadOnlyList<FileNode> Files);
