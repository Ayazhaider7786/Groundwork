namespace backend.Data.Enums;

/// <summary>
/// Which <c>IFileStorage</c> implementation backs uploads. Switching provider is a
/// one-line configuration change; see FileStorage:Provider.
/// </summary>
public enum FileStorageProvider
{
    /// <summary>The server's own disk.</summary>
    Local,

    /// <summary>
    /// Declared so the roadmap is visible, but no implementation is registered yet.
    /// Selecting it fails at startup with an explicit message rather than at the
    /// first upload.
    /// </summary>
    Azure,
}
