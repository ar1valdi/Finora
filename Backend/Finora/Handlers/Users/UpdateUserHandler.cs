using MediatR;
using Finora.Messages.Users;
using Finora.Repositories.Interfaces;
using Finora.Services;
using Mapster;
using Finora.Messages.Wrappers;

namespace Finora.Handlers;

public class UpdateUserHandler(
    IUserRepository _userRepository, 
    IPasswordService _passwordService
    ) : IRequestHandler<UpdateUserRequest, RabbitResponse<object>>
{
    public async Task<RabbitResponse<object>> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (existingUser == null)
        {
            return new RabbitResponse<object>
            {
                StatusCode = 404
            };
        }

        existingUser.FirstName = string.IsNullOrEmpty(request.FirstName) ? existingUser.FirstName : request.FirstName;
        existingUser.SecondName =  existingUser.SecondName;
        existingUser.LastName = string.IsNullOrEmpty(request.LastName) ? existingUser.LastName : request.LastName;
        existingUser.Email = string.IsNullOrEmpty(request.Email) ? existingUser.Email : request.Email;
        existingUser.DateOfBirth = request.DateOfBirth is null ? existingUser.DateOfBirth : DateTime.SpecifyKind(request.DateOfBirth.Value, DateTimeKind.Unspecified);
        
        if (!string.IsNullOrEmpty(request.Password))
        {
            existingUser.PasswordHash = _passwordService.HashPassword(request.Password);
        }

        await _userRepository.UpdateAsync(existingUser, cancellationToken);

        return new RabbitResponse<object>  
        {
            Data = existingUser.Adapt<UserDTO>(),
            StatusCode = 200
        };
    }
}
