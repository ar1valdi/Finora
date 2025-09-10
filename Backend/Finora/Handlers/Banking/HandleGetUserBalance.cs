using MediatR;
using Finora.Messages.Banking;
using Finora.Messages.Wrappers;
using Finora.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Finora.Handlers.Banking;

public class HandleGetUserBalance(
    IBankAccountRepository _bankAccountRepository,
    IUserRepository _userRepository,
    ILogger<HandleGetUserBalance> _logger
) : IRequestHandler<GetUserBalanceRequest, RabbitResponse<object>>
{
    public async Task<RabbitResponse<object>> Handle(GetUserBalanceRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting balance for user: {UserId}", request.UserId);

        if (!Guid.TryParse(request.UserId, out var userId))
        {
            _logger.LogWarning("Invalid UserId format: {UserId}", request.UserId);
            return new RabbitResponse<object> 
            { 
                StatusCode = 400,
                Errors = ["Invalid user ID format"]
            };
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return new RabbitResponse<object>
            {
                StatusCode = 404,
                Errors = ["User not found"]
            };
        }

        if (user.BankAccountId == null)
        {
            _logger.LogWarning("User has no associated bank account: {UserId}", userId);
            return new RabbitResponse<object>
            {
                StatusCode = 404,
                Errors = ["User has no associated bank account"]
            };
        }

        var bankAccount = await _bankAccountRepository.GetByIdAsync(
            user.BankAccountId.Value,
            cancellationToken);

        if (bankAccount == null)
        {
            _logger.LogWarning("Bank account not found for user: {UserId}", userId);
            return new RabbitResponse<object>
            {
                StatusCode = 404,
                Errors = ["Bank account not found for this user"]
            };
        }

        var response = new GetUserBalanceResponse
        {
            UserId = userId.ToString(),
            BankAccountId = bankAccount.Id.ToString(),
            Balance = bankAccount.Balance,
            IsClosed = bankAccount.IsClosed
        };

        _logger.LogInformation("Successfully retrieved balance for user {UserId}: {Balance}", 
            userId, bankAccount.Balance);

        return new RabbitResponse<object>
        {
            StatusCode = 200,
            Data = response
        };
    }
}
