namespace CloudReportsBuildingBlocksPOC.Models.Batches;

public enum BatchStatus
{
    /// <summary>
    /// The batch is waiting to be processed.
    /// </summary>
    Pending,

    /// <summary>
    /// The batch is currently being processed.
    /// </summary>
    Processing,

    /// <summary>
    /// The batch has been processed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The batch processing failed due to an error.
    /// </summary>
    Failed,
}

public enum FileStatus
{
    /// <summary>
    /// The file is waiting to be uploaded.
    /// </summary>
    Uploading,

    /// <summary>
    /// The file is fully uploaded.
    /// </summary>
    Uploaded,

    /// <summary>
    /// The file processing failed due to an error.
    /// </summary>
    Failed,
}