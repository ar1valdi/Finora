using Finora.Models;
using Finora.Persistance.Contexts;
using Finora.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Finora.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(FinoraDbContext context) : base(context)
    {
    }
    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(u => u.Email == email);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }
}
