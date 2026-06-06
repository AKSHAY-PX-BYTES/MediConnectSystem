using MediConnect.Application.Common.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Domain.Enums;
using MediConnect.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Persistence;

/// <summary>
/// Seeds baseline platform data: subscription plans, a Super Admin account, and a
/// demo clinic with sample doctors/patients so the API is usable immediately.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, IPasswordHasher hasher)
    {
        await db.Database.MigrateAsync();

        // ---- Subscription plans ----
        if (!await db.SubscriptionPlans.AnyAsync())
        {
            db.SubscriptionPlans.AddRange(
                new SubscriptionPlan { Name = "Starter", Tier = SubscriptionPlanTier.Starter, MonthlyPrice = 0, YearlyPrice = 0, MaxDoctors = 3, MaxStaff = 5, MaxPatients = 1000 },
                new SubscriptionPlan { Name = "Professional", Tier = SubscriptionPlanTier.Professional, MonthlyPrice = 49, YearlyPrice = 490, MaxDoctors = 10, MaxStaff = 25, MaxPatients = 10000 },
                new SubscriptionPlan { Name = "Business", Tier = SubscriptionPlanTier.Business, MonthlyPrice = 149, YearlyPrice = 1490, MaxDoctors = -1, MaxStaff = -1, MaxPatients = -1 },
                new SubscriptionPlan { Name = "Enterprise", Tier = SubscriptionPlanTier.Enterprise, MonthlyPrice = 0, YearlyPrice = 0, MaxDoctors = -1, MaxStaff = -1, MaxPatients = -1 });
            await db.SaveChangesAsync();
        }

        // ---- Super Admin (tenant-less) ----
        if (!await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Role == UserRole.SuperAdmin))
        {
            db.Users.Add(new ApplicationUser
            {
                TenantId = Guid.Empty,
                Email = "superadmin@mediconnect.io",
                PasswordHash = hasher.Hash("SuperAdmin@123"),
                FirstName = "Platform",
                LastName = "Admin",
                Role = UserRole.SuperAdmin,
                IsActive = true,
                EmailConfirmed = true
            });
            await db.SaveChangesAsync();
        }

        // ---- Demo clinic ----
        if (!await db.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Slug == "demo-clinic"))
        {
            var tenant = new Tenant
            {
                Name = "Demo Clinic",
                Slug = "demo-clinic",
                ContactEmail = "admin@democlinic.io",
                City = "Mumbai",
                Country = "India",
                Currency = "INR",
                IsActive = true
            };
            db.Tenants.Add(tenant);

            var starter = await db.SubscriptionPlans.FirstAsync(p => p.Tier == SubscriptionPlanTier.Starter);
            db.Subscriptions.Add(new Subscription
            {
                TenantId = tenant.Id,
                PlanId = starter.Id,
                Status = SubscriptionStatus.Active,
                StartUtc = DateTime.UtcNow,
                CurrentPeriodEndUtc = DateTime.UtcNow.AddYears(1)
            });

            db.Users.Add(new ApplicationUser
            {
                TenantId = tenant.Id,
                Email = "admin@democlinic.io",
                PasswordHash = hasher.Hash("Admin@123"),
                FirstName = "Demo",
                LastName = "Admin",
                Role = UserRole.ClinicAdmin,
                IsActive = true,
                EmailConfirmed = true
            });

            var dental = new Department { TenantId = tenant.Id, Name = "Dental", Description = "Dental care" };
            var general = new Department { TenantId = tenant.Id, Name = "General Medicine", Description = "General consultations" };
            db.Departments.AddRange(dental, general);

            db.Doctors.AddRange(
                new Doctor
                {
                    TenantId = tenant.Id, FirstName = "Aisha", LastName = "Khan",
                    Specialization = "Dentist", DepartmentId = dental.Id,
                    ConsultationFee = 500, ExperienceYears = 8, DefaultSlotDurationMinutes = 30
                },
                new Doctor
                {
                    TenantId = tenant.Id, FirstName = "Rohan", LastName = "Mehta",
                    Specialization = "General Physician", DepartmentId = general.Id,
                    ConsultationFee = 400, ExperienceYears = 12, DefaultSlotDurationMinutes = 15
                });

            db.Patients.Add(new Patient
            {
                TenantId = tenant.Id, FirstName = "Sam", LastName = "Patel",
                Gender = GenderType.Male, Email = "sam@example.com",
                MedicalRecordNumber = "MRN-DEMO-0001", IsActive = true
            });

            await db.SaveChangesAsync();
        }
    }
}
