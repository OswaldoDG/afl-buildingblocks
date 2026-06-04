namespace CloudReportsBuildingBlocksPOC.GraphQL.Types;

using CloudReportsBuildingBlocksPOC.Models.Batches;

public class BatchResponseType : ObjectType<BatchResponseDto>
{
    protected override void Configure(IObjectTypeDescriptor<BatchResponseDto> descriptor)
    {
        descriptor.Field(f => f.Id)
            .Type<NonNullType<LongType>>()
            .Description("Unique identifier for the batch request.");

        descriptor.Field(f => f.CreationDate)
            .Type<NonNullType<DateTimeType>>()
            .Description("Date and time when the batch was created.");

        descriptor.Field(f => f.BatchStatus)
            .Type<NonNullType<BatchStatusType>>()
            .Description("Current status of the batch operation.");

        descriptor.Field(f => f.Name)
            .Type<StringType>()
            .Description("Name of the batch job.");

        descriptor.Field(f => f.DestinationFolder)
            .Type<NonNullType<StringType>>()
            .Description("Destination folder where batch files will be uploaded.");

        descriptor.Field(f => f.Items)
            .Type<NonNullType<ListType<NonNullType<BatchItemResponseType>>>>()
            .Description("Batch items to be processed.");
    }
}