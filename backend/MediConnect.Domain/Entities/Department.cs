using MediConnect.Domain.Common;

namespace MediConnect.Domain.Entities;

/// <summary>A clinical department / specialty (e.g. Cardiology, Dental, Orthopedics).</summary>
public class Department : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
