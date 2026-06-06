using MediConnect.Domain.Common;
using MediConnect.Domain.Enums;

namespace MediConnect.Domain.Entities;

public class Appointment : TenantEntity
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? DepartmentId { get; set; }

    public AppointmentType Type { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Requested;

    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public string? Symptoms { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public string? CancellationReason { get; set; }

    // Queue management
    public int? QueueNumber { get; set; }
    public DateTime? CheckedInAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int? EstimatedWaitMinutes { get; set; }

    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public Department? Department { get; set; }
    public ICollection<AppointmentAttachment> Attachments { get; set; } = new List<AppointmentAttachment>();
}

public class AppointmentAttachment : TenantEntity
{
    public Guid AppointmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }

    public Appointment? Appointment { get; set; }
}
