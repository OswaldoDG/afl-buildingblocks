namespace CloudReportsBuildingBlocksPOC.GraphQL.Types;

public class BatchItemRequestInput
{
    public string? ItemId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public int Size { get; set; }
}