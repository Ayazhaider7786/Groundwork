using backend.Data.Enums;

namespace backend.Services.Model.Files;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>The single switch that decides which backing store is used.</summary>
    public FileStorageProvider Provider { get; set; } = FileStorageProvider.Local;

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Lower-case, dot-prefixed (".png"). An empty list allows nothing — a
    /// whitelist that silently permits everything when misconfigured is worse than
    /// one that rejects everything and gets noticed.
    ///
    /// <para>
    /// <c>.svg</c> is left out on purpose: an SVG is a document that can carry
    /// script, so allowing it would mean hosting attacker-authored script on the
    /// API's own origin the moment a download is ever served inline.
    /// </para>
    /// </summary>
    public List<string> AllowedExtensions { get; set; } = [];

    public LocalFileStorageOptions Local { get; set; } = new();
}
