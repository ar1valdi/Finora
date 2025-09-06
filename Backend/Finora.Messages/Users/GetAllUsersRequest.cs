using Finora.Messages.Interfaces;

namespace Finora.Messages.Users;

public class GetAllUsersRequest : IQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}