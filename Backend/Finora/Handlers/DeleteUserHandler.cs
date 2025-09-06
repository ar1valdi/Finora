using MediatR;
using Finora.Messages.Users;
using Finora.Repositories.Interfaces;
using Finora.Models;

namespace Finora.Handlers;

public class DeleteUserHandler : IRequestHandler<DeleteUserRequest, object>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<object> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        // Parse the ID from string to Guid
        if (!Guid.TryParse(request.Id, out var userId))
        {
            throw new ArgumentException($"Invalid user ID format: {request.Id}");
        }

        // Get existing user
        var existingUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        if (existingUser == null)
        {
            throw new ArgumentException($"User with ID {request.Id} not found");
        }

        // Delete user from repository
        await _userRepository.DeleteAsync(existingUser, cancellationToken);

        return new
        {
            Success = true,
            Message = "User deleted successfully",
            Data = new { Id = request.Id }
        };
    }
}
