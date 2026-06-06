using MediConnect.Domain.Enums;

namespace MediConnect.Application.Features.Appointments.Dtos;

public record AppointmentDto(
    Guid Id,
    Guid PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName,
    Guid? DepartmentId,
    string? DepartmentName,
    AppointmentType Type,
    AppointmentStatus Status,
    DateOnly AppointmentDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Symptoms,
    string? Notes,
    int? QueueNumber,
    int? EstimatedWaitMinutes,
    DateTime CreatedAtUtc);
