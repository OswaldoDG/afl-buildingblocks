namespace CloudReportsBuildingBlocksPOC.Repositories.Migrations;

using FluentMigrator;

[Migration(20260421060000)]
public class Migration20260421060000SorEntities: Migration
{
    public override void Down()
    {
        Execute.Sql("DROP TYPE distance_unit_types;");
        Execute.Sql("DROP TYPE test_port_types;");
        Execute.Sql("DROP TYPE rule_types;");
        Execute.Sql("DROP TYPE standards_types;");
        Execute.Sql("DROP TYPE rule_loss_types;");
        Execute.Sql("DROP TYPE power_units;");
        Execute.Sql("DROP TYPE category_types;");
        Execute.Sql("DROP TYPE fiber_type_actual;");
        Execute.Sql("DROP TYPE use_types;");
        Execute.Sql("DROP TYPE calculation_types;");
        Execute.Sql("DROP TYPE length_thresh_types;");
        Execute.Sql("DROP TYPE file_modification_types;");
        Execute.Sql("DROP TYPE unit_modes;");

        Execute.Sql("DROP TABLE lossthreshold;");
        Execute.Sql("DROP TABLE certrule;");
        Execute.Sql("DROP TABLE atdjob;");
        Execute.Sql("DROP TABLE instrumentinfo;");
        Execute.Sql("DROP TABLE atdentry;");
    }

    public override void Up()
    {
        Execute.Sql("CREATE TYPE distance_unit_types AS ENUM ('kilometers','miles','kilofeet','meters','millimeters','feet');");
        Execute.Sql("CREATE TYPE test_port_types AS ENUM ('none','multimode','singlemode');");
        Execute.Sql("CREATE TYPE rule_types AS ENUM ('none','tia','iso','en','user','application');");
        Execute.Sql("CREATE TYPE standards_types AS ENUM ('cabling','application');");
        Execute.Sql("CREATE TYPE rule_loss_types AS ENUM ('none','totalloss','fiberloss');");
        Execute.Sql("CREATE TYPE power_units AS ENUM ('db','dbm','w');");
        Execute.Sql("CREATE TYPE category_types AS ENUM ('none','tia','iso','en','user','ethernet','fibrechannel','fddipmd','other');");
        Execute.Sql("CREATE TYPE fiber_type_actual AS ENUM ('none','os1','os2','smuser','om1625125um','om150125um','om2625125um','om250125um','om3laseroptimized50125um','om4laseroptimized50125um','mmuser','mmuser50125um','mmuser625125um','g657a1','om5');");
        Execute.Sql("CREATE TYPE use_types AS ENUM ('none','in','out','inout','horiz','back');");
        Execute.Sql("CREATE TYPE calculation_types AS ENUM ('none','calc201501', 'calc201711');");
        Execute.Sql("CREATE TYPE length_thresh_types AS ENUM ('none','rule', 'application');");
        Execute.Sql("CREATE TYPE file_modification_types AS ENUM ('created','modified');");
        Execute.Sql("CREATE TYPE unit_modes AS ENUM ('main','remote');");

        Create.Table("atdentry")
            .WithColumn("atdentryid").AsString(64).NotNullable().PrimaryKey()
            .WithColumn("filepath").AsString().NotNullable()
            .WithColumn("lastupdated").AsDateTime().Nullable()
            .WithColumn("filename").AsString().Nullable()
            .WithColumn("otdrsharedobjectspath").AsString().Nullable()
            .WithColumn("warnings").AsString().Nullable()
            .WithColumn("errormessage").AsString().Nullable()
            .WithColumn("errorcode").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("createdon").AsDateTime().NotNullable()
            .WithColumn("version").AsString().Nullable()
            .WithColumn("areallvalidolts").AsBoolean().NotNullable().WithDefaultValue(true);

        Create.Table("instrumentinfo")
            .WithColumn("id").AsInt64().NotNullable().PrimaryKey().Identity()
            .WithColumn("atdentryid").AsString(64).NotNullable().ForeignKey("atdentry", "atdentryid").OnDeleteOrUpdate(System.Data.Rule.Cascade)
            .WithColumn("caldate").AsDateTime().Nullable()
            .WithColumn("timestamp").AsDateTime().Nullable()
            .WithColumn("createdormodified").AsCustom("file_modification_types").Nullable()
            .WithColumn("instrumenttype").AsString().NotNullable()
            .WithColumn("key").AsString().NotNullable()
            .WithColumn("locatedat").AsString().NotNullable()
            .WithColumn("mainorremote").AsCustom("unit_modes").Nullable()
            .WithColumn("Model").AsString().NotNullable()
            .WithColumn("serialnumber").AsString().NotNullable();


        Create.Table("atdjob")
            .WithColumn("id").AsInt64().NotNullable().PrimaryKey().Identity()
            .WithColumn("atdentryid").AsString(64).NotNullable().ForeignKey("atdentry", "atdentryid").OnDeleteOrUpdate(System.Data.Rule.Cascade)
            .WithColumn("contractorname").AsString().Nullable()
            .WithColumn("operator").AsString().Nullable()
            .WithColumn("operator2").AsString().Nullable()
            .WithColumn("jobname").AsString().NotNullable()
            .WithColumn("customername").AsString().Nullable()
            .WithColumn("serializeid").AsInt32().Nullable();

        Create.Table("certrule")
            .WithColumn("id").AsInt64().NotNullable().PrimaryKey().Identity()
            .WithColumn("atdentryid").AsString(64).NotNullable().ForeignKey("atdentry", "atdentryid").OnDeleteOrUpdate(System.Data.Rule.Cascade)
            .WithColumn("maxlength").AsDouble().NotNullable().WithDefaultValue(0)
            .WithColumn("maxlengthunit").AsCustom("distance_unit_types").NotNullable()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("lossperconnector").AsFloat().Nullable()
            .WithColumn("lossper2ndconnector").AsFloat().Nullable()
            .WithColumn("lossper3rdconnector").AsFloat().Nullable()
            .WithColumn("lossper4thconnector").AsFloat().Nullable()
            .WithColumn("losspersplice").AsFloat().Nullable()
            .WithColumn("losspertestcordtotestcord").AsDouble().Nullable()
            .WithColumn("losspertestcordtofut").AsDouble().Nullable()
            .WithColumn("fibertype").AsCustom("test_port_types").NotNullable()
            .WithColumn("description").AsString().NotNullable()
            .WithColumn("losstype").AsCustom("rule_types").Nullable()
            .WithColumn("stdtype").AsCustom("standards_types").Nullable()
            .WithColumn("lossthreshtype").AsCustom("rule_loss_types").Nullable()
            .WithColumn("powerunits").AsCustom("power_units").Nullable()
            .WithColumn("cattype").AsCustom("category_types").Nullable()
            .WithColumn("actualfibertype").AsCustom("fiber_type_actual").Nullable()
            .WithColumn("usetype").AsCustom("use_types").Nullable()
            .WithColumn("calculationtouse").AsCustom("calculation_types").Nullable()
            .WithColumn("lengththreshtype").AsCustom("length_thresh_types").Nullable()
            ;

        Create.Table("lossthreshold")
            .WithColumn("id").AsInt64().NotNullable().PrimaryKey().Identity()
            .WithColumn("certruleid").AsInt64().NotNullable().ForeignKey("certrule", "id").OnDeleteOrUpdate(System.Data.Rule.Cascade)
            .WithColumn("minloss").AsDouble().Nullable()
            .WithColumn("maxloss").AsDouble().Nullable()
            .WithColumn("wavelength").AsInt32().Nullable();

    }
}
