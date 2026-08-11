using backend.Model.Requests.Auth;
using backend.Model.Responses.Auth;
using backend.Services.Model.Auth;
using backend.Services.Services.Auth;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>Creates an account. The new user becomes the Admin of their own account.</summary>
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Registration requested for {Email}.", request.Email);

            var model = request.Adapt<RegisterModel>();
            var result = await authService.RegisterAsync(model, cancellationToken);

            if (!result.Succeeded)
            {
                logger.LogWarning("Registration failed for {Email}.", request.Email);
                return BadRequest(new { errors = result.Errors });
            }

            logger.LogInformation("Registration succeeded for {Email}.", request.Email);
            return Ok(result.Adapt<AuthResponse>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while registering {Email}.", request.Email);
            return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>Signs in and issues an access token plus a refresh token.</summary>
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Login requested for {Email}.", request.Email);

            var model = request.Adapt<LoginModel>();
            var result = await authService.LoginAsync(model, cancellationToken);

            if (!result.Succeeded)
            {
                logger.LogWarning("Login failed for {Email}.", request.Email);
                return Unauthorized(new { errors = result.Errors });
            }

            logger.LogInformation("Login succeeded for {Email}.", request.Email);
            return Ok(result.Adapt<AuthResponse>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while signing in {Email}.", request.Email);
            return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>Exchanges a refresh token for a new token pair. The old token is revoked.</summary>
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Token refresh requested.");

            var result = await authService.RefreshAsync(request.RefreshToken, cancellationToken);

            if (!result.Succeeded)
            {
                logger.LogWarning("Token refresh failed.");
                return Unauthorized(new { errors = result.Errors });
            }

            logger.LogInformation("Token refresh succeeded.");
            return Ok(result.Adapt<AuthResponse>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while refreshing a token.");
            return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>Revokes a refresh token so it can no longer be redeemed.</summary>
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Logout requested.");

            var revoked = await authService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

            if (!revoked)
            {
                logger.LogWarning("Logout requested with a token that was unknown or already revoked.");
                return Problem(detail: "The refresh token was not found.", statusCode: StatusCodes.Status404NotFound);
            }

            logger.LogInformation("Logout succeeded.");
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while logging out.");
            return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>Returns the signed-in user's profile and roles.</summary>
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Fetching the current user's profile.");

            var user = await authService.GetCurrentUserAsync(cancellationToken);

            if (user is null)
            {
                logger.LogWarning("The current user's profile could not be resolved from the token.");
                return Problem(detail: "The current user was not found.", statusCode: StatusCodes.Status404NotFound);
            }

            return Ok(user.Adapt<UserResponse>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching the current user's profile.");
            return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
