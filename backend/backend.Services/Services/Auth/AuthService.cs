using backend.Data.Abstractions;
using backend.Data.Constants;
using backend.Data.Data;
using backend.Data.Entities;
using backend.Services.Model.Auth;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Services.Services.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default);

    Task<AuthResult> LoginAsync(LoginModel model, CancellationToken cancellationToken = default);

    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<UserModel?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext dbContext,
    IJwtTokenService jwtTokenService,
    ICurrentUserProvider currentUserProvider,
    IOptions<JwtOptions> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResult> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default)
    {
        var existingUser = await userManager.FindByEmailAsync(model.Email);
        if (existingUser is not null)
        {
            logger.LogWarning("Registration rejected: email {Email} is already registered.", model.Email);
            return AuthResult.Fail("An account with this email address already exists.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            IsActive = true,
        };

        var creationResult = await userManager.CreateAsync(user, model.Password);
        if (!creationResult.Succeeded)
        {
            logger.LogWarning("Registration failed for {Email}: {Errors}", model.Email, DescribeErrors(creationResult));
            return AuthResult.Fail([.. creationResult.Errors.Select(error => error.Description)]);
        }

        // Every account holder administers their own account — there is no other role.
        var roleResult = await userManager.AddToRoleAsync(user, RoleNames.Admin);
        if (!roleResult.Succeeded)
        {
            logger.LogError("Could not assign the {Role} role to {Email}: {Errors}", RoleNames.Admin, model.Email, DescribeErrors(roleResult));
            return AuthResult.Fail([.. roleResult.Errors.Select(error => error.Description)]);
        }

        logger.LogInformation("Registered new user {UserId}.", user.Id);
        return await BuildAuthResultAsync(user, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(LoginModel model, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(model.Email);

        // An unknown email and a wrong password report the same message so the
        // endpoint cannot be used to discover which addresses are registered.
        if (user is null || !user.IsActive)
        {
            logger.LogWarning("Login rejected for {Email}: no active account.", model.Email);
            return AuthResult.Fail("Invalid email or password.");
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            logger.LogWarning("Login rejected for {UserId}: account is locked out.", user.Id);
            return AuthResult.Fail("This account is temporarily locked. Please try again later.");
        }

        if (!signInResult.Succeeded)
        {
            logger.LogWarning("Login rejected for {UserId}: incorrect password.", user.Id);
            return AuthResult.Fail("Invalid email or password.");
        }

        logger.LogInformation("User {UserId} signed in.", user.Id);
        return await BuildAuthResultAsync(user, cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedToken = await dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.Token == refreshToken, cancellationToken);

        if (storedToken is null || storedToken.RevokedAt is not null || storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            logger.LogWarning("Refresh rejected: token is unknown, revoked or expired.");
            return AuthResult.Fail("The refresh token is invalid or has expired.");
        }

        if (!storedToken.User.IsActive)
        {
            logger.LogWarning("Refresh rejected for {UserId}: account is inactive.", storedToken.UserId);
            return AuthResult.Fail("The refresh token is invalid or has expired.");
        }

        // Rotate on use: a replayed token is worthless once it has been redeemed.
        storedToken.RevokedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Refreshed tokens for user {UserId}.", storedToken.UserId);
        return await BuildAuthResultAsync(storedToken.User, cancellationToken);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.Token == refreshToken, cancellationToken);

        if (storedToken is null || storedToken.RevokedAt is not null)
        {
            return false;
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Revoked a refresh token for user {UserId}.", storedToken.UserId);
        return true;
    }

    public async Task<UserModel?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUserProvider.UserId;
        if (userId is null)
        {
            return null;
        }

        var user = await userManager.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == userId && candidate.IsActive, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return await ProjectToUserModelAsync(user);
    }

    private async Task<AuthResult> BuildAuthResultAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.CreateAccessToken(user, roles);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = jwtTokenService.CreateRefreshToken(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var userModel = user.Adapt<UserModel>();
        userModel.Roles = [.. roles];

        return AuthResult.Success(accessToken, refreshToken.Token, userModel);
    }

    private async Task<UserModel> ProjectToUserModelAsync(ApplicationUser user)
    {
        var userModel = user.Adapt<UserModel>();
        userModel.Roles = [.. await userManager.GetRolesAsync(user)];
        return userModel;
    }

    private static string DescribeErrors(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(error => error.Description));
}
