using MediConnect.Domain.Common;
using MediConnect.Domain.Enums;

namespace MediConnect.Domain.Entities;

public class Doctor : TenantEntity
{
    public Guid? DepartmentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Title { get; set; } = "Dr.";
    public string? Specialization { get; set; }
    public string? Qualifications { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Bio { get; set; }
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public int DefaultSlotDurationMinutes { get; set; } = 15;
    public bool IsAcceptingAppointments { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public Department? Department { get; set; }
    public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
    public ICollection<DoctorTimeOff> TimeOffs { get; set; } = new List<DoctorTimeOff>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public string FullName => $"{Title} {FirstName} {LastName}".Trim();
}

/// <summary>A recurring weekly availability window for a doctor.</summary>
public class DoctorAvailability : TenantEntity
{
    public Guid DoctorId { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public TimeOnly? BreakStart { get; set; }
    public TimeOnly? BreakEnd { get; set; }
    public bool IsActive { get; set; } = true;

    public Doctor? Doctor { get; set; }
}

/// <summary>A one-off block of unavailability (vacation, leave, conference).</summary>
public class DoctorTimeOff : TenantEntity
{
    public Guid DoctorId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Reason { get; set; }

    public Doctor? Doctor { get; set; }
}
