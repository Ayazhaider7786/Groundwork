using backend.Data.Abstractions;
using backend.Services.Common;
using backend.Services.Model.Files;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Services.Services.Files;

public interface IFileUploadService
{
    /// <summary>
    /// Stores a file under <c>Upload/{currentUserId}/{location}/</c> and returns the
    /// storage key. The owning user is taken from the token, never from the caller,
    /// so nothing can be written into another user's folder.
    /// </summary>
    /// <param name="location">
    /// A value from <see cref="FileUploadLocation"/> — always the constant, never a
    /// literal. An unrecognised value is refused rather than silently creating a
    /// folder.
    /// </param>
    Task<FileUploadResult> UploadAsync(FileUploadModel file, string location, CancellationToken cancellationToken = default);

    /// <summary>Null when the key does not exist or does not belong to the current user.</summary>
    Task<FileDownloadModel?> DownloadAsync(string storageKey, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}

/// <summary>
/// The service controllers talk to. It owns the rules — ownership, location, size,
/// extension — and delegates the bytes to whichever <see cref="IFileStorage"/> is
/// registered, so the rules hold identically whatever the backing store is.
/// </summary>
public class FileUploadService(
    IFileStorage fileStorage,
    ICurrentUserProvider currentUserProvider,
    IOptions<FileStorageOptions> options,
    ILogger<FileUploadService> logger) : IFileUploadService
{
    private readonly FileStorageOptions _options = options.Value;

    /// <summary>
    /// <c>Upload</c>, user, location, file name. Named rather than written as a bare
    /// 4 so the ownership check reads as a statement about the key's shape.
    /// </summary>
    private const int StorageKeySegmentCount = 4;

    public async Task<FileUploadResult> UploadAsync(
        FileUploadModel file,
        string location,
        CancellationToken cancellationToken = default)
    {
        // FileUploadLocation is a constants class, so a typo compiles. Checking
        // membership here turns that into a refused upload instead of files quietly
        // landing in a folder nothing ever reads.
        if (!FileUploadLocation.All.Contains(location, StringComparer.Ordinal))
        {
            logger.LogWarning("Upload of {FileName} rejected: '{Location}' is not a known upload location.", file.FileName, location);
            return FileUploadResult.Fail($"'{location}' is not a valid upload location.");
        }

        var userId = currentUserProvider.UserId;

        if (userId is null)
        {
            logger.LogWarning("Upload of {FileName} rejected: no authenticated user.", file.FileName);
            return FileUploadResult.Fail("Uploads require an authenticated user.");
        }

        var validationErrors = Validate(file);

        if (validationErrors.Count > 0)
        {
            logger.LogWarning("Upload of {FileName} rejected: {Errors}", file.FileName, string.Join("; ", validationErrors));
            return FileUploadResult.Fail([.. validationErrors]);
        }

        var folder = BuildFolder(userId.Value, location);

        logger.LogInformation(
            "Uploading {FileName} ({SizeInBytes} bytes, client-declared {ClientContentType}) to {Folder}.",
            file.FileName,
            file.SizeInBytes,
            file.ContentType,
            folder);

        var storedFile = await fileStorage.SaveAsync(file, folder, cancellationToken);

        return FileUploadResult.Success(storedFile);
    }

    public Task<FileDownloadModel?> DownloadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        if (!IsOwnedByCurrentUser(storageKey))
        {
            // Same answer as "no such file", so keys cannot be probed for existence.
            logger.LogWarning("Refused download of {StorageKey}: not owned by the current user.", storageKey);
            return Task.FromResult<FileDownloadModel?>(null);
        }

        return fileStorage.GetAsync(storageKey, cancellationToken);
    }

    public Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        if (!IsOwnedByCurrentUser(storageKey))
        {
            logger.LogWarning("Refused delete of {StorageKey}: not owned by the current user.", storageKey);
            return Task.FromResult(false);
        }

        return fileStorage.DeleteAsync(storageKey, cancellationToken);
    }

    /// <summary>
    /// The one place the folder convention is expressed:
    /// <c>Upload/{userId}/{location}</c>. Everything a user uploaded from one
    /// feature therefore sits in a single directory.
    /// </summary>
    private static string BuildFolder(Guid userId, string location) =>
        $"{FileUploadLocation.RootFolder}/{userId}/{location}";

    /// <summary>
    /// Keys are <c>Upload/{userId}/{location}/{file}</c>, so ownership is readable
    /// from the key itself — no database round trip needed to authorise a download.
    ///
    /// <para>
    /// The key's <b>whole shape</b> is checked rather than scanned for a user id,
    /// and that distinction is the security of this method. Reading the second
    /// segment and stopping there would accept
    /// <c>Upload/{mine}/Profile/../../{theirs}/Profile/x.png</c>: the segment at
    /// index 1 is still mine, yet <c>Path.GetFullPath</c> later collapses the ".."
    /// and lands inside another user's folder — still under the storage root, so the
    /// provider's root check sees nothing wrong either. Demanding exactly four
    /// segments, splitting on both separators and keeping empty entries means a
    /// "..", a backslash or a doubled slash fails the shape here and never reaches
    /// the point where it would be normalised away.
    /// </para>
    /// </summary>
    private bool IsOwnedByCurrentUser(string storageKey)
    {
        var userId = currentUserProvider.UserId;

        if (userId is null || string.IsNullOrWhiteSpace(storageKey))
        {
            return false;
        }

        var segments = storageKey.Split(['/', '\\']);

        return segments.Length == StorageKeySegmentCount
            && string.Equals(segments[0], FileUploadLocation.RootFolder, StringComparison.Ordinal)
            && Guid.TryParse(segments[1], out var ownerId)
            && ownerId == userId.Value
            && FileUploadLocation.All.Contains(segments[2], StringComparer.Ordinal)
            && IsSafeFileName(segments[3]);
    }

    /// <summary>
    /// The generated name is a GUID plus an extension, so anything that could act as
    /// a path instead of a name is not a key this application ever issued.
    /// </summary>
    private static bool IsSafeFileName(string fileName) =>
        fileName.Length > 0
        && fileName is not "." and not ".."
        && fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

    private List<string> Validate(FileUploadModel file)
    {
        var errors = new List<string>();

        if (file.SizeInBytes <= 0)
        {
            errors.Add("The file is empty.");
        }

        if (file.SizeInBytes > _options.MaxFileSizeBytes)
        {
            var limitInMegabytes = _options.MaxFileSizeBytes / 1024d / 1024d;
            errors.Add($"The file exceeds the maximum size of {limitInMegabytes:0.#} MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension))
        {
            errors.Add("The file has no extension.");
        }
        else if (!_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"Files of type '{extension}' are not allowed.");
        }

        return errors;
    }
}
