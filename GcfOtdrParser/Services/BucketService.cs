namespace GcfOtdrParser.Services;

using Google.Cloud.Storage.V1;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class BucketService
{
    /// <summary>
    /// Fetches all object names within a specific path and its descendants.
    /// </summary>
    /// <param name="bucketName">The name of your GCP bucket.</param>
    /// <param name="prefix">The starting directory path (e.g., "folder/subfolder/").</param>
    /// <returns>A list of full object paths/names.</returns>
    public static async Task<List<string>> GetFilesRecursiveAsync(string bucketName, string prefix)
    {
        var storage = await StorageClient.CreateAsync();
        var fileList = new List<string>();

        // Ensure the prefix ends with a slash to avoid matching "folder1" when you want "folder/"
        if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith("/"))
        {
            prefix += "/";
        }

        // ListObjectsAsync returns an IAsyncEnumerable.
        // By NOT setting a Delimiter, the API automatically recurses through all "folders".
        var options = new ListObjectsOptions { };
        var objects = storage.ListObjectsAsync(bucketName, prefix, options);

        await foreach (var storageObject in objects)
        {
            // Filter out the directory placeholder itself if it exists
            if (storageObject.Name != prefix)
            {
                fileList.Add(storageObject.Name);
            }
        }

        return fileList;
    }
}
