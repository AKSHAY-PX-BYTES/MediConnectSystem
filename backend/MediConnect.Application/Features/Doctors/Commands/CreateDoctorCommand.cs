using FluentValidation;
using MediatR;
using MediConnect.Application.Common.Exceptions;
using MediConnect.Application.Common.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Application.Features.Doctors.Commands;

public record CreateDoctorCommand(
    string FirstName,
    string LastName,
    Guid? DepartmentId,
    string? Specialization,
    string? Email,
    string? PhoneNumber,
    decimal ConsultationFee,
    int ExperienceYears,
    int DefaultSlotDurationMinutes) : IRequest<Guid>;

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ConsultationFee).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DefaultSlotDurationMinutes).InclusiveBetween(5, 120);
    }
}

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantProvider _tenant;

    public CreateDoctorCommandHandler(IApplicationDbContext db, ICurrentTenantProvider tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateDoctorCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.TenantId
            ?? throw new ForbiddenAccessException("A clinic context is required.");

        if (request.DepartmentId is not null)
        {
            var deptExists = await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId, ct);
            if (!deptExists) throw new NotFoundException("Department", request.DepartmentId);
        }

        var doctor = new Doctor
        {
            TenantId = tenantId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DepartmentId = request.DepartmentId,
            Specialization = request.Specialization,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            ConsultationFee = request.ConsultationFee,
            ExperienceYears = request.ExperienceYears,
            DefaultSlotDurationMinutes = request.DefaultSlotDurationMinutes,
            IsActive = true,
            IsAcceptingAppointments = true
        };

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync(ct);
        return doctor.Id;
    }
}
