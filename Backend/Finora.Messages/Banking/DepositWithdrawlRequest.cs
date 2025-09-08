using Finora.Messages.Interfaces;

namespace Finora.Messages.Banking;

public class DepositWithdrawlRequest : ICommand
{
    public string BankAccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}