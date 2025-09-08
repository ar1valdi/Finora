using Microsoft.EntityFrameworkCore.Storage;
using Finora.Kernel;

namespace Finora.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
