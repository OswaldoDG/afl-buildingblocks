namespace CloudReportsBuildingBlocksPOC.Models.FileSystem;

/// <summary>
/// Represents a file in the file system.
/// </summary>
public class BlobEntry
{
    /// <summary>
    /// File name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Full file system path associated with the current object.
    /// </summary>
    public string? FullPath { get; set; }

    /// <summary>
    /// Parent directory path.
    /// </summary>
    public string? ParentPath { get; set; }

    /// <summary>
    /// Size in bytes.
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// Determines if the blob is a folder.
    /// </summary>
    public bool IsFolder { get; set; }

    /// <summary>
    /// Download URL for the file. Null if the blob is a folder or if the download URL was not requested.
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// File last modification date.
    /// </summary>
    public DateTimeOffset? LastModificationTime { get; set; }

    /// <summary>
    /// Child files in case the current blob is a folder. Null if the blob is a file.
    /// </summary>
    public List<BlobEntry>? Children { get; set; }
}
