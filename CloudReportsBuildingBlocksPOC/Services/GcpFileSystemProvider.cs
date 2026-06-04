namespace CloudReportsBuildingBlocksPOC.Services;

using CloudReportsBuildingBlocksPOC.Models.FileSystem;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using FluentStorage;
using FluentStorage.Blobs;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Caching.Distributed;
using System.IO;
using System.IO.Compression;

/// <summary>
/// GCP FS provider.
/// </summary>
/// <param name="logger">Logger service.</param>
/// <param name="configuration">Config options.</param>
/// <param name="distributedCache">Cache service.</param>
public partial class GcpFileSystemProvider(ILogger<GcpFileSystemProvider> logger, IConfiguration configuration, IDistributedCache distributedCache, UrlSigner urlSigner, IBlobStorage blobStore) : IFileSystemProvider
{
    private readonly string _containerName = configuration.GetValue<string>("GcpStorage:BucketName")!;
    private readonly int _fileSystemContentCachedMinutes = configuration.GetValue<int>("FileSystemContentCachedMinutes", 0);

    /// <summary>
    /// Generates a signed URL for a GCS object.
    /// </summary>
    /// <param name="objectPath">The path/name of the file in the bucket.</param>
    /// <param name="durationInMinutes">How long the URL remains valid.</param>
    /// <returns>A signed URL string.</returns>
    public async Task<string> CreateDownloadUrl(string objectPath, int durationInMinutes = 15)
    {
        LogPathMethodCall("CreateDownloadUrl", objectPath);
        // Generate the signed URL for a GET request (reading the file)
        return await urlSigner.SignAsync(_containerName, objectPath.TrimStart('/').TrimEnd('/'), TimeSpan.FromMinutes(durationInMinutes), HttpMethod.Get);
    }

    /// <summary>
    /// Generates a signed URL for a GCS object.
    /// </summary>
    /// <param name="objectPath">The path/name of the file in the bucket.</param>
    /// <param name="durationInMinutes">How long the URL remains valid.</param>
    /// <returns>A signed URL string.</returns>
    public async Task<string> CreateUploadUrl(string objectPath, int durationInMinutes = 15)
    {
        LogPathMethodCall("CreateUploadUrl", objectPath);
        // Generate the signed URL for a GET request (reading the file)
        return await urlSigner.SignAsync(_containerName, objectPath.TrimStart('/').TrimEnd('/'), TimeSpan.FromMinutes(durationInMinutes), HttpMethod.Put);
    }

    /// <summary>
    /// Generates a signed URL for a GCS object.
    /// </summary>
    /// <param name="objectPaths">List of path/name of the files in the bucket.</param>
    /// <param name="durationInMinutes">How long the URL remains valid.</param>
    /// <returns>A signed URL string.</returns>
    public async Task<List<string>> CreateUploadUrl(List<string> objectPaths, int durationInMinutes = 15)
    {
        List<string> result = [];
        int batchSize = configuration.GetValue<int>("GcpStorage:SignedUrlBatchSize", 100);
        TimeSpan duration = TimeSpan.FromMinutes(durationInMinutes);

        List<Task<string>> urlTasks = new List<Task<string>>();
        int items = 0;
        foreach (var f in objectPaths)
        {
            urlTasks.Add(urlSigner.SignAsync(_containerName, f.TrimStart('/').TrimEnd('/'), duration, HttpMethod.Put));
            items++;

            if (items >= batchSize)
            {
                var urls = await Task.WhenAll(urlTasks);
                result.AddRange(urls);
                urlTasks.Clear();
                items = 0;
            }
        }

        if (urlTasks.Count > 0)
        {
            var urls = await Task.WhenAll(urlTasks);
            result.AddRange(urls);
        }

        // Generate the signed URL for a GET request (reading the file)
        return result;
    }

    public async Task<bool> DirectoryExists(string path, bool? create = false)
    {
        LogPathMethodCall("DirectoryExists", path);
        var objects = await blobStore.ListAsync(path?.TrimStart('/').TrimEnd('/'), recurse: false);
        if (objects.Count == 0)
        {
            if (create != true)
            {
                return false;
            }
            else
            {
                // Create a placeholder object to represent the directory
                await blobStore.WriteTextAsync(path, string.Empty);
            }
        }

        return true;
    }

