using MediatR;
using Finora.Repositories.Interfaces;
using Finora.Messages.Wrappers;
using Finora.Services;
using Finora.Messages.Auth;

namespace Finora.Handlers;

public class LoginHandler(IUserRepository userRepository, IPasswordService passwordService) : IRequestHandler<LoginRequest, RabbitResponse<object>>
{
    public async Task<RabbitResponse<object>> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            return new RabbitResponse<object>
            {
                StatusCode = 404
            };
        }

        if (!passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return new RabbitResponse<object>
            {
                StatusCode = 401
            };
        }

        return new RabbitResponse<object> {
            Data = user,
            StatusCode = 200
        };
    }
}
