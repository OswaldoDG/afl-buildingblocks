namespace CloudReportsBuildingBlocksPOC.Repositories.Migrations;

using FluentMigrator;

[Migration(20260327170000)]
public class Migration20260327170000InitialConfig : Migration
{
    public override void Down()
    {
        // Postgresql must be lowercased.
        Delete.Table("batches");
        Delete.Table("batchitems");
    }

    public override void Up()
    {
        // Postgresql must be lowercased.
        Create.Table("batches")
            .WithColumn("id").AsInt64().NotNullable().PrimaryKey().Identity()
            .WithColumn("creationdate").AsDateTime().NotNullable()
            .WithColumn("closingdate").AsDateTime().Nullable()
            .WithColumn("batchstatus").AsInt32().NotNullable()
            .WithColumn("userid").AsString(64).NotNullable()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("errormessage").AsString(1000).Nullable()
            .WithColumn("destinationfolder").AsString(512).NotNullable();

        Create.Table("batchitems")
            .WithColumn("id").AsInt64().NotNullable().PrimaryKey().Identity()
            .WithColumn("batchid").AsInt64().ForeignKey("batches", "id").OnDeleteOrUpdate(System.Data.Rule.Cascade)
            .WithColumn("itemid").AsString(64).Nullable()
            .WithColumn("filename").AsString(512).NotNullable()
            .WithColumn("size").AsInt32().NotNullable()
            .WithColumn("status").AsInt32().NotNullable()
            .WithColumn("errormessage").AsString(1000).Nullable();
    }
}
