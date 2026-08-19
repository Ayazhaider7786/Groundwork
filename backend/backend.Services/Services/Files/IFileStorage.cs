using backend.Services.Model.Files;

namespace backend.Services.Services.Files;

/// <summary>
/// Where bytes physically live. This is the extension point: adding Azure, S3 or
/// anything else means writing one more implementation and registering it in
/// Program.cs — no caller changes.
///
/// <para>
/// This interface gets its own file, unlike every other service in the codebase,
/// precisely because it has more than one implementation and so belongs to none of
/// them.
/// </para>
///
/// <para>
/// Implementations handle storage only. Size limits, extension whitelists, ownership
/// and any other business rule belong in <see cref="IFileUploadService"/>, so every
/// provider enforces them identically and a new provider cannot forget one.
/// </para>
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Writes the file and returns the key it can be fetched by later. The key is
    /// generated here, never taken from the caller or from the uploaded file name.
    /// </summary>
    Task<StoredFileModel> SaveAsync(FileUploadModel file, string folder, CancellationToken cancellationToken = default);

    /// <summary>Returns null when the key does not exist.</summary>
    Task<FileDownloadModel?> GetAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>False when there was nothing to delete.</summary>
    Task<bool> DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
