namespace backend.Services.Model.Files;

public class FileDownloadModel
{
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    /// <summary>Open stream positioned at the start. The caller disposes it.</summary>
    public Stream Content { get; set; } = Stream.Null;
}
