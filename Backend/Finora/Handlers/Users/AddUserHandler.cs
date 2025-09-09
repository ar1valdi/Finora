using MediatR;
using Finora.Messages.Users;
using Finora.Repositories.Interfaces;
using Finora.Models;
using Finora.Services;
using Mapster;
using Finora.Messages.Wrappers;
using Microsoft.Extensions.Options;
using Finora.Web.Configuration;

namespace Finora.Handlers;

public class AddUserHandler(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IMailingService mailingService,
    IOptions<MailingConfiguration> configuration
) : IRequestHandler<AddUserRequest, RabbitResponse<object>>
{
    public async Task<RabbitResponse<object>> Handle(AddUserRequest request, CancellationToken cancellationToken)
    {
        var hashedPassword = passwordService.HashPassword(request.Password);

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

        var addedUser = await userRepository.AddAsync(user, cancellationToken);
        var userDto = addedUser.Adapt<UserDTO>();

        mailingService.SendEmailAtJobHandled(
            request.Email,
            configuration.Value.Templates["AccountCreated"].Subject,
            configuration.Value.Templates["AccountCreated"].Body.Replace("{fullName}", user.FullName).Replace("{bankAccountId}", user.BankAccountId.ToString()),
            Guid.NewGuid(),
            cancellationToken
        );

        return new RabbitResponse<object>
        {
            Data = userDto,
            StatusCode = 201
        };
    }
}