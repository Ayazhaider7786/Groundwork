namespace backend.Services.Model.Files;

/// <summary>
/// Settings for the Local provider. Each provider gets its own nested section, so
/// adding Azure means adding an <c>Azure</c> object beside this one rather than
/// widening a shared options class.
/// </summary>
public class LocalFileStorageOptions
{
    /// <summary>
    /// The storage root. A relative path is resolved against the app's content
    /// root; an absolute path is used as-is. Every key sits under an
    /// <c>Upload/</c> folder inside it.
    ///
    /// <para>
    /// Keep it outside wwwroot. Static file middleware serves whatever it can
    /// reach, which would hand out other users' uploads without ever consulting
    /// the ownership check.
    /// </para>
    /// </summary>
    public string RootPath { get; set; } = "App_Data";
}
