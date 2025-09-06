using MediatR;
using Finora.Messages.Users;
using Finora.Repositories.Interfaces;
using Finora.Services;
using Mapster;

namespace Finora.Handlers;

public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, object>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public UpdateUserHandler(IUserRepository userRepository, IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task<object> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        // Get existing user
        var existingUser = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (existingUser == null)
        {
            throw new ArgumentException($"User with ID {request.Id} not found");
        }

        // Update user properties
        existingUser.FirstName = request.FirstName;
        existingUser.SecondName = request.SecondName;
        existingUser.LastName = request.LastName;
        existingUser.Email = request.Email;
        existingUser.DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Unspecified);
        
        // Hash password if provided
        if (!string.IsNullOrEmpty(request.Password))
        {
            existingUser.PasswordHash = _passwordService.HashPassword(request.Password);
            existingUser.PasswordSalt = string.Empty; // BCrypt includes salt in the hash
        }

        // Update user in repository
        await _userRepository.UpdateAsync(existingUser, cancellationToken);

        // Map back to DTO for response
        var userDto = existingUser.Adapt<UserDTO>();

        return new
        {
            Success = true,
            Message = "User updated successfully",
            Data = userDto
        };
    }
}
