using backend.SeedData.Seeders;
using Microsoft.Extensions.Logging;

namespace backend.SeedData;

/// <summary>
/// Runs the seeders in dependency order. The only entry point startup calls —
/// individual seeders are never invoked directly from Program.cs.
/// </summary>
public class DatabaseSeeder(
    RoleSeeder roleSeeder,
    UserSeeder userSeeder,
    ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync()
    {
        logger.LogInformation("Seeding database.");

        // Roles must exist before a user can be assigned one.
        await roleSeeder.SeedAsync();
        await userSeeder.SeedAsync();

        logger.LogInformation("Database seeding complete.");
    }
}
