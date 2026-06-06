using MediConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext so the Application layer remains
/// persistence-ignorant. All tenant-scoped DbSets are automatically filtered
/// by the current tenant via global query filters in the implementation.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Department> Departments { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<DoctorAvailability> DoctorAvailabilities { get; }
    DbSet<DoctorTimeOff> DoctorTimeOffs { get; }
    DbSet<Patient> Patients { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<AppointmentAttachment> AppointmentAttachments { get; }
    DbSet<MedicalRecord> MedicalRecords { get; }
    DbSet<MedicalRecordAttachment> MedicalRecordAttachments { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<PrescriptionItem> PrescriptionItems { get; }
    DbSet<PrescriptionTemplate> PrescriptionTemplates { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
