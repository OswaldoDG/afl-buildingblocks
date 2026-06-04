namespace CloudReportsBuildingBlocksPOC.GraphQL.Types;

public class BatchCompletedEntityInput
{
    public long Id { get; set; }

    public bool? Errored { get; set; }

    public string? ErrorMessage { get; set; }
}