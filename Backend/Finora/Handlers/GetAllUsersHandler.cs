using MediatR;
using Finora.Messages.Users;
using Finora.Repositories.Interfaces;
using Mapster;

namespace Finora.Handlers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersRequest, object>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<object> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
        var (users, totalCount) = await _userRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken: cancellationToken);

        return new Paginated<UserDTO>
        {
            Data = users.Adapt<List<UserDTO>>(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
