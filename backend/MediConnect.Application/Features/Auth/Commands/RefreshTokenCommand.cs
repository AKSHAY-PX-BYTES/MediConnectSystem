using MediatR;
using MediConnect.Application.Common.Exceptions;
using MediConnect.Application.Common.Interfaces;
using MediConnect.Application.Features.Auth.Dtos;
using MediConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokens;

    public RefreshTokenCommandHandler(IApplicationDbContext db, ITokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var stored = await _db.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, ct);

        if (stored is null || !stored.IsActive)
            throw new ForbiddenAccessException("Invalid or expired refresh token.");

        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == stored.UserId, ct);

        if (user is null || !user.IsActive)
            throw new ForbiddenAccessException("User no longer active.");

        // Rotate the refresh token.
        var tokens = _tokens.GenerateTokens(user);
        stored.RevokedAtUtc = DateTime.UtcNow;
        stored.ReplacedByToken = tokens.RefreshToken;

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = tokens.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        });

        await _db.SaveChangesAsync(ct);

        return new AuthResponseDto(
            tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAtUtc,
            new UserDto(user.Id, user.TenantId, user.Email, user.FullName, user.Role.ToString()));
    }
}
