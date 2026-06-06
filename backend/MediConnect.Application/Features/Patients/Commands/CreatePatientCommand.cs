using FluentValidation;
using MediatR;
using MediConnect.Application.Common.Exceptions;
using MediConnect.Application.Common.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Domain.Enums;

namespace MediConnect.Application.Features.Patients.Commands;

public record CreatePatientCommand(
    string FirstName,
    string LastName,
    GenderType Gender,
    DateOnly? DateOfBirth,
    string? Email,
    string? PhoneNumber,
    string? BloodGroup,
    string? Allergies) : IRequest<Guid>;

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantProvider _tenant;

    public CreatePatientCommandHandler(IApplicationDbContext db, ICurrentTenantProvider tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreatePatientCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.TenantId
            ?? throw new ForbiddenAccessException("A clinic context is required.");

        var patient = new Patient
        {
            TenantId = tenantId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            BloodGroup = request.BloodGroup,
            Allergies = request.Allergies,
            MedicalRecordNumber = $"MRN-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            IsActive = true
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(ct);
        return patient.Id;
    }
}
