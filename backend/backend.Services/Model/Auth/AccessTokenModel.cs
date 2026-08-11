namespace backend.Services.Model.Auth;

public class AccessTokenModel
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}
