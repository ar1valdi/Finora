using Finora.Messages.Interfaces;
using Finora.Messages.Wrappers;
using MediatR;

namespace Finora.Messages.Banking;

public class GetUserTransactionsRequest : IQuery
{
    public string UserId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}


