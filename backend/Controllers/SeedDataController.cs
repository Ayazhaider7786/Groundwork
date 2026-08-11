using backend.Model.Responses.SeedData;
using backend.Services.Services.SeedData;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Development-only helper that lists the seeded accounts, passwords included, so
/// the login screen can fill itself in.
///
/// It is deliberately unauthenticated: its whole purpose is to help someone sign in
/// BEFORE they have a token. That makes the environment gate the only thing standing
/// between this and a credential leak, so it returns 404 outside Development rather
/// than merely returning an empty list. Delete this controller before any
/// production deployment.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SeedDataController(
    ISeedDataService seedDataService,
    IWebHostEnvironment environment,
    ILogger<SeedDataController> logger) : ControllerBase
{
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IReadOnlyList<SeedUserResponse>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpGet("users")]
    public IActionResult GetSeedUsers()
    {
        try
        {
            if (!environment.IsDevelopment())
            {
                logger.LogWarning("Seed users were requested outside Development and refused.");
                return Problem(
                    detail: "Seed data is only available in Development.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            var seedUsers = seedDataService.GetSeedUsers();

            logger.LogInformation("Returning {Count} seed users.", seedUsers.Count);
            return Ok(seedUsers.Adapt<IReadOnlyList<SeedUserResponse>>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching seed users.");
            return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
