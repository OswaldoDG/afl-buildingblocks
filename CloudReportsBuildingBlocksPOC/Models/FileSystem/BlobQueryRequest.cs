namespace CloudReportsBuildingBlocksPOC.Models.FileSystem;

/// <summary>
/// DTO to get storage items.
/// </summary>
public class BlobQueryRequest
{
    /// <summary>
    /// Defines if the search is recursive.
    /// </summary>
    public bool Recursive { get; set; }

    /// <summary>
    /// Prefix for the storage container.
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// Defines if the results should be flattened, GraphQL cant deal easily with nested results.
    /// </summary>
    public bool Flattened { get; set; }

    /// <summary>
    /// Specifies if the results should include download URLs for the files. This can be used to provide direct access to the files without needing additional API calls to generate the URLs.
    /// </summary>
    public bool IncludeDownloadUrl { get; set; }
}
