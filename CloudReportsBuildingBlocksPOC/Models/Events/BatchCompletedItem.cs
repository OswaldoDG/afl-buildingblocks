namespace CloudReportsBuildingBlocksPOC.Models.Events;

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
