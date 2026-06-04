namespace GcfOtdrParser;

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    public IDbTransaction Transaction { get; }

    IGenericRepository<T> Repository<T>()
        where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}