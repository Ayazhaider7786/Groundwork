namespace backend.Data.Entities.Common;

/// <summary>
/// Marks an entity that is deactivated rather than removed. Kept separate from
/// <see cref="IAuditableEntity"/> so append-only records can be audited without
/// ever being hidden.
/// </summary>
public interface ISoftDeletable
{
    bool IsActive { get; set; }
}
