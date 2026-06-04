namespace CloudReportsBuildingBlocksPOC.Models.Batches;

public class BatchCompletedEntityDto
{
    /// <summary>
    /// Unique identifier for the batch item.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// True if an error occurred during upload.
    /// </summary>
    public bool? Errored { get; set; }

    /// <summary>
    /// Error message associated with the current operation.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
