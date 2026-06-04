namespace ClientPOC.Models;

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