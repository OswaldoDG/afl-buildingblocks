namespace CloudReportsBuildingBlocksPOC.Repositories;

using Dapper;

public class Database
{
    public const string SorsDatabaseName = "sorsdb";
    public const string SorsConnectionName = "sors";
    public const string MasterConnectionName = "master";

    private readonly DapperContext _context;

    public Database(DapperContext context)
    {
        _context = context;
    }

    public void CreateDatabase(string dbName)
    {
        //var query = "SELECT * FROM sys.databases WHERE name = @name";
        //var parameters = new DynamicParameters();
        //parameters.Add("name", dbName);
        //using (var connection = _context.CreateMasterConnection())
        //{
        //    var records = connection.Query(query, parameters);
        //    if (!records.Any())
        //    {
        //      connection.Execute($"CREATE DATABASE {dbName}");
        //    }
        //}
    }
}
