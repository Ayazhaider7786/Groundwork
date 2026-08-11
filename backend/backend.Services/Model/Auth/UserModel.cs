namespace backend.Services.Model.Auth;

public class UserModel
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public IReadOnlyList<string> Roles { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}
