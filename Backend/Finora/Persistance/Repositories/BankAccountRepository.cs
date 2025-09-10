using Finora.Models;
using Finora.Persistance.Contexts;
using Finora.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Finora.Repositories;

public class BankAccountRepository : BaseRepository<BankAccount>, IBankAccountRepository
{
    public BankAccountRepository(FinoraDbContext context) : base(context)
    {
    }

    public Task<BankAccount?> GetByUserId(Guid id, CancellationToken ct)
    {
        return _dbSet.Where(x => x.Id == id).FirstOrDefaultAsync(ct);
    }
}