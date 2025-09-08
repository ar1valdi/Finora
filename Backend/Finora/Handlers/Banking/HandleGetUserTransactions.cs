using MediatR;
using Finora.Messages.Banking;
using Finora.Messages.Wrappers;
using Finora.Repositories.Interfaces;

namespace Finora.Handlers;

public class HandleGetUserTransactions : IRequestHandler<GetUserTransactionsRequest, RabbitResponse<object>>
{
    private readonly IBankTransactionRepository _bankTransactionRepository;
    private readonly IBankAccountRepository _bankAccountRepository;

    public HandleGetUserTransactions(IBankTransactionRepository bankTransactionRepository, IBankAccountRepository bankAccountRepository)
    {
        _bankTransactionRepository = bankTransactionRepository;
        _bankAccountRepository = bankAccountRepository;
    }

    public async Task<RabbitResponse<object>> Handle(GetUserTransactionsRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            return new RabbitResponse<object> { StatusCode = 400 };
        }

        var accounts = await _bankAccountRepository.GetAllAsync(a => a.User.Id == userId, cancellationToken);
        var accountIds = accounts.Select(a => a.Id).ToHashSet();

        if (!accountIds.Any())
        {
            return new RabbitResponse<object> { StatusCode = 200, Data = new GetUserTransactionsResponse { Total = 0 } };
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (transactions, total) = await _bankTransactionRepository.GetPagedByAccountIdsAsync(accountIds, page, pageSize, cancellationToken);

        var items = transactions
            .Select(t => new UserTransactionDTO
            {
                Id = t.Id.ToString(),
                FromBankAccountId = t.FromId.ToString(),
                ToBankAccountId = t.ToId?.ToString() ?? string.Empty,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate,
                Description = t.Description
            })
            .ToList();

        return new RabbitResponse<object>
        {
            StatusCode = 200,
            Data = new GetUserTransactionsResponse
            {
                Total = total,
                Items = items
            }
        };
    }
}


