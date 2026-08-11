using backend.Data.Constants;
using backend.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace backend.SeedData.Seeders;

/// <summary>
/// Creates the Identity roles listed in <see cref="RoleNames.All"/>. Idempotent —
/// running it again on a seeded database is a no-op.
/// </summary>
public class RoleSeeder(RoleManager<ApplicationRole> roleManager, ILogger<RoleSeeder> logger)
{
    public async Task SeedAsync()
    {
        foreach (var roleName in RoleNames.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new ApplicationRole(roleName));

            if (result.Succeeded)
            {
                logger.LogInformation("Created the {Role} role.", roleName);
            }
            else
            {
                logger.LogError(
                    "Could not create the {Role} role: {Errors}",
                    roleName,
                    string.Join("; ", result.Errors.Select(error => error.Description)));
            }
        }
    }
}
