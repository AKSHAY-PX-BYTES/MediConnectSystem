using System.Security.Claims;
using MediConnect.Application.Common.Interfaces;
using MediConnect.Domain.Enums;
using MediConnect.Infrastructure.Security;

namespace MediConnect.WebApi.Services;

/// <summary>
/// Reads the authenticated user's identity and tenant from the JWT claims on the
/// current HTTP request. Registered as scoped so each request gets its own context.
/// </summary>
public class CurrentUserService : ICurrentUser, ICurrentTenantProvider
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public bool IsSuperAdmin => string.Equals(Role, nameof(UserRole.SuperAdmin), StringComparison.OrdinalIgnoreCase);

    public Guid? TenantId
    {
        get
        {
            var raw = Principal?.FindFirstValue(JwtTokenService.TenantClaimType);
            if (Guid.TryParse(raw, out var id) && id != Guid.Empty)
                return id;
            return null;
        }
    }
}
