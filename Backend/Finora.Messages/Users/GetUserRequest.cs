using Finora.Messages.Interfaces;

namespace Finora.Messages.Users;

public class GetUserRequest : IQuery
{
    public string Id { get; set; } = string.Empty;
}
