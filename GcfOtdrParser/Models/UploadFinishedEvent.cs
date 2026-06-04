namespace GcfOtdrParser.Models;

using System.Collections.Generic;

public class BatchCompletedEvent
{
    /// <summary>
    /// Batch id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Name of the GCP bucket.
    /// </summary>
    public string BucketName { get; set; }

    /// <summary>
    /// Name of the batch job, used for logging and tracking purposes.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Destination folder where the batch files will be uploaded.
    /// </summary>
    required public string DestinationFolder { get; set; }

    /// <summary>
    /// Uploaded files in batch.
    /// </summary>
    public List<BatchCompletedItem> Items { get; set; } = [];
}


public class BatchCompletedItem
{
    /// <summary>
    /// Item unique id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the file associated with this instance.
    /// </summary>
    required public string FileName { get; set; }
}
