namespace backend.Data.Entities.Common;

/// <summary>
/// Base class for every user-configured entity: identity, audit trail and
/// soft-delete flag. Entities that already extend a framework base class
/// (ApplicationUser) implement the two interfaces directly instead.
/// </summary>
public abstract class AuditableEntity : IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsActive { get; set; } = true;
}
