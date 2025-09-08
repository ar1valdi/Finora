using MediatR;
using Finora.Repositories.Interfaces;
using Finora.Messages.Wrappers;
using Finora.Messages.Banking;
using Finora.Models;
using Finora.Models.Enums;

namespace Finora.Handlers;

public class HandleDepositWithdrawl(
    IBankAccountRepository _bankAccountRepository,
    IBankTransactionRepository _bankTransactionRepository
    ) : IRequestHandler<DepositWithdrawlRequest, RabbitResponse<object>>
{

    public async Task<RabbitResponse<object>> Handle(DepositWithdrawlRequest request, CancellationToken cancellationToken)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAsync(Guid.Parse(request.BankAccountId), cancellationToken);
        
        if (bankAccount == null)
        {
            return new RabbitResponse<object> { StatusCode = 404 };
        }

        bankAccount.Balance += request.Amount;

        await _bankTransactionRepository.AddAsync(new BankTransaction
        {
            From = bankAccount,
            FromId = bankAccount.Id,
            Amount = request.Amount,
            TransactionDate = DateTime.Now,
            Description = request.Amount > 0 ? "Deposit" : "Withdrawal",
            Type = TransactionType.Deposit,
        }, cancellationToken);

        await _bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
        
        return new RabbitResponse<object> { StatusCode = 200 };
    }
}
