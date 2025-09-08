using Finora.Models;
using Finora.Persistance.Contexts;
using Finora.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Finora.Repositories;

public class BankTransactionRepository : BaseRepository<BankTransaction>, IBankTransactionRepository
{
    public BankTransactionRepository(FinoraDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<BankTransaction> Items, int Total)> GetPagedByAccountIdsAsync(IEnumerable<Guid> accountIds, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var ids = accountIds.ToArray();
        var query = _dbSet
            .AsQueryable()
            .Where(t => ids.Contains(t.FromId) || (t.ToId.HasValue && ids.Contains(t.ToId.Value)));

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(t => t.From)
            .Include(t => t.To)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}


