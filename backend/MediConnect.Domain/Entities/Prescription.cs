using MediConnect.Domain.Common;

namespace MediConnect.Domain.Entities;

public class Prescription : TenantEntity
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? AppointmentId { get; set; }

    public DateOnly IssuedDate { get; set; }
    public string? Diagnosis { get; set; }
    public string? Advice { get; set; }
    public string? PdfUrl { get; set; }

    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}

public class PrescriptionItem : TenantEntity
{
    public Guid PrescriptionId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public string? Instructions { get; set; }

    public Prescription? Prescription { get; set; }
}

/// <summary>Reusable prescription template a doctor can apply quickly.</summary>
public class PrescriptionTemplate : TenantEntity
{
    public Guid DoctorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ItemsJson { get; set; } = "[]";
}
