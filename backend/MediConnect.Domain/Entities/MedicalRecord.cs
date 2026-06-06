using MediConnect.Domain.Common;

namespace MediConnect.Domain.Entities;

/// <summary>An EMR clinical visit record / encounter note.</summary>
public class MedicalRecord : TenantEntity
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? DoctorId { get; set; }

    public DateOnly VisitDate { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentNotes { get; set; }
    public string? VitalsJson { get; set; }
    public string? FollowUpInstructions { get; set; }
    public DateOnly? FollowUpDate { get; set; }

    public Patient? Patient { get; set; }
    public ICollection<MedicalRecordAttachment> Attachments { get; set; } = new List<MedicalRecordAttachment>();
}

public class MedicalRecordAttachment : TenantEntity
{
    public Guid MedicalRecordId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? Category { get; set; } // X-Ray, Lab Report, Scan, etc.
    public string? ContentType { get; set; }

    public MedicalRecord? MedicalRecord { get; set; }
}
