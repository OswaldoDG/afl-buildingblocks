namespace CloudReportsBuildingBlocksPOC.GraphQL.Types;

/// <summary>
/// Query for strage content.
/// </summary>
public class BlobQueryInput
{
    /// <summary>
    /// Path prefix at the container.
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// Specify if should be recursive.
    /// </summary>
    public bool Recursive { get; set; } = false;

    /// <summary>
    /// Specify if results should be flattened, GraphQL cant deal easily with nested results.
    /// </summary>
    public bool Flattened { get; set; }

    /// <summary>
    /// Specifies if the results should include download URLs for the files. This can be used to provide direct access to the files without needing additional API calls to generate the URLs.
    /// </summary>
    public bool IncludeDownloadUrl { get; set; }
}
