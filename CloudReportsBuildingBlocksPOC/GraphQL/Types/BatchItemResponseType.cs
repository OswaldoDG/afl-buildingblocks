namespace CloudReportsBuildingBlocksPOC.GraphQL.Types;

using CloudReportsBuildingBlocksPOC.Models.Batches;

public class BatchItemResponseType : ObjectType<BatchItemResponseDto>
{
    protected override void Configure(IObjectTypeDescriptor<BatchItemResponseDto> descriptor)
    {
        descriptor.Field(f => f.Id)
            .Type<NonNullType<LongType>>()
            .Description("Unique identifier for the batch item.");

        descriptor.Field(f => f.ItemId)
            .Type<StringType>()
            .Description("Client-side unique identifier for the batch item.");

        descriptor.Field(f => f.UploadUrl)
            .Type<StringType>()
            .Description("Pre-signed URL for uploading the file.");
    }
}