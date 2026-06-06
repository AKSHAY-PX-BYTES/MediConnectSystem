namespace MediConnect.Application.Features.Auth.Dtos;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    UserDto User);

public record UserDto(
    Guid Id,
    Guid? TenantId,
    string Email,
    string FullName,
    string Role);
