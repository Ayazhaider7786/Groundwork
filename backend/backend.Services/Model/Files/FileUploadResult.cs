namespace backend.Services.Model.Files;

/// <summary>
/// An upload can be refused for several distinct reasons — unknown location, empty,
/// too large, wrong extension — and the caller needs to relay which. Result object
/// rather than an exception, per the Result Object Pattern.
/// </summary>
public class FileUploadResult
{
    public bool Succeeded { get; set; }

    public IReadOnlyList<string> Errors { get; set; } = [];

    public StoredFileModel? File { get; set; }

    public static FileUploadResult Fail(params string[] errors) =>
        new() { Succeeded = false, Errors = errors };

    public static FileUploadResult Success(StoredFileModel file) =>
        new() { Succeeded = true, File = file };
}
