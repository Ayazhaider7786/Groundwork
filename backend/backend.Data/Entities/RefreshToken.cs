namespace backend.Data.Entities;

/// <summary>
/// A single-use token that exchanges for a new access token. System-emitted, so
/// it carries its own lifecycle fields rather than extending AuditableEntity —
/// revocation is not a soft delete, and the owning user is already on the row.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
