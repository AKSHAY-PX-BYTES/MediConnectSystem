using MediConnect.Domain.Entities;
using MediConnect.Domain.Enums;

namespace MediConnect.Application.Common.Interfaces;

public record AuthTokens(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);

/// <summary>JWT generation and password hashing services.</summary>
public interface ITokenService
{
    AuthTokens GenerateTokens(ApplicationUser user);
    string GenerateRefreshToken();
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
