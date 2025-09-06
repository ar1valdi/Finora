using Finora.Models;

namespace Finora.Repositories.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
}
