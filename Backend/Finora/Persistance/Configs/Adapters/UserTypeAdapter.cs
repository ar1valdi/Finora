using Finora.Models;
using Finora.Messages.Users;
using Mapster;

namespace Finora.Backend.Persistance.Configs.Adapters
{
    internal class UserTypeAdapter : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<User, UserDTO>()
                .Map(dest => dest.Id, src => src.Id.ToString());

            config.NewConfig<UserDTO, User>()
                .Map(dest => dest.DateOfBirth, src => DateTime.SpecifyKind(src.DateOfBirth, DateTimeKind.Unspecified))
                .Map(dest => dest.CreatedAt, src => DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified))
                .Map(dest => dest.Id, src => Guid.Parse(src.Id));
        }
    }
}
