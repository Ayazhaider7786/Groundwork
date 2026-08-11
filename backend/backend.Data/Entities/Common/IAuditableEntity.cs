namespace backend.Data.Entities.Common;

/// <summary>
/// Audit trail for entities a user creates or edits. Populated automatically by
/// ApplicationDbContext.SaveChanges — never assign these in a service.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }

    Guid? CreatedBy { get; set; }

    DateTime? UpdatedAt { get; set; }

    Guid? UpdatedBy { get; set; }
}
