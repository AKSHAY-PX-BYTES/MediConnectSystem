using MediatR;
using MediConnect.Application.Common.Exceptions;
using MediConnect.Application.Common.Interfaces;
using MediConnect.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Application.Features.Appointments.Commands;

/// <summary>Doctor/clinic action to approve, reject, reschedule, or progress an appointment.</summary>
public record UpdateAppointmentStatusCommand(
    Guid AppointmentId,
    AppointmentStatus NewStatus,
    string? Reason = null,
    DateOnly? NewDate = null,
    TimeOnly? NewStartTime = null) : IRequest<Unit>;

public class UpdateAppointmentStatusCommandHandler
    : IRequestHandler<UpdateAppointmentStatusCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public UpdateAppointmentStatusCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(UpdateAppointmentStatusCommand request, CancellationToken ct)
    {
        var appt = await _db.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct)
            ?? throw new NotFoundException("Appointment", request.AppointmentId);

        switch (request.NewStatus)
        {
            case AppointmentStatus.Rejected:
                appt.RejectionReason = request.Reason;
                break;

            case AppointmentStatus.Cancelled:
                appt.CancellationReason = request.Reason;
                break;

            case AppointmentStatus.Rescheduled:
                if (request.NewDate is null || request.NewStartTime is null)
                    throw new ConflictException("New date and time are required to reschedule.");
                appt.AppointmentDate = request.NewDate.Value;
                var duration = (appt.EndTime.ToTimeSpan() - appt.StartTime.ToTimeSpan()).TotalMinutes;
                appt.StartTime = request.NewStartTime.Value;
                appt.EndTime = request.NewStartTime.Value.AddMinutes(duration);
                break;

            case AppointmentStatus.CheckedIn:
                appt.CheckedInAtUtc = DateTime.UtcNow;
                appt.QueueNumber ??= await NextQueueNumberAsync(appt.DoctorId, appt.AppointmentDate, ct);
                break;

            case AppointmentStatus.InProgress:
                appt.StartedAtUtc = DateTime.UtcNow;
                break;

            case AppointmentStatus.Completed:
                appt.CompletedAtUtc = DateTime.UtcNow;
                break;
        }

        appt.Status = request.NewStatus;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }

    private async Task<int> NextQueueNumberAsync(Guid doctorId, DateOnly date, CancellationToken ct)
    {
        var max = await _db.Appointments
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate == date && a.QueueNumber != null)
            .MaxAsync(a => (int?)a.QueueNumber, ct);
        return (max ?? 0) + 1;
    }
}
