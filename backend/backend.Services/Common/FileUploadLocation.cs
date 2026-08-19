namespace backend.Services.Common;

/// <summary>
/// Which feature a file was uploaded from. Becomes the third segment of every
/// storage key: <c>Upload/{UserId}/{FileUploadLocation}/{file}</c>.
///
/// <para>
/// To use this, simply use <c>FileUploadLocation.Profile</c> — always the constant,
/// never a literal.
/// </para>
///
/// <para>
/// These values are <b>persisted data</b>, not just code. Changing one orphans every
/// file already stored under the old value — the saved paths still point at a folder
/// nothing reads any more. Add entries freely; change an existing value only by
/// moving the folders to match.
/// </para>
///
/// <para>
/// Because these are strings rather than an enum, the compiler cannot catch a
/// mistyped location. <see cref="All"/> exists so the service can reject an unknown
/// value at runtime — every new entry must be added to it.
/// </para>
/// </summary>
public static class FileUploadLocation
{
    /// <summary>
    /// The fixed first segment of every storage key, so uploads occupy one
    /// predictable subtree and nothing else has to be excluded from backups or
    /// cleanup jobs.
    /// </summary>
    public const string RootFolder = "Upload";

    /// <summary>Avatars and anything else attached to a user's own profile.</summary>
    public const string Profile = "Profile";

    /// <summary>Every known upload location. Adding a location starts here.</summary>
    public static readonly IReadOnlyList<string> All = [Profile];
}
