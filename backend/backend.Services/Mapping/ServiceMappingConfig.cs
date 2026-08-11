using backend.Data.Entities;
using backend.Services.Model.Auth;
using Mapster;

namespace backend.Services.Mapping;

/// <summary>
/// Entity to service-model mappings. Only registers what Mapster cannot infer —
/// identical property names map themselves.
/// </summary>
public class ServiceMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ApplicationUser, UserModel>()
            // IdentityUser.Email is nullable; UserModel.Email is not.
            .Map(destination => destination.Email, source => source.Email ?? string.Empty)
            // Roles come from UserManager, not from the entity — the service fills them in.
            .Ignore(destination => destination.Roles);
    }
}
