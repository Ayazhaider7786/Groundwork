using backend.Data.Constants;
using backend.Services.Model.SeedData;
using Microsoft.Extensions.Options;

namespace backend.Services.Services.SeedData;

public interface ISeedDataService
{
    IReadOnlyList<SeedUserModel> GetSeedUsers();
}

/// <summary>
/// Reports the accounts the seeder created, passwords included, so the login screen
/// can offer them.
///
/// This reads the same configuration the seeder does rather than a database table,
/// which is why no plaintext password is ever persisted: Identity stores only
/// hashes, and the one copy of the password stays in user-secrets. Exposure is the
/// caller's responsibility — SeedDataController hard-gates this to Development.
/// </summary>
public class SeedDataService(IOptions<SeedDataOptions> options) : ISeedDataService
{
    private readonly SeedDataOptions _options = options.Value;

    public IReadOnlyList<SeedUserModel> GetSeedUsers() =>
    [
        .. _options.Users
            .Where(user => !string.IsNullOrWhiteSpace(user.Email) && !string.IsNullOrWhiteSpace(user.Password))
            .Select(user => new SeedUserModel
            {
                FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName,
                Email = user.Email,
                Password = user.Password,

                // Admin is the only role that exists; every account administers its own data.
                Role = RoleNames.Admin,
            }),
    ];
}
