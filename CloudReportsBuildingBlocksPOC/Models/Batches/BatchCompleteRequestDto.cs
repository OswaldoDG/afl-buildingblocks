namespace CloudReportsBuildingBlocksPOC.Models.Batches;

/// <summary>
/// Request to finalize a batch operation, indicating that all items have been processed and providing the results of each item, including any errors that may have occurred during processing.
/// </summary>
public class BatchCompleteRequestDto
{
    /// <summary>
    /// Unique identifier for the batch request, used for tracking and correlation.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// List of completed batch items, including their status and any error messages.
    /// </summary>
    public List<BatchCompletedEntityDto> Items { get; set; } = [];
}
