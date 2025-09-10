using Finora.Models;

namespace Finora.Repositories.Interfaces;

public interface IBankAccountRepository : IBaseRepository<BankAccount>
{
    Task<BankAccount?> GetByUserId(Guid id, CancellationToken ct);
}