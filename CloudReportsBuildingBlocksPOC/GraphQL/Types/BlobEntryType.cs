namespace CloudReportsBuildingBlocksPOC.GraphQL.Types;

using CloudReportsBuildingBlocksPOC.Models.FileSystem;

public class BlobEntryType : ObjectType<BlobEntry>
{
    protected override void Configure(IObjectTypeDescriptor<BlobEntry> descriptor)
    {
        descriptor.Field(f => f.Name).Type<NonNullType<StringType>>();
        descriptor.Field(f => f.IsFolder).Type<NonNullType<BooleanType>>();
    }
}