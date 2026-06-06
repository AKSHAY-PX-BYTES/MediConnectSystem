namespace MediConnect.Application.Common.Interfaces;

/// <summary>
/// Resolves the current tenant and user context for the active request. Implemented
/// in the WebApi layer from JWT claims / tenant-resolution middleware.
/// </summary>
public interface ICurrentTenantProvider
{
    Guid? TenantId { get; }
    bool IsSuperAdmin { get; }
}

public interface ICurrentUser
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsSuperAdmin { get; }
}
