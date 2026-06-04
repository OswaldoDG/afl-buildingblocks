namespace CloudReportsBuildingBlocksPOC.GraphQL.Types;

public class BatchCompleteRequestInput
{
    public long Id { get; set; }

    public List<BatchCompletedEntityInput> Items { get; set; } = [];
}