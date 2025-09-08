using Finora.Models;

namespace Finora.Repositories.Interfaces;

public interface IBankTransactionRepository : IBaseRepository<BankTransaction>
{
    Task<(IEnumerable<BankTransaction> Items, int Total)> GetPagedByAccountIdsAsync(IEnumerable<Guid> accountIds, int page, int pageSize, CancellationToken cancellationToken = default);
}


