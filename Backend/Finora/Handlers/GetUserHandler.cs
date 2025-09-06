using MediatR;
using Finora.Messages.Users;
using Finora.Repositories.Interfaces;
using Mapster;

namespace Finora.Handlers;

public class GetUserHandler : IRequestHandler<GetUserRequest, object>
{
    private readonly IUserRepository _userRepository;

    public GetUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<object> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(request.Id), cancellationToken);
        
        if (user == null)
        {
            throw new ArgumentException($"User with ID {request.Id} not found");
        }
        
        return user.Adapt<UserDTO>();
    }
}
