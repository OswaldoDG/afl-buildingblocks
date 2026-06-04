namespace CloudReportsBuildingBlocksPOC.GraphQL.Mutations;

using CloudReportsBuildingBlocksPOC.GraphQL.Types;
using CloudReportsBuildingBlocksPOC.Models.Batches;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using HotChocolate.Authorization;
using System.Timers;

/// <summary>
/// Graph QL SORS mutations.
/// </summary>
[ExtendObjectType("Mutation")]
public partial class SorsMutations(ILogger<SorsMutations> logger)
{

    /// <summary>
    /// Creates a new batch for file uploads.
    /// </summary>
    /// <param name="input">GraphQL input parameters</param>
    /// <param name="fileBatchService">Batch file service instance.</param>
    /// <returns>Create batch with upload URls.</returns>
    [Authorize]
    public async Task<BatchResponseDto?> CreateBatch(
        BatchRequestInput input,
        [Service] IFileBatchService fileBatchService)
    {
        LogCreateBatch(input.DestinationFolder);

        var request = new BatchRequestDto
        {
            Name = input.Name,
            DestinationFolder = input.DestinationFolder,
            Items = [.. input.Items.Select(item => new BatchItemRequestDto
            {
                ItemId = item.ItemId,
                FileName = item.FileName,
                Size = item.Size,
            })],
        };

        var batch = await fileBatchService.Create(request);
        return batch;
    }

    /// <summary>
    /// Completes a batch operation by marking all items as processed.
    /// </summary>
    /// <param name="id">Batch id</param>
    /// <param name="input">GraphQL input parameters.</param>
    /// <param name="fileBatchService">Batch file service instance.</param>
    /// <returns>True if batch completes successfully.</returns>
    /// <exception cref="ArgumentException">Wrong parameters were sent.</exception>
    /// <exception cref="InvalidOperationException">Batch not found.</exception>
    [Authorize]
    [Error(typeof(ArgumentException))]
    public async Task<bool> CompleteBatch(
        long id,
        BatchCompleteRequestInput input,
        [Service] IFileBatchService fileBatchService)
    {
        LogCompleteBatch(id);

        if (input.Id != id)
        {
            throw new ArgumentException("Id in path and body do not match", nameof(id));
        }

        // Map input to DTO
        var request = new BatchCompleteRequestDto
        {
            Id = input.Id,

            Items = [.. input.Items.Select(item => new BatchCompletedEntityDto
            {
                Id = item.Id,
                Errored = item.Errored,
                ErrorMessage = item.ErrorMessage,
            })],
        };

        var result = await fileBatchService.Complete(request);

        if (!result)
        {
            throw new InvalidOperationException("Batch not found or could not be completed");
        }

        return result;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "SorsMutations-CreateBatch called with DestinationFolder: {destinationFolder}")]
    partial void LogCreateBatch(string destinationFolder);

    [LoggerMessage(Level = LogLevel.Debug, Message = "GraphQL-CompleteBatch called for batch ID: {id}")]
    partial void LogCompleteBatch(long id);
}