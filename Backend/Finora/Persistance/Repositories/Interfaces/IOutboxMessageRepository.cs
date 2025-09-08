using Finora.Kernel;

namespace Finora.Repositories.Interfaces;

public interface IOutboxMessageRepository : IBaseRepository<OutboxMessage> {
    Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(CancellationToken cancellationToken);
    Task<bool> IsInOutbox(Guid correlationId, CancellationToken ct);
}