namespace CloudReportsBuildingBlocksPOC.Services.Abstractions;

using BlobEntry = Models.FileSystem.BlobEntry;

public interface IFileSystemProvider
{
    /// <summary>
    /// Create an upload URL for a file in the specified bucket. The URL will be valid for the specified duration.
    /// </summary>
    /// <param name="objectPath">File path including name and extension path/file.ext.</param>
    /// <param name="durationInMinutes">How long the URL remains valid.</param>
    /// <returns>A signed URL string.</returns>
    Task<string> CreateUploadUrl(string objectPath, int durationInMinutes = 15);

    /// <summary>
    /// Create an upload URL for a file in the specified bucket. The URL will be valid for the specified duration.
    /// </summary>
    /// <param name="objectPaths">File path including name and extension path/file.ext.</param>
    /// <param name="durationInMinutes">How long the URL remains valid.</param>
    /// <returns>A signed URL string.</returns>
    Task<List<string>> CreateUploadUrl(List<string> objectPaths, int durationInMinutes = 15);


    /// <summary>
    /// Create an upload URL for a file in the specified bucket. The URL will be valid for the specified duration.
    /// </summary>
    /// <param name="objectPath">File path including name and extension path/file.ext.</param>
    /// <param name="durationInMinutes">How long the URL remains valid.</param>
    /// <returns>A signed URL string.</returns>
    Task<string> CreateDownloadUrl(string objectPath, int durationInMinutes = 15);

    /// <summary>
    /// Get a list of files in the specific path.
    /// </summary>
    /// <param name="path">Path in the container.</param>
    /// <param name="recursive">Determines if the listing should be recursive.</param>
    /// <param name="flattened">Determines if the listing is returned as a flat list.</param>
    /// <param name="downloadUrl">Determines if the listing should include download URLs.</param>
    /// <returns>List of files or null if error.</returns>
    Task<List<BlobEntry>> GetContent(string path, bool recursive, bool flattened, bool downloadUrl);

    /// <summary>
    /// Zip the content int the specific path.
    /// </summary>
    /// <param name="containerPath">Path in the container.</param>
    /// <param name="zipPath">Path where the zip file will be created.</param>
    /// <param name="recursive">Determines if the listing should be recursive.</param>
    /// <returns>List of files or null if error.</returns>
    Task<string> ZipContent(string containerPath, string zipPath, bool recursive);

    /// <summary>
    /// Find/Create a directory using a path.
    /// </summary>
    /// <param name="path">Path of the directory to create.</param>
    /// <param name="create">Determines if path must be created if it doesn't exists.</param>
    /// <returns>true if the paths exists.</returns>
    Task<bool> DirectoryExists(string path, bool? create = false);
}
