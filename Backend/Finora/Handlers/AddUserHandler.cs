using MediatR;
using Finora.Messages.Users;
using Finora.Repositories.Interfaces;
using Finora.Models;
using Finora.Services;
using Mapster;

namespace Finora.Handlers;

public class AddUserHandler : IRequestHandler<AddUserRequest, object>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public AddUserHandler(IUserRepository userRepository, IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task<object> Handle(AddUserRequest request, CancellationToken cancellationToken)
    {
        // Hash the password
        var hashedPassword = _passwordService.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
            FirstName = request.FirstName,
            SecondName = request.SecondName,
            LastName = request.LastName,
            Email = request.Email,
            DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Unspecified),
            PasswordHash = hashedPassword,
            IsDeleted = false,
        };


        var addedUser = await _userRepository.AddAsync(user, cancellationToken);
        var userDto = addedUser.Adapt<UserDTO>();

        return new
        {
            Success = true,
            Message = "User added successfully",
            Data = userDto
        };
    }
}
