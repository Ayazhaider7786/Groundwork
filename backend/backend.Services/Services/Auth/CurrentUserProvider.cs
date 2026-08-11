using System.Security.Claims;
using backend.Data.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace backend.Services.Services.Auth;

/// <summary>
/// The only implementation in the solution whose interface lives in another
/// project: ApplicationDbContext needs it for audit stamping, and backend.Data
/// cannot reference backend.Services.
/// </summary>
public class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public Guid? UserId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;

            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // The token carries "sub". The default inbound claim map rewrites it to
            // NameIdentifier, but that map can be cleared — accept either spelling
            // rather than silently returning null if it ever is.
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }
}
