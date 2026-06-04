namespace CloudReportsBuildingBlocksPOC.GraphQL.Types;

using CloudReportsBuildingBlocksPOC.Models.Batches;

public class BatchStatusType : EnumType<BatchStatus>
{
    protected override void Configure(IEnumTypeDescriptor<BatchStatus> descriptor)
    {
        descriptor.Name("BatchStatus");
        descriptor.Value(BatchStatus.Pending).Description("The batch is waiting to be processed.");
        descriptor.Value(BatchStatus.Processing).Description("The batch is currently being processed.");
        descriptor.Value(BatchStatus.Completed).Description("The batch has been processed successfully.");
        descriptor.Value(BatchStatus.Failed).Description("The batch processing failed due to an error.");
    }
}