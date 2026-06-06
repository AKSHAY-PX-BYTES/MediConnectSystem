using FluentValidation;
using MediatR;
using MediConnect.Application.Common.Exceptions;
using MediConnect.Application.Common.Interfaces;
using MediConnect.Application.Features.Auth.Dtos;
using MediConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public LoginCommandHandler(IApplicationDbContext db, IPasswordHasher hasher, ITokenService tokens)
    {
        _db = db;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // IgnoreQueryFilters: login happens before a tenant context is established.
        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new ForbiddenAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new ForbiddenAccessException("This account has been deactivated.");

        // Block login if the tenant itself is deactivated (Super Admins excluded).
        if (user.TenantId != Guid.Empty)
        {
            var tenantActive = await _db.Tenants
                .IgnoreQueryFilters()
                .Where(t => t.Id == user.TenantId)
                .Select(t => t.IsActive)
                .FirstOrDefaultAsync(ct);
            if (!tenantActive)
                throw new ForbiddenAccessException("This clinic account is currently inactive.");
        }

        user.LastLoginUtc = DateTime.UtcNow;

        var tokens = _tokens.GenerateTokens(user);
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
