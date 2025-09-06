using Finora.Messages.Interfaces;

namespace Finora.Messages.Users;

public class DeleteUserRequest : ICommand
{
    public string Id { get; set; } = string.Empty;
}