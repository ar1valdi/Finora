using MediatR;
using Finora.Messages.Users;
using Finora.Repositories.Interfaces;
using Mapster;
using Finora.Messages.Wrappers;

namespace Finora.Handlers;

public class GetUserHandler : IRequestHandler<GetUserRequest, RabbitResponse<object>>
{
    private readonly IUserRepository _userRepository;

    public GetUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<RabbitResponse<object>> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(request.Id), cancellationToken);
        
        if (user == null)
        {
            return new RabbitResponse<object>
            {
                StatusCode = 404
            };
        }
        
        return new RabbitResponse<object>
        {
            Data = user.Adapt<UserDTO>(),
            StatusCode = 200
        };
    }
}
