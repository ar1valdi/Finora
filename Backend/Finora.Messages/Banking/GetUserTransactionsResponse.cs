using Finora.Messages.Interfaces;

namespace Finora.Messages.Banking;

public class GetUserTransactionsResponse : IMessage
{
    public int Total { get; set; }
    public IEnumerable<UserTransactionDTO> Items { get; set; } = Enumerable.Empty<UserTransactionDTO>();
}

public class UserTransactionDTO
{
    public string Id { get; set; } = string.Empty;
    public string FromBankAccountId { get; set; } = string.Empty;
    public string ToBankAccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
}


