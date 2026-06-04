namespace CloudReportsBuildingBlocksPOC.Repositories;

using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;

public class GenericRepository<T> : IGenericRepository<T>
    where T : class
{
    private readonly IDbConnection _connection;
    private readonly string _tableName;
    private readonly string _keyColumn;
    private readonly PropertyInfo[] _properties;
    private readonly PropertyInfo _keyProperty;

    public GenericRepository(IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        var type = typeof(T);

        // Get table name from [Table] attribute or default to pluralized lowercase type name.
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        _tableName = tableAttr?.Name ?? type.Name.ToLower() + "s";

        // Get properties.
        _properties = [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite)];
        _properties = _properties.Where(p => p.GetCustomAttribute<KeyAttribute>() != null || p.GetCustomAttribute<ColumnAttribute>() != null).ToArray();

        // Get key column.
        _keyProperty = _properties.FirstOrDefault(
            p => p.GetCustomAttribute<KeyAttribute>() != null)
            ?? _properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No key property found for type {type.Name}");

        var columnAttr = _keyProperty.GetCustomAttribute<ColumnAttribute>();
        _keyColumn = columnAttr?.Name ?? _keyProperty.Name.ToLower();
    }

    public async Task<T?> GetByIdAsync(object id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM {_tableName} WHERE {_keyColumn} = @Id";
        return await _connection.QueryFirstOrDefaultAsync<T>(
            new CommandDefinition(sql, new { Id = id }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<T>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM {_tableName}";
        return await _connection.QueryAsync<T>(
            new CommandDefinition(sql, transaction: transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<T>> QueryAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        return await _connection.QueryAsync<T>(
            new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken));
    }

    public async Task<T?> QueryFirstOrDefaultAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        return await _connection.QueryFirstOrDefaultAsync<T>(
            new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken));
    }

    public async Task<long> InsertAsync(T entity, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var insertableProps = _properties
            .Where(p => p != _keyProperty || !IsIdentityColumn(p))
            .ToArray();

        var columnNames = string.Join(", ", insertableProps.Select(GetColumnName));
        var paramNames = string.Join(", ", insertableProps.Select(p => $"@{p.Name}"));

        var sql = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({paramNames})";

        if (IsIdentityColumn(_keyProperty))
        {
            sql += $" RETURNING {_keyColumn}";
            long id = await _connection.ExecuteScalarAsync<long>(
                new CommandDefinition(sql, entity, transaction, cancellationToken: cancellationToken));

            _keyProperty.SetValue(entity, id);
            return id;
        }

        return await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(T entity, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var updateableProps = _properties.Where(p => p != _keyProperty).ToArray();
        var setClause = string.Join(", ", updateableProps.Select(p => $"{GetColumnName(p)} = @{p.Name}"));

        var sql = $"UPDATE {_tableName} SET {setClause} WHERE {_keyColumn} = @{_keyProperty.Name}";

        var rowsAffected = await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, transaction, cancellationToken: cancellationToken));

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(object id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var sql = $"DELETE FROM {_tableName} WHERE {_keyColumn} = @Id";

        var rowsAffected = await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, transaction, cancellationToken: cancellationToken));

        return rowsAffected > 0;
    }

    // High volume ops.
    public async Task<int> BulkInsertAsync(IEnumerable<T> entities, IDbTransaction? transaction = null, int batchSize = 1000, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        if (entityList.Count == 0)
        {
            return 0;
        }

        var insertableProps = _properties
            .Where(p => p != _keyProperty || !IsIdentityColumn(p))
            .ToArray();

        var columnNames = string.Join(", ", insertableProps.Select(GetColumnName));
        var totalInserted = 0;

        // Process in batches to optimize performance
        for (int i = 0; i < entityList.Count; i += batchSize)
        {
            var batch = entityList.Skip(i).Take(batchSize).ToList();

            // Use COPY for PostgreSQL (more efficient for large volumes)
            // Alternative: use multiple VALUES in a single INSERT
            var valueRows = new List<string>();
            var parameters = new DynamicParameters();

            for (int j = 0; j < batch.Count; j++)
            {
                var paramNames = new List<string>();
                foreach (var prop in insertableProps)
                {
                    var paramName = $"{prop.Name}{j}";
                    paramNames.Add($"@{paramName}");
                    parameters.Add(paramName, prop.GetValue(batch[j]));
                }
                valueRows.Add($"({string.Join(", ", paramNames)})");
            }

            var sql = $"INSERT INTO {_tableName} ({columnNames}) VALUES {string.Join(", ", valueRows)}";

            totalInserted += await _connection.ExecuteAsync(
                new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        return totalInserted;
    }

    public async Task<int> BulkUpdateAsync(IEnumerable<T> entities, IDbTransaction? transaction = null, int batchSize = 1000, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        if (entityList.Count == 0)
        {
            return 0;
        }

        var totalUpdated = 0;

        // Batch updates to reduce transaction overhead and improve performance
        for (int i = 0; i < entityList.Count; i += batchSize)
        {
            var batch = entityList.Skip(i).Take(batchSize);

            // Execute updates in parallel within the batch.
            var tasks = batch.Select(entity => UpdateAsync(entity, transaction, cancellationToken));
            var results = await Task.WhenAll(tasks);
            totalUpdated += results.Count(r => r);
        }

        return totalUpdated;
    }

    public async Task<int> BulkDeleteAsync(IEnumerable<object> ids, IDbTransaction? transaction = null, int batchSize = 1000, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return 0;
        }

        var totalDeleted = 0;

        for (int i = 0; i < idList.Count; i += batchSize)
        {
            var batch = idList.Skip(i).Take(batchSize).ToList();
            var sql = $"DELETE FROM {_tableName} WHERE {_keyColumn} = ANY(@Ids)";

            totalDeleted += await _connection.ExecuteAsync(
                new CommandDefinition(sql, new { Ids = batch.ToArray() }, transaction, cancellationToken: cancellationToken));
        }

        return totalDeleted;
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        return await _connection.ExecuteAsync(
            new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken));
    }

    public async Task<TResult?> ExecuteScalarAsync<TResult>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        return await _connection.ExecuteScalarAsync<TResult>(
            new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken));
    }

    // Helper methods.
    private static string GetColumnName(PropertyInfo property)
    {
        var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
        return columnAttr?.Name ?? property.Name.ToLower();
    }

    private static bool IsIdentityColumn(PropertyInfo property)
    {
        return property.GetCustomAttribute<DatabaseGeneratedAttribute>()?.DatabaseGeneratedOption 
            == DatabaseGeneratedOption.Identity;
    }
}