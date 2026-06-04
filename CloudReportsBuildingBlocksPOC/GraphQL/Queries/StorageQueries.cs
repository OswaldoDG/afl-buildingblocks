namespace CloudReportsBuildingBlocksPOC.GraphQL.Queries;

using CloudReportsBuildingBlocksPOC.GraphQL.Types;
using CloudReportsBuildingBlocksPOC.Models.FileSystem;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using HotChocolate.Authorization;

/// <summary>
/// GraphQL queries for storage operations.
/// </summary>
/// <param name="logger">Logger.</param>
/// <param name="fileSystemProvider">File system provider.</param>
[ExtendObjectType("Query")]
public partial class StorageQueries(ILogger<StorageQueries> logger, IFileSystemProvider fileSystemProvider)
{
    /// <summary>
    /// Get content listing for the specified path.
    /// </summary>
    /// <param name="input">Content query input.</param>
    /// <returns>List of blob entries.</returns>
    [Authorize]
    public async Task<List<BlobEntry>> GetContentEntries(
        BlobQueryInput input)
    {
        LogGetContentEntries(input.Prefix);
        return await fileSystemProvider.GetContent(input.Prefix, input.Recursive, input.Flattened, input.IncludeDownloadUrl );
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "GraphQL-GetContentEntries called with prefix: {prefix}")]
    partial void LogGetContentEntries(string prefix);
}
