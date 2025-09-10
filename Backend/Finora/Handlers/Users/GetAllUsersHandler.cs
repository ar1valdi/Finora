using MediatR;
using Finora.Messages.Users;
using Finora.Repositories.Interfaces;
using Mapster;
using Finora.Messages.Wrappers;

namespace Finora.Handlers;

public class GetAllUsersHandler(
    IUserRepository _userRepository
) : IRequestHandler<GetAllUsersRequest, RabbitResponse<object>>
{
    public async Task<RabbitResponse<object>> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
        var (users, totalCount) = await _userRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken: cancellationToken);

        return new RabbitResponse<object>
        {
            Data = new Paginated<UserDTO>
            {
                Data = users.Adapt<List<UserDTO>>(),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            },
            StatusCode = 200
        };
    }
}
