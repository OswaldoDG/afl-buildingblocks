namespace CloudReportsBuildingBlocksPOC.Repositories;

using Npgsql;
using System.Data;

public class DapperContext
{
    private readonly IConfiguration _configuration;

    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection()
        => new NpgsqlConnection(_configuration.GetConnectionString(Database.SorsConnectionName));

    public IDbConnection CreateMasterConnection()
    {
        var cn = _configuration.GetConnectionString(Database.MasterConnectionName);
        return new NpgsqlConnection(cn);
    }
}
