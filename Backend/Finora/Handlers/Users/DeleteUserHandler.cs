using MediatR;
using Finora.Messages.Users;
using Finora.Repositories.Interfaces;
using Finora.Models;
using Finora.Messages.Wrappers;

namespace Finora.Handlers;

public class DeleteUserHandler(IUserRepository _userRepository) : IRequestHandler<DeleteUserRequest, RabbitResponse<object>>
{
    public async Task<RabbitResponse<object>> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var userId))
        {
            throw new ArgumentException($"Invalid user ID format: {request.Id}");
        }

        var existingUser = await _userRepository.GetByIdWithBankAccountAsync(userId, cancellationToken);
        
        if (existingUser == null)
        {
            return new RabbitResponse<object>
            {
                StatusCode = 404
            };
        }

        existingUser.IsDeleted = true;
        if (existingUser.BankAccount != null)
        {
            existingUser.BankAccount.IsClosed = true;
        }
        await _userRepository.UpdateAsync(existingUser, cancellationToken);

        return new RabbitResponse<object>
        {
            StatusCode = 204
        };
    }
}
