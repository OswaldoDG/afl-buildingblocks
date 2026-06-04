namespace CloudReportsBuildingBlocksPOC.GraphQL.Types;

public class BatchRequestInput
{
    public string? Name { get; set; }

    public string DestinationFolder { get; set; } = string.Empty;

    public List<BatchItemRequestInput> Items { get; set; } = [];
}