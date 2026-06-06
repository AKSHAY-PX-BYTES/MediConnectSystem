using MediConnect.Domain.Common;
using MediConnect.Domain.Enums;

namespace MediConnect.Domain.Entities;

/// <summary>
/// A Tenant represents a single clinic/hospital/healthcare center. It is the
/// root of the multi-tenant model — every tenant-scoped entity references it.
/// </summary>
public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Unique slug used for subdomain / tenant resolution (e.g. "smile-dental").</summary>
    public string Slug { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Currency { get; set; } = "INR";

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public Subscription? Subscription { get; set; }
}
