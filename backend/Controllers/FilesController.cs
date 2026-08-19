using backend.Model.Responses.Files;
using backend.Services.Model.Files;
using backend.Services.Services.Files;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FilesController(IFileUploadService fileUploadService, ILogger<FilesController> logger) : ControllerBase
{
    /// <summary>
    /// Uploads a file and returns the storage key. Save that key on your own entity —
    /// it is what identifies the file afterwards.
    /// </summary>
    /// <param name="file">The multipart form field carrying the bytes.</param>
    /// <param name="location">A value from <c>FileUploadLocation</c>, e.g. "Profile".</param>
    /// <param name="cancellationToken">Cancels the upload if the client disconnects.</param>
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UploadedFileResponse), StatusCodes.Status200OK)]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string location, CancellationToken cancellationToken = default)
    {
        try
        {
            if (file is null)
            {
                logger.LogWarning("Upload requested with no file attached.");
                return Problem(detail: "A file must be attached.", statusCode: StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                logger.LogWarning("Upload requested with no upload location.");
                return Problem(detail: "An upload location must be provided.", statusCode: StatusCodes.Status400BadRequest);
            }

            logger.LogInformation("Upload requested: {FileName} for {Location}.", file.FileName, location);

            await using var content = file.OpenReadStream();

            var model = new FileUploadModel
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeInBytes = file.Length,
                Content = content,
            };

            var result = await fileUploadService.UploadAsync(model, location, cancellationToken);

            if (!result.Succeeded)
            {
                logger.LogWarning("Upload of {FileName} was rejected.", file.FileName);
                return Problem(detail: string.Join(" ", result.Errors), statusCode: StatusCodes.Status400BadRequest);
            }

            logger.LogInformation("Upload stored as {StorageKey}.", result.File!.StorageKey);
            return Ok(result.File.Adapt<UploadedFileResponse>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while uploading a file.");
            return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>Downloads a previously uploaded file by its storage key.</summary>
    /// <param name="storageKey">The key returned by the upload.</param>
    /// <param name="cancellationToken">Cancels the read if the client disconnects.</param>
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                logger.LogWarning("Download requested with no storage key.");
                return Problem(detail: "A storage key must be provided.", statusCode: StatusCodes.Status400BadRequest);
            }

            var file = await fileUploadService.DownloadAsync(storageKey, cancellationToken);

            if (file is null)
            {
                logger.LogWarning("Download requested for unavailable storage key {StorageKey}.", storageKey);
                return Problem(detail: "The file was not found.", statusCode: StatusCodes.Status404NotFound);
            }

            // The content type comes from the whitelisted extension, so it is already
            // trustworthy; nosniff stops a browser overriding it with a guess of its own.
            Response.Headers.XContentTypeOptions = "nosniff";

            // Naming the file sets Content-Disposition: attachment, so an uploaded
            // file is saved rather than rendered in the site's origin.
            return File(file.Content, file.ContentType, file.FileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while downloading {StorageKey}.", storageKey);
            return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>Deletes a previously uploaded file.</summary>
    /// <param name="storageKey">The key returned by the upload.</param>
    /// <param name="cancellationToken">Cancels the delete if the client disconnects.</param>
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ObjectResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                logger.LogWarning("Delete requested with no storage key.");
                return Problem(detail: "A storage key must be provided.", statusCode: StatusCodes.Status400BadRequest);
            }

            var deleted = await fileUploadService.DeleteAsync(storageKey, cancellationToken);

            if (!deleted)
            {
                logger.LogWarning("Delete requested for unavailable storage key {StorageKey}.", storageKey);
                return Problem(detail: "The file was not found.", statusCode: StatusCodes.Status404NotFound);
            }

            logger.LogInformation("Deleted {StorageKey}.", storageKey);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting {StorageKey}.", storageKey);
            return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
