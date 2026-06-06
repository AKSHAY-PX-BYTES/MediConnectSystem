using MediConnect.Domain.Common;

namespace MediConnect.Domain.Entities;

/// <summary>Immutable audit trail entry for security and compliance (HIPAA/GDPR).</summary>
public class AuditLog : BaseEntity
{
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Changes { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
