namespace GcfOtdrParser;

using Npgsql;
using System;
using System.Data;

public class DapperContext
{
    private const string OTDRCONNSTRING = "OTDRCONNSTRING";

    public IDbConnection CreateConnection()
        => new NpgsqlConnection(GetSecretValue(OTDRCONNSTRING));

    private static string GetSecretValue(string key)
    {
        switch (key)
        {
            case OTDRCONNSTRING:
                return Environment.GetEnvironmentVariable("OTDRCONNSTRING") ?? string.Empty;
            default:
                return string.Empty;
        }
    }
}
