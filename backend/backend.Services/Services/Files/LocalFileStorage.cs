using backend.Services.Common;
using backend.Services.Model.Files;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Services.Services.Files;

/// <summary>
/// Writes uploads to the server's own disk under a configured root. Suitable for a
/// single-server deployment; behind a load balancer each node would see only its own
/// files, which is the point at which a shared provider is needed.
/// </summary>
public class LocalFileStorage(
    IOptions<FileStorageOptions> options,
    IHostEnvironment environment,
    ILogger<LocalFileStorage> logger) : IFileStorage
{
    private readonly FileStorageOptions _options = options.Value;

    /// <summary>
    /// Maps a whitelisted extension to the content type served back. Immutable once
    /// built, so one shared instance is enough.
    /// </summary>
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    public async Task<StoredFileModel> SaveAsync(
        FileUploadModel file,
        string folder,
        CancellationToken cancellationToken = default)
    {
        var storageKey = BuildStorageKey(file.FileName, folder);
        var fullPath = ResolveFullPath(storageKey);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        long bytesWritten;

        try
        {
            await using var target = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await file.Content.CopyToAsync(target, cancellationToken);
            bytesWritten = target.Length;
        }
        catch (Exception ex)
        {
            // A client that disconnects mid-upload would otherwise leave a truncated
            // file behind that nothing points at, so nothing would ever clean it up.
            logger.LogWarning(ex, "Writing {StorageKey} failed; removing the partial file.", storageKey);
            TryDeletePartialFile(fullPath);
            throw;
        }

        logger.LogInformation("Stored upload {StorageKey} ({SizeInBytes} bytes).", storageKey, bytesWritten);

        return new StoredFileModel
        {
            StorageKey = storageKey,
            FileName = file.FileName,
            ContentType = ResolveContentType(Path.GetExtension(storageKey)),
            SizeInBytes = bytesWritten,
            UploadedAt = DateTime.UtcNow,
        };
    }

    public Task<FileDownloadModel?> GetAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveFullPath(storageKey);

        if (!File.Exists(fullPath))
        {
            logger.LogWarning("Requested upload {StorageKey} does not exist.", storageKey);
            return Task.FromResult<FileDownloadModel?>(null);
        }

        var model = new FileDownloadModel
        {
            FileName = Path.GetFileName(storageKey),

            // Derived from the extension, which the key preserves, rather than
            // returned as a flat octet-stream. The uploader's declared type is never
            // used: a browser acts on this value.
            ContentType = ResolveContentType(Path.GetExtension(storageKey)),

            // FileShare.Read so a concurrent download of the same file is not blocked.
            Content = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true),
        };

        return Task.FromResult<FileDownloadModel?>(model);
    }

    public Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveFullPath(storageKey);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult(false);
        }

        File.Delete(fullPath);
        logger.LogInformation("Deleted upload {StorageKey}.", storageKey);

        return Task.FromResult(true);
    }

    /// <summary>
    /// The file name is generated, never taken from the upload: a GUID under the
    /// folder the service composed. That removes collisions and means an uploaded
    /// file name can never become a path.
    /// </summary>
    private static string BuildStorageKey(string fileName, string folder)
    {
        var extension = SanitizeExtension(Path.GetExtension(fileName));
        return $"{SanitizeFolder(folder)}/{Guid.NewGuid():N}{extension}";
    }

    /// <summary>
    /// The folder arrives multi-segment (<c>Upload/{userId}/{location}</c>), so each
    /// segment is cleaned separately and the separators are rebuilt. Cleaning strips
    /// dots, which is what turns a ".." segment into an empty one that is then
    /// dropped — traversal cannot survive this even before the root check.
    /// </summary>
    private static string SanitizeFolder(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return FileUploadLocation.RootFolder;
        }

        var segments = folder
            .Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)
            .Select(SanitizeSegment)
            .Where(segment => segment.Length > 0)
            .ToArray();

        return segments.Length == 0 ? FileUploadLocation.RootFolder : string.Join('/', segments);
    }

    private static string SanitizeSegment(string segment) =>
        new([.. segment.Where(character => char.IsLetterOrDigit(character) || character is '-' or '_')]);

    private static string SanitizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        var cleaned = new string([.. extension.Where(char.IsLetterOrDigit)]);

        return cleaned.Length == 0 ? string.Empty : $".{cleaned.ToLowerInvariant()}";
    }

    private static string ResolveContentType(string extension) =>
        ContentTypeProvider.TryGetContentType(extension, out var contentType)
            ? contentType
            : "application/octet-stream";

    private void TryDeletePartialFile(string fullPath)
    {
        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            // Swallowed on purpose: the original failure is the one worth reporting,
            // and a leftover file is a cleanup problem, not a request-breaking one.
            logger.LogError(ex, "Could not remove the partial file at {FullPath}.", fullPath);
        }
    }

    /// <summary>
    /// Resolves a key to an absolute path and refuses anything that lands outside
    /// the storage root. The service has already validated the key's shape, so this
    /// is a backstop — but it is what makes the provider safe to reuse from a caller
    /// that has not been as careful.
    /// </summary>
    private string ResolveFullPath(string storageKey)
    {
        var rootPath = GetRootPath();
        var fullPath = Path.GetFullPath(Path.Combine(rootPath, storageKey));

        if (!fullPath.StartsWith(rootPath, StringComparison.Ordinal))
        {
            logger.LogWarning("Refused storage key {StorageKey}: it resolves outside the storage root.", storageKey);
            throw new InvalidOperationException("The storage key resolves outside the storage root.");
        }

        return fullPath;
    }

    /// <summary>
    /// The trailing separator matters: without it, a root of "…/App_Data" would also
    /// accept paths under a sibling directory named "…/App_Data-backup".
    /// </summary>
    private string GetRootPath()
    {
        var configuredPath = _options.Local.RootPath;

        var rootedPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath);

        return Path.GetFullPath(rootedPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    }
}
