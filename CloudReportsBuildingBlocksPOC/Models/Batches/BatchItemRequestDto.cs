namespace CloudReportsBuildingBlocksPOC.Models.Batches;

public class BatchItemRequestDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the batch item at the client side.
    /// When converted to BatchItemEntity, this value will be used as the Id for the entity, ensuring consistency between client and server representations.
    /// If no item id id is provided then the filename will be used in place.
    /// </summary>
    public string? ItemId { get; set; }

    /// <summary>
    /// Gets or sets the name of the file associated with this instance.
    /// </summary>
    required public string FileName { get; set; }

    /// <summary>
    /// File size in bytes, used for validation and processing logic.
    /// </summary>
    public int Size { get; set; }
}
