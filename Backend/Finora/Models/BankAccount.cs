using Finora.Kernel;

namespace Finora.Models
{
    public class BankAccount : EntityBase
    {
        public User User { get; set; } = new User();
        public bool IsClosed { get; set; } = false;
        public decimal Balance { get; set; } = 0;
        public List<BankTransaction> IncomingTransactions { get; set; } = new List<BankTransaction>();
        public List<BankTransaction> OutgoingTransactions { get; set; } = new List<BankTransaction>();
    }
}
