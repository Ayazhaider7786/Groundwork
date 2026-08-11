namespace backend.Data.Abstractions;

/// <summary>
/// Supplies the ID of the user performing the current operation.
/// Lives in backend.Data because ApplicationDbContext depends on it for audit
/// stamping, and backend.Data must not reference any other project.
/// The implementation lives in backend.Services/Auth/CurrentUserProvider.cs.
/// </summary>
public interface ICurrentUserProvider
{
    /// <summary>
    /// The acting user's ID, or null when the operation runs outside an
    /// authenticated request — startup seeding and background workers.
    /// </summary>
    Guid? UserId { get; }
}
