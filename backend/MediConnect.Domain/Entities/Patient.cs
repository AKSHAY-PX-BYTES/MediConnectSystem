using MediConnect.Domain.Common;
using MediConnect.Domain.Enums;

namespace MediConnect.Domain.Entities;

public class Patient : TenantEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public GenderType Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? BloodGroup { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    /// <summary>Human-friendly per-clinic medical record number.</summary>
    public string? MedicalRecordNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
