namespace CloudReportsBuildingBlocksPOC.GraphQL.Queries;

using CloudReportsBuildingBlocksPOC.Models.Batches;
using CloudReportsBuildingBlocksPOC.Repositories;
using HotChocolate.Authorization;

/// <summary>
/// SOR GraphQL queries.
/// </summary>
/// <param name="logger">Logger.</param>
/// <param name="repository">Entity repo.</param>
[ExtendObjectType("Query")]
public partial class SorsQueries(ILogger<SorsQueries> logger, IGenericRepository<BatchEntity> repository)
{
    /// <summary>
    /// Gets a batch by its ID.
    /// </summary>
    /// <param name="id">Batch Id.</param>
    /// <returns>Batch entity.</returns>
    [Authorize]
    public async Task<BatchEntity?> GetBatch(
        long id)
    {
        LogGetBatch(id);
        return await repository.GetByIdAsync(id);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "GraphQL-GetBatch called for ID: {id}")]
    partial void LogGetBatch(long id);
}