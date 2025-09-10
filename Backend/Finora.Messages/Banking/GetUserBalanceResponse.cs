namespace Finora.Messages.Banking;

public class GetUserBalanceResponse
{
    public string UserId { get; set; } = string.Empty;
    public string BankAccountId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public bool IsClosed { get; set; }
}
