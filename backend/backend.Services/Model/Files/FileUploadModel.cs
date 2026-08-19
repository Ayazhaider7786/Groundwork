namespace backend.Services.Model.Files;

/// <summary>
/// A file on its way in. The controller builds this from IFormFile so the service
/// layer never sees an ASP.NET MVC type.
/// </summary>
public class FileUploadModel
{
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// What the client claimed the file is. Logged but never trusted: the content
    /// type served back is derived from the whitelisted extension instead, because
    /// a browser acts on that value.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    public long SizeInBytes { get; set; }

    /// <summary>
    /// Left open by the caller and read by the storage provider. The caller owns
    /// disposal — for an upload that is the request pipeline.
    /// </summary>
    public Stream Content { get; set; } = Stream.Null;
}
