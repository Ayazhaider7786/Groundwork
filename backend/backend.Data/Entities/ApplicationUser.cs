using backend.Data.Entities.Common;
using Microsoft.AspNetCore.Identity;

namespace backend.Data.Entities;

/// <summary>
/// An account holder. One user owns one account — there is no organization or
/// team concept, so every user-owned entity points straight back here.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity, ISoftDeletable
{
    public string FullName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
