namespace backend.Services.Model.SeedData;

/// <summary>
/// A seeded account offered to the login screen so a developer can sign in without
/// hunting for credentials. Carries the plaintext password by design — see
/// SeedDataService for why that is safe only in Development.
/// </summary>
public class SeedUserModel
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}
