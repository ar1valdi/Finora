using Finora.Models;
using Finora.Persistance.Contexts;
using Finora.Repositories.Interfaces;

namespace Finora.Repositories;

public class BankAccountRepository : BaseRepository<BankAccount>, IBankAccountRepository
{
    public BankAccountRepository(FinoraDbContext context) : base(context)
    {
    }
}