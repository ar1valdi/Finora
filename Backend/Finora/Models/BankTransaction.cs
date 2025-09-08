using Finora.Kernel;
using Finora.Models.Enums;

namespace Finora.Models
{
    public class BankTransaction : EntityBase
    {
        public BankAccount From { get; set; } = new BankAccount();
        public Guid FromId { get; set; }
        public BankAccount? To { get; set; } = null;
        public Guid? ToId { get; set; }
        public TransactionType Type { get; set; } = TransactionType.Deposit;
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}