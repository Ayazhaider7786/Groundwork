namespace backend.Services.Model.SeedData;

/// <summary>
/// One account the seeder creates on a fresh database. The password lives in
/// user-secrets, not in a committed appsettings file.
/// </summary>
public class SeedUserOptions
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
}
