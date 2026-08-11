using backend.Data.Constants;
using backend.Data.Entities;
using backend.Services.Model.SeedData;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.SeedData.Seeders;

/// <summary>
/// Creates the configured accounts so a fresh database can be signed into.
/// Idempotent — an existing account is left untouched apart from confirming its
/// role assignment.
/// </summary>
public class UserSeeder(
    UserManager<ApplicationUser> userManager,
    IOptions<SeedDataOptions> options,
    ILogger<UserSeeder> logger)
{
    private readonly SeedDataOptions _options = options.Value;

    public async Task SeedAsync()
    {
        if (_options.Users.Count == 0)
        {
            logger.LogWarning(
                "No seed users are configured ({Section}:Users); skipping user seeding.",
                SeedDataOptions.SectionName);
            return;
        }

        foreach (var seedUser in _options.Users)
        {
            await SeedUserAsync(seedUser);
        }
    }

    private async Task SeedUserAsync(SeedUserOptions seedUser)
    {
        if (string.IsNullOrWhiteSpace(seedUser.Email) || string.IsNullOrWhiteSpace(seedUser.Password))
        {
            logger.LogWarning("A configured seed user has no email or no password; skipping it.");
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(seedUser.Email);
        if (existingUser is not null)
        {
            await EnsureAdminRoleAsync(existingUser);
            return;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = seedUser.Email,
            Email = seedUser.Email,
            EmailConfirmed = true,
            FullName = string.IsNullOrWhiteSpace(seedUser.FullName) ? "Administrator" : seedUser.FullName,
            IsActive = true,
        };

        var creationResult = await userManager.CreateAsync(user, seedUser.Password);
        if (!creationResult.Succeeded)
        {
            logger.LogError(
                "Could not create the seed user {Email}: {Errors}",
                seedUser.Email,
                string.Join("; ", creationResult.Errors.Select(error => error.Description)));
            return;
        }

        logger.LogInformation("Created the seed user {Email}.", seedUser.Email);
        await EnsureAdminRoleAsync(user);
    }

    private async Task EnsureAdminRoleAsync(ApplicationUser user)
    {
        if (await userManager.IsInRoleAsync(user, RoleNames.Admin))
        {
            return;
        }

        var result = await userManager.AddToRoleAsync(user, RoleNames.Admin);

        if (result.Succeeded)
        {
            logger.LogInformation("Assigned the {Role} role to {Email}.", RoleNames.Admin, user.Email);
        }
        else
        {
            logger.LogError(
                "Could not assign the {Role} role to {Email}: {Errors}",
                RoleNames.Admin,
                user.Email,
                string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }
}
