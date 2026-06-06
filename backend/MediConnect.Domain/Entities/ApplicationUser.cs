using MediConnect.Domain.Common;
using MediConnect.Domain.Enums;

namespace MediConnect.Domain.Entities;

/// <summary>
/// Application user. Super Admin users have a null <see cref="TenantId"/> and are
/// not constrained by the tenant query filter; all other users belong to a clinic.
/// </summary>
public class ApplicationUser : BaseEntity, ITenantScoped
{
    /// <summary>Null for platform-level Super Admins.</summary>
    public Guid TenantId { get; set; }

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; }
    public DateTime? LastLoginUtc { get; set; }

    // Linked profile entities (optional, depending on role)
    public Guid? DoctorId { get; set; }
    public Guid? PatientId { get; set; }

    public Tenant? Tenant { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? CreatedByIp { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;

    public ApplicationUser? User { get; set; }
}
