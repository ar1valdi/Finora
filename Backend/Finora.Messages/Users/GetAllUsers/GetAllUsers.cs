using Finora.Messages.Interfaces;

namespace Finora.Messages.Users.GetAllUsers
{
    internal class GetAllUsers : IQuery
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
