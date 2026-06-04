namespace CloudReportsBuildingBlocksPOC.Models.Batches;

public class BatchResponseDto
{
    /// <summary>
    /// Unique identifier for the batch request, used for tracking and correlation.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the object was created.
    /// </summary>
    public DateTimeOffset CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the current status of the batch operation.
    /// </summary>
    public BatchStatus BatchStatus { get; set; }

    /// <summary>
    /// Name of the batch job, used for logging and tracking purposes.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Destination folder where the batch files will be uploaded.
    /// </summary>
    required public string DestinationFolder { get; set; }

    /// <summary>
    /// Batch items to be processed.
    /// </summary>
    public List<BatchItemResponseDto> Items { get; set; } = [];
}
