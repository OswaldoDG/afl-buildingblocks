namespace CloudReportsBuildingBlocksPOC.Services.Abstractions;

using CloudReportsBuildingBlocksPOC.Models.Batches;

public interface IFileBatchService
{
    /// <summary>
    /// Create an upload batch.
    /// </summary>
    /// <param name="batchRequest">Request data.</param>
    /// <returns>Created batch with upload Urls or null if an error occurs.</returns>
    Task<BatchResponseDto?> Create(BatchRequestDto batchRequest);

    /// <summary>
    /// Complete a batch.
    /// </summary>
    /// <param name="completeRequest">Request data for completing the batch.</param>
    /// <returns>True if batch is completed.</returns>
    Task<bool> Complete(BatchCompleteRequestDto completeRequest);

    /// <summary>
    /// Emits a SIgnalR event to notify clients that a batch has been completed.
    /// </summary>
    /// <param name="sessionId">SessionId</param>
    /// <returns>Completed.</returns>
    Task CompleteSession(string sessionId);
}