    public async Task<List<BlobEntry>> GetContent(string path, bool recursive, bool flattened, bool downloadUrl)
    {
        var cacheKey = CacheKey(path, recursive, flattened);
        List<BlobEntry> content;

        if (_fileSystemContentCachedMinutes > 0)
        {
            string? cachedData = await distributedCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                content = System.Text.Json.JsonSerializer.Deserialize<List<BlobEntry>>(cachedData) ?? [];
                if (downloadUrl)
                {
                    var files = content.Where(e => !e.IsFolder).ToList();
                    foreach (var file in files)
                    {
                        if (downloadUrl)
                        {
                            file.DownloadUrl = await CreateDownloadUrl(file.FullPath!);
                        }
                    }
                }

                return content;
            }
        }
        content = await RecursePath(blobStore, path, recursive, flattened, downloadUrl);

        if (_fileSystemContentCachedMinutes > 0)
        {
            string serializedContent = System.Text.Json.JsonSerializer.Serialize(content);
            await distributedCache.SetStringAsync(cacheKey, serializedContent, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_fileSystemContentCachedMinutes) });
        }

        return content;
    }

    public async Task<string> ZipContent(string containerPath, string zipPath, bool recursive)
    {
        var structure = await RecursePath(blobStore, containerPath, recursive, true, false);

        // Use a FileStream for the output, but ZipArchive works with any Stream (e.g., MemoryStream)
        using FileStream zipToOpen = new(zipPath, FileMode.Create);
        using ZipArchive archive = new(zipToOpen, ZipArchiveMode.Create);

        foreach (var file in structure.Where(e => !e.IsFolder && e.Size > 0))
        {
            // Create a zip entry with the relative path inside the zip
            string relativePath = file.FullPath!.TrimStart('/', '\\');
            ZipArchiveEntry entry = archive.CreateEntry(relativePath);
            using Stream entryStream = await entry.OpenAsync();
            using Stream fileStream = await blobStore.OpenReadAsync(file.FullPath);
            await fileStream.CopyToAsync(entryStream);
        }

        return zipPath;
    }

    private static string CacheKey(string path, bool recursive, bool flattened) => $"content:{path}:{recursive}:{flattened}";

    private static BlobEntry ToBlobEntry(FluentStorage.Blobs.Blob blob, bool flattened)
    {
        return new BlobEntry
        {
            Name = blob.Name,
            FullPath = blob.FullPath,
            Size = blob.Size,
            LastModificationTime = blob.LastModificationTime,
            IsFolder = blob.IsFolder,
            ParentPath = flattened ? blob.FullPath.TrimEnd(blob.Name.ToCharArray()).TrimEnd('/') : null,
        };
    }

    private async Task<List<BlobEntry>> RecursePath(IBlobStorage storage, string path, bool recursive, bool flattened, bool downloadUrl)
    {
        IReadOnlyCollection<FluentStorage.Blobs.Blob> blobs = await storage.ListAsync(path?.TrimStart('/').TrimEnd('/'));
        List<BlobEntry> allEntries = [];
        foreach (var blob in blobs)
        {
            allEntries.Add(ToBlobEntry(blob, flattened));

            if (recursive)
            {
                var folders = allEntries.Where(e => e.IsFolder).ToList();
                foreach (var folder in folders)
                {
                    // Recursively list files in the subfolder
                    var subEntries = await RecursePath(storage, folder.FullPath!, recursive, flattened, downloadUrl);
                    if (flattened)
                    {
                        allEntries.AddRange(subEntries);
                    }
                    else
                    {
                        folder.Children = subEntries;
                    }
                }
            }
        }

        if (downloadUrl)
        {
            var files = allEntries.Where(e => !e.IsFolder).ToList();
            foreach (var file in files)
            {
                if (downloadUrl)
                {
                    file.DownloadUrl = await CreateDownloadUrl(file.FullPath!);
                }
            }
        }

        return allEntries;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "GcpFileSystemProvider-{method} for path {path}")]
    partial void LogPathMethodCall(string method, string path);
}
