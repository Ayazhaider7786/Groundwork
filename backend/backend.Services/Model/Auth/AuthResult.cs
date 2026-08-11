namespace backend.Services.Model.Auth;

/// <summary>
/// Auth operations fail for several distinct business reasons, so they report a
/// flag and a message list rather than throwing. Payload fields are meaningful
/// only when <see cref="Succeeded"/> is true.
/// </summary>
public class AuthResult
{
    public bool Succeeded { get; set; }

    public IReadOnlyList<string> Errors { get; set; } = [];

    public string AccessToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAt { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    public UserModel? User { get; set; }

    public static AuthResult Fail(params string[] errors) =>
        new() { Succeeded = false, Errors = errors };

    public static AuthResult Success(AccessTokenModel accessToken, string refreshToken, UserModel user) =>
        new()
        {
            Succeeded = true,
            AccessToken = accessToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshToken = refreshToken,
            User = user,
        };
}
