namespace backend.Model.Responses.Files;

public class UploadedFileResponse
{
    /// <summary>
    /// Save this on your own entity to reference the file later; it survives a
    /// provider change. It is also what the download and delete endpoints take.
    /// </summary>
    public string StorageKey { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeInBytes { get; set; }

    public DateTime UploadedAt { get; set; }
}
