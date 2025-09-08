using MediatR;
using Finora.Messages.Users;
using Finora.Repositories.Interfaces;
using Finora.Models;
using Finora.Services;
using Mapster;
using Finora.Messages.Wrappers;

namespace Finora.Handlers;

public class AddUserHandler : IRequestHandler<AddUserRequest, RabbitResponse<object>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public AddUserHandler(IUserRepository userRepository, IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task<RabbitResponse<object>> Handle(AddUserRequest request, CancellationToken cancellationToken)
    {
        var hashedPassword = _passwordService.HashPassword(request.Password);

        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
            Balance = 0,
            IsClosed = false,
        };

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
            BankAccount = account,
            BankAccountId = account.Id,
        };

        var addedUser = await _userRepository.AddAsync(user, cancellationToken);
        var userDto = addedUser.Adapt<UserDTO>();

        return new RabbitResponse<object>
        {
            Data = userDto,
            StatusCode = 201
        };
    }
}
