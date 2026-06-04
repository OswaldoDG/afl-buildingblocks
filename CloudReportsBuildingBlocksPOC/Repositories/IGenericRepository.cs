namespace CloudReportsBuildingBlocksPOC.Repositories;

using System.Data;

public interface IGenericRepository<T>
    where T : class
{
    // Read operations.
    Task<T?> GetByIdAsync(object id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> QueryAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    Task<T?> QueryFirstOrDefaultAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    // Individual write operations.
    Task<long> InsertAsync(T entity, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(T entity, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(object id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    // Bath operations for high volume scenarios.
    Task<int> BulkInsertAsync(IEnumerable<T> entities, IDbTransaction? transaction = null, int batchSize = 1000, CancellationToken cancellationToken = default);

    Task<int> BulkUpdateAsync(IEnumerable<T> entities, IDbTransaction? transaction = null, int batchSize = 1000, CancellationToken cancellationToken = default);

    Task<int> BulkDeleteAsync(IEnumerable<object> ids, IDbTransaction? transaction = null, int batchSize = 1000, CancellationToken cancellationToken = default);

    // Utility methods for executing raw SQL when necessary.
    Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}