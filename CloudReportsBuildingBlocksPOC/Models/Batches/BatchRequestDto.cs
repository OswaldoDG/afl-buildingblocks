namespace CloudReportsBuildingBlocksPOC.Models.Batches;

/// <summary>
/// Batch creation request Dto.
/// </summary>
public class BatchRequestDto
{
   /// <summary>
   /// Name of the batch job, used for logging and tracking purposes.
   /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Destination folder where the batch files will be uploaded.
    /// </summary>
    required public string DestinationFolder { get; set; }

    /// <summary>
    /// Session identifier to distinguish between shared user accounts.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Batch items to be processed.
    /// </summary>
    public List<BatchItemRequestDto> Items { get; set; } = [];
}
