using Finora.Messages.Interfaces;

namespace Finora.Messages.Banking;

public class GetUserBalanceRequest : IQuery
{
    public string UserId { get; set; } = string.Empty;
}
