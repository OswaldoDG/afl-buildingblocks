namespace CloudReportsBuildingBlocksPOC.Models.Batches;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Define a batch request for file upload and processing.
/// </summary>
[Table("Batches")]
public class BatchEntity
{
    /// <summary>
    /// Unique identifier for the batch request, used for tracking and correlation.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the object was created.
    /// </summary>
    [Column]
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity is considered closed.
    /// </summary>
    [Column]
    public DateTime? ClosingDate { get; set; }

    /// <summary>
    /// Gets or sets the current status of the batch operation.
    /// </summary>
    [Column]
    public BatchStatus BatchStatus { get; set; }

    /// <summary>
    /// The unique identifier of the user making the request.
    /// </summary>
    [Column]
    required public string UserId { get; set; }

    /// <summary>
    /// Name of the batch job, used for logging and tracking purposes.
    /// </summary>
    [Column]
    public string? Name { get; set; }

    /// <summary>
    /// Destination folder where the batch files will be uploaded.
    /// </summary>
    [Column]
    required public string DestinationFolder { get; set; }

    /// <summary>
    /// Processing error message.
    /// </summary>
    [Column]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Session identifier for SignalR notifications, used to notify the specific client that created this batch.
    /// </summary>
    [Column]
    public string? SessionId { get; set; }

    /// <summary>
    /// Batch items to be processed.
    /// </summary>
    public List<BatchItemEntity> Items { get; set; } = [];
}
