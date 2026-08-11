namespace backend.Data.Constants;

/// <summary>
/// Role names as constants. [Authorize(Roles = ...)] requires a compile-time
/// constant, so a C# enum cannot be used — this is the one sanctioned exception
/// to the no-magic-strings rule.
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";

    /// <summary>Every role the seeder creates. Adding a role starts here.</summary>
    public static readonly IReadOnlyList<string> All = [Admin];
}
