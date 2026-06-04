namespace GcfOtdrParser;

using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private readonly Dictionary<Type, object> _repositories = [];
    private IDbTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        _connection.Open();
    }

    public IDbTransaction? Transaction { get { return _transaction; } }

    public IGenericRepository<T> Repository<T>()
        where T : class
    {
        var type = typeof(T);

        if (!_repositories.TryGetValue(type, out object? value))
        {
            value = new GenericRepository<T>(_connection);
            _repositories[type] = value;
        }

        return (IGenericRepository<T>)value;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // There is no tracking in Dapper ops are executed immediately, so this is a no-op.
        return Task.FromResult(0);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction?.Dispose();
        _transaction = _connection.BeginTransaction();
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
          throw new InvalidOperationException("No active transaction to commit.");
        }

        _transaction.Commit();
        _transaction.Dispose();
        _transaction = null;
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
          throw new InvalidOperationException("No active transaction to rollback.");
        }

        _transaction.Rollback();
        _transaction.Dispose();
        _transaction = null;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
          return;
        }

        await DisposeAsyncCore().ConfigureAwait(false);

        _disposed = true;

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Dispose managed resources
            _transaction?.Dispose();
            _connection?.Dispose();
        }

        // Dispose unmanaged resources (if any) here.
        _disposed = true;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_transaction is not null)
        {
            _transaction.Dispose();
        }

        if (_connection is IAsyncDisposable asyncDisposableConnection)
        {
            await asyncDisposableConnection.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _connection?.Dispose();
        }
    }
}