namespace CloudReportsBuildingBlocksPOC.Models.Batches;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Item in a batch request, representing a single file to be processed.
/// </summary>
[Table("BatchItems")]
public class BatchItemEntity
{
    /// <summary>
    /// Unique identifier for the batch item.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Associated batch ID, linking this item to its parent batch request for processing and tracking.
    /// </summary>
    [Column]
    public long BatchId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the batch item at the client side.
    /// When converted to BatchItemEntity, this value will be used as the Id for the entity, ensuring consistency between client and server representations.
    /// If no item id id is provided then the filename will be used in place.
    /// </summary>
    [Column]
    public string? ItemId { get; set; }

    /// <summary>
    /// Gets or sets the name of the file associated with this instance.
    /// </summary>
    [Column]
    required public string FileName { get; set; }

    /// <summary>
    /// File size in bytes, used for validation and processing logic.
    /// </summary>
    [Column]
    public int Size { get; set; }

    /// <summary>
    /// File upload status.
    /// </summary>
    [Column]
    public FileStatus Status { get; set; } = FileStatus.Uploading;

    /// <summary>
    /// Processing error message.
    /// </summary>
    [Column]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the batch entity associated with the current context.
    /// </summary>
    public BatchEntity Batch { get; set; }
}
