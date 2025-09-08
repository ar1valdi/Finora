using Finora.Kernel;
using Finora.Persistance.Contexts;
using Finora.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Finora.Repositories;

public class OutboxMessageRepository : BaseRepository<OutboxMessage>, IOutboxMessageRepository {
    public OutboxMessageRepository(FinoraDbContext context) : base(context) {
    }

    public async Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(CancellationToken cancellationToken)
    {
        return await _dbSet.Where(x => x.Status == OutboxMessageStatus.Pending).ToListAsync(cancellationToken);
    }

    public async Task<bool> IsInOutbox(Guid correlationId, CancellationToken ct)
    {
        return await _dbSet.AnyAsync(x => x.CorrelationId.Equals(correlationId));
    }
}