using FluentValidation;
using MediatR;
using MediConnect.Application.Common.Interfaces;
using MediConnect.Application.Features.Auth.Dtos;
using MediConnect.Domain.Entities;
using MediConnect.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Application.Features.Auth.Commands;

/// <summary>
/// Self-service clinic onboarding: creates a new Tenant together with its first
/// Clinic Admin user and a trial subscription on the Starter plan.
/// </summary>
public record RegisterClinicCommand(
    string ClinicName,
    string AdminFirstName,
    string AdminLastName,
    string Email,
    string Password,
    string? PhoneNumber) : IRequest<AuthResponseDto>;

public class RegisterClinicCommandValidator : AbstractValidator<RegisterClinicCommand>
{
    public RegisterClinicCommandValidator()
    {
        RuleFor(x => x.ClinicName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AdminFirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdminLastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
    }
}

public class RegisterClinicCommandHandler : IRequestHandler<RegisterClinicCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public RegisterClinicCommandHandler(
        IApplicationDbContext db, IPasswordHasher hasher, ITokenService tokens)
    {
        _db = db;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<AuthResponseDto> Handle(RegisterClinicCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email, ct);
        if (emailExists)
            throw new Common.Exceptions.ConflictException("An account with this email already exists.");

        var slug = Slugify(request.ClinicName);
        var tenant = new Tenant
        {
            Name = request.ClinicName.Trim(),
            Slug = await EnsureUniqueSlugAsync(slug, ct),
            ContactEmail = email,
            ContactPhone = request.PhoneNumber,
            IsActive = true
        };

        var admin = new ApplicationUser
        {
            TenantId = tenant.Id,
            Email = email,
            PasswordHash = _hasher.Hash(request.Password),
            FirstName = request.AdminFirstName.Trim(),
            LastName = request.AdminLastName.Trim(),
            PhoneNumber = request.PhoneNumber,
            Role = UserRole.ClinicAdmin,
            IsActive = true,
            EmailConfirmed = false
        };

        // Attach a 14-day trial on the Starter plan if one exists.
        var starterPlan = await _db.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Tier == SubscriptionPlanTier.Starter && p.IsActive, ct);
        if (starterPlan is not null)
        {
            _db.Subscriptions.Add(new Subscription
            {
                TenantId = tenant.Id,
                PlanId = starterPlan.Id,
                Status = SubscriptionStatus.Trialing,
                StartUtc = DateTime.UtcNow,
                TrialEndsUtc = DateTime.UtcNow.AddDays(14),
                CurrentPeriodEndUtc = DateTime.UtcNow.AddDays(14)
            });
        }

        _db.Tenants.Add(tenant);
        _db.Users.Add(admin);

        var tokens = _tokens.GenerateTokens(admin);
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = admin.Id,
            Token = tokens.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        });

        await _db.SaveChangesAsync(ct);

        return new AuthResponseDto(
            tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAtUtc,
            new UserDto(admin.Id, admin.TenantId, admin.Email, admin.FullName, admin.Role.ToString()));
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug, CancellationToken ct)
    {
        var slug = baseSlug;
        var i = 1;
        while (await _db.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Slug == slug, ct))
            slug = $"{baseSlug}-{i++}";
        return slug;
    }

    private static string Slugify(string value)
    {
        var slug = new string(value.ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }
}
