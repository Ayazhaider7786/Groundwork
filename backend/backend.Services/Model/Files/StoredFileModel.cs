namespace backend.Services.Model.Files;

public class StoredFileModel
{
    /// <summary>
    /// Provider-independent identifier, e.g.
    /// <c>Upload/{userId}/Profile/9f2c….png</c>. <b>This is the value to save on
    /// your own entity</b> — save it rather than a filesystem path or a blob URL,
    /// so the same value keeps working after a provider switch.
    /// </summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>The name the user uploaded, kept for display only — never used as a path.</summary>
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    /// <summary>Bytes actually written, as counted by the storage provider.</summary>
    public long SizeInBytes { get; set; }

    public DateTime UploadedAt { get; set; }
}
