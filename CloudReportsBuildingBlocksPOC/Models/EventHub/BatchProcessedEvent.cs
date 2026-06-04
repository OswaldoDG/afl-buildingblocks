using CloudReportsBuildingBlocksPOC.Models.Batches;

namespace CloudReportsBuildingBlocksPOC.Models.EventHub;

public class BatchProcessedEvent
{
    public string SessionId { get; set; }
    public string UserId { get; set; }
    public string ProjectId { get; set; }
    public BatchCompleteRequestDto BatchInfo { get; set; }

}
