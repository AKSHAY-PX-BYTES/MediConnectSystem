using MediConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediConnect.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("Tenants");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(120);
        b.HasIndex(x => x.Slug).IsUnique();
        b.HasOne(x => x.Subscription)
            .WithOne(s => s.Tenant!)
            .HasForeignKey<Subscription>(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.ToTable("Users");
        b.HasKey(x => x.Id);
        b.Property(x => x.Email).IsRequired().HasMaxLength(256);
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.PasswordHash).IsRequired();
        b.Property(x => x.FirstName).HasMaxLength(100);
        b.Property(x => x.LastName).HasMaxLength(100);
        b.HasMany(x => x.RefreshTokens)
            .WithOne(t => t.User!)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.TenantId);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens");
        b.HasKey(x => x.Id);
        b.Property(x => x.Token).IsRequired();
        b.HasIndex(x => x.Token);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.ToTable("Departments");
        b.Property(x => x.Name).IsRequired().HasMaxLength(150);
        b.HasIndex(x => new { x.TenantId, x.Name });
    }
}

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> b)
    {
        b.ToTable("Doctors");
        b.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        b.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        b.Property(x => x.ConsultationFee).HasPrecision(18, 2);
        b.HasOne(x => x.Department)
            .WithMany(d => d.Doctors)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => x.TenantId);
    }
}

public class DoctorAvailabilityConfiguration : IEntityTypeConfiguration<DoctorAvailability>
{
    public void Configure(EntityTypeBuilder<DoctorAvailability> b)
    {
        b.ToTable("DoctorAvailabilities");
        b.HasOne(x => x.Doctor)
            .WithMany(d => d.Availabilities)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.DoctorId, x.DayOfWeek });
    }
}

public class DoctorTimeOffConfiguration : IEntityTypeConfiguration<DoctorTimeOff>
{
    public void Configure(EntityTypeBuilder<DoctorTimeOff> b)
    {
        b.ToTable("DoctorTimeOffs");
        b.HasOne(x => x.Doctor)
            .WithMany(d => d.TimeOffs)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> b)
    {
        b.ToTable("Patients");
        b.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        b.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        b.Property(x => x.MedicalRecordNumber).HasMaxLength(50);
        b.HasIndex(x => new { x.TenantId, x.MedicalRecordNumber });
        b.HasIndex(x => x.TenantId);
    }
}

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> b)
    {
        b.ToTable("Appointments");
        b.HasOne(x => x.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
        b.HasMany(x => x.Attachments)
            .WithOne(a => a.Appointment!)
            .HasForeignKey(a => a.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.TenantId, x.DoctorId, x.AppointmentDate });
        b.HasIndex(x => new { x.TenantId, x.PatientId });
    }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> b)
    {
        b.ToTable("Invoices");
        b.Property(x => x.InvoiceNumber).HasMaxLength(50);
        b.Property(x => x.SubTotal).HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.TaxPercent).HasPrecision(5, 2);
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.Property(x => x.Total).HasPrecision(18, 2);
        b.Property(x => x.AmountPaid).HasPrecision(18, 2);
        b.HasMany(x => x.Items).WithOne(i => i.Invoice!).HasForeignKey(i => i.InvoiceId);
        b.HasMany(x => x.Payments).WithOne(p => p.Invoice!).HasForeignKey(p => p.InvoiceId);
        b.HasIndex(x => new { x.TenantId, x.InvoiceNumber }).IsUnique();
    }
}

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> b)
    {
        b.ToTable("InvoiceItems");
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Ignore(x => x.LineTotal);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("Payments");
        b.Property(x => x.Amount).HasPrecision(18, 2);
    }
}

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> b)
    {
        b.ToTable("SubscriptionPlans");
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.MonthlyPrice).HasPrecision(18, 2);
        b.Property(x => x.YearlyPrice).HasPrecision(18, 2);
    }
}

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> b)
    {
        b.ToTable("Subscriptions");
        b.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> b)
    {
        b.ToTable("Prescriptions");
        b.HasMany(x => x.Items).WithOne(i => i.Prescription!).HasForeignKey(i => i.PrescriptionId);
        b.HasOne(x => x.Patient).WithMany(p => p.Prescriptions).HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> b)
    {
        b.ToTable("MedicalRecords");
        b.HasOne(x => x.Patient).WithMany(p => p.MedicalRecords).HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Attachments).WithOne(a => a.MedicalRecord!).HasForeignKey(a => a.MedicalRecordId);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.Property(x => x.Action).HasMaxLength(100);
        b.Property(x => x.EntityName).HasMaxLength(150);
        b.HasIndex(x => new { x.TenantId, x.CreatedAtUtc });
    }
}
