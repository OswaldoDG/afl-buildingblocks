namespace CloudReportsBuildingBlocksPOC.Repositories.Migrations;

using FluentMigrator;

[Migration(20260526060000)]
public class Migration20260526060000AddSessionToBatch : Migration
{
    public override void Down()
    {
        Delete.Column("sessionid").FromTable("batches");
    }

    public override void Up()
    {
        Alter.Table("batches")
            .AddColumn("sessionid").AsString(64).Nullable();
    }
}
