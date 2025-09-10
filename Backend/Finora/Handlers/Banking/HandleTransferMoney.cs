using MediatR;
using Finora.Messages.Banking;
using Finora.Messages.Wrappers;
using Finora.Repositories.Interfaces;
using Finora.Models.Enums;

namespace Finora.Handlers;

public class HandleTransferMoney(
    IBankAccountRepository _bankAccountRepository,
    IBankTransactionRepository _bankTransactionRepository
) : IRequestHandler<TransferMoneyRequest, RabbitResponse<object>>
{
    public async Task<RabbitResponse<object>> Handle(TransferMoneyRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.FromBankAccountId, out var fromId) || !Guid.TryParse(request.ToBankAccountId, out var toId))
        {
            return new RabbitResponse<object> { StatusCode = 400 };
        }

        if (fromId == toId || request.Amount <= 0)
        {
            return new RabbitResponse<object> { StatusCode = 400 };
        }

        var from = await _bankAccountRepository.GetByIdAsync(fromId, cancellationToken);
        var to = await _bankAccountRepository.GetByIdAsync(toId, cancellationToken);

        if (from == null || to == null)
        {
            return new RabbitResponse<object> { StatusCode = 404 };
        }

        await _bankTransactionRepository.AddAsync(new Models.BankTransaction
        {
            From = from,
            FromId = from.Id,
            To = to,
            ToId = to.Id,
            Amount = request.Amount,
            TransactionDate = DateTime.Now,
            Description = request.Description ?? string.Empty,
            Type = TransactionType.Transfer,
        }, cancellationToken);

        return new RabbitResponse<object> { StatusCode = 200 };
    }
}


