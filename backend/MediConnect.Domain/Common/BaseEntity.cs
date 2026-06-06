namespace MediConnect.Domain.Common;

/// <summary>
/// Base type for all persistent entities. Provides a strongly-typed Guid key
/// and standard auditing fields populated automatically by the DbContext.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Soft-delete support
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}

/// <summary>
/// Base type for entities that belong to a single tenant (clinic). The
/// <see cref="TenantId"/> is used by EF Core global query filters to guarantee
/// complete data isolation between clinics.
/// </summary>
public abstract class TenantEntity : BaseEntity
{
    public Guid TenantId { get; set; }
}

/// <summary>
/// Marker interface so infrastructure code can detect tenant-scoped entities.
/// </summary>
public interface ITenantScoped
{
    Guid TenantId { get; set; }
}
