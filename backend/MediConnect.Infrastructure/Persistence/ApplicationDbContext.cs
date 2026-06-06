using System.Reflection;
using MediConnect.Application.Common.Interfaces;
using MediConnect.Domain.Common;
using MediConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Persistence;

/// <summary>
/// The application's EF Core DbContext. Enforces multi-tenant isolation through
/// global query filters keyed on the current tenant, applies soft-delete filters,
/// and automatically populates auditing fields on save.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUser _currentUser;

    // These instance fields are referenced directly by the global query filters.
    // EF Core detects member access on the context instance and re-evaluates the
    // values for every query execution, so the cached model stays correct even
    // though each scoped DbContext represents a different tenant/request.
    private readonly Guid _currentTenantId;
    private readonly bool _isSuperAdmin;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenantProvider tenant,
        ICurrentUser currentUser) : base(options)
    {
        _currentUser = currentUser;
        _currentTenantId = tenant.TenantId ?? Guid.Empty;
        _isSuperAdmin = tenant.IsSuperAdmin;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<DoctorTimeOff> DoctorTimeOffs => Set<DoctorTimeOff>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentAttachment> AppointmentAttachments => Set<AppointmentAttachment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<MedicalRecordAttachment> MedicalRecordAttachments => Set<MedicalRecordAttachment>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<PrescriptionTemplate> PrescriptionTemplates => Set<PrescriptionTemplate>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (!typeof(BaseEntity).IsAssignableFrom(clrType))
                continue;

            var method = typeof(ApplicationDbContext)
                .GetMethod(
                    typeof(ITenantScoped).IsAssignableFrom(clrType)
                        ? nameof(SetTenantFilter)
                        : nameof(SetSoftDeleteFilter),
                    BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(clrType);

            method.Invoke(this, new object[] { modelBuilder });
        }

        base.OnModelCreating(modelBuilder);
    }

    // Soft-delete-only filter for non tenant-scoped entities (Tenant, plans, etc.).
    private void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    // Combined soft-delete + tenant-isolation filter. Super Admins bypass tenant scoping.
    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : BaseEntity, ITenantScoped
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            !e.IsDeleted && (_isSuperAdmin || e.TenantId == _currentTenantId));
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInfo()
    {
        var userId = _currentUser.UserId;
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = now;
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = now;
                    entry.Entity.UpdatedBy = userId;
                    break;

                case EntityState.Deleted:
                    // Convert hard deletes to soft deletes.
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAtUtc = now;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }
    }
}
