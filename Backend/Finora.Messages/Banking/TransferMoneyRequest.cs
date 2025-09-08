using Finora.Messages.Interfaces;

namespace Finora.Messages.Banking;

public class TransferMoneyRequest : ICommand
{
    public string FromBankAccountId { get; set; } = string.Empty;
    public string ToBankAccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}


