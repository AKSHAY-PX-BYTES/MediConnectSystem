using FluentValidation;
using MediatR;
using MediConnect.Application.Common.Exceptions;
using MediConnect.Application.Common.Interfaces;
using MediConnect.Application.Features.Appointments.Dtos;
using MediConnect.Domain.Entities;
using MediConnect.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Application.Features.Appointments.Commands;

public record BookAppointmentCommand(
    Guid PatientId,
    Guid DoctorId,
    Guid? DepartmentId,
    AppointmentType Type,
    DateOnly AppointmentDate,
    TimeOnly StartTime,
    string? Symptoms) : IRequest<AppointmentDto>;

public class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.AppointmentDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Appointment date cannot be in the past.");
    }
}

public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, AppointmentDto>
{
    private readonly IApplicationDbContext _db;

    public BookAppointmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<AppointmentDto> Handle(BookAppointmentCommand request, CancellationToken ct)
    {
        var doctor = await _db.Doctors
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, ct)
            ?? throw new NotFoundException(nameof(Doctor), request.DoctorId);

        if (!doctor.IsAcceptingAppointments)
            throw new ConflictException("This doctor is not currently accepting appointments.");

        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, ct)
            ?? throw new NotFoundException(nameof(Patient), request.PatientId);

        var endTime = request.StartTime.AddMinutes(doctor.DefaultSlotDurationMinutes);

        // Prevent double-booking the same doctor for an overlapping slot.
        var clash = await _db.Appointments.AnyAsync(a =>
            a.DoctorId == request.DoctorId &&
            a.AppointmentDate == request.AppointmentDate &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Rejected &&
            request.StartTime < a.EndTime && endTime > a.StartTime, ct);

        if (clash)
            throw new ConflictException("The selected time slot is no longer available.");

        var appointment = new Appointment
        {
            TenantId = doctor.TenantId,
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            DepartmentId = request.DepartmentId ?? doctor.DepartmentId,
            Type = request.Type,
            Status = AppointmentStatus.Requested,
            AppointmentDate = request.AppointmentDate,
            StartTime = request.StartTime,
            EndTime = endTime,
            Symptoms = request.Symptoms
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync(ct);

        return new AppointmentDto(
            appointment.Id, patient.Id, patient.FullName, doctor.Id, doctor.FullName,
            appointment.DepartmentId, null, appointment.Type, appointment.Status,
            appointment.AppointmentDate, appointment.StartTime, appointment.EndTime,
            appointment.Symptoms, appointment.Notes, appointment.QueueNumber,
            appointment.EstimatedWaitMinutes, appointment.CreatedAtUtc);
    }
}
