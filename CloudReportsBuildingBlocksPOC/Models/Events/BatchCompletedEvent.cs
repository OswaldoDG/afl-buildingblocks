namespace CloudReportsBuildingBlocksPOC.Models.Events;

/// <summary>
/// Event indicating successful batch completion.
/// </summary>
public class BatchCompletedEvent
{
    /// <summary>
    /// Batch id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Name of the GCP bucket.
    /// </summary>
    required public string BucketName { get; set; }

    /// <summary>
    /// Name of the batch job, used for logging and tracking purposes.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Destination folder where the batch files will be uploaded.
    /// </summary>
    required public string DestinationFolder { get; set; }

    /// <summary>
    /// Session identifier for SignalR notifications, used to target the specific client that created this batch.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Uploaded files in batch.
    /// </summary>
    public List<BatchCompletedItem> Items { get; set; } = [];
}
