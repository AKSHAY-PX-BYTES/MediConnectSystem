using MediConnect.Domain.Common;
using MediConnect.Domain.Enums;

namespace MediConnect.Domain.Entities;

/// <summary>A subscription plan definition managed by the Super Admin.</summary>
public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public SubscriptionPlanTier Tier { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }

    /// <summary>-1 represents unlimited.</summary>
    public int MaxDoctors { get; set; }
    public int MaxStaff { get; set; }
    public int MaxPatients { get; set; }
    public bool IsActive { get; set; } = true;
    public string? FeaturesJson { get; set; }
}

/// <summary>A tenant's active subscription to a plan.</summary>
public class Subscription : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trialing;
    public DateTime StartUtc { get; set; } = DateTime.UtcNow;
    public DateTime? TrialEndsUtc { get; set; }
    public DateTime? CurrentPeriodEndUtc { get; set; }
    public bool AutoRenew { get; set; } = true;

    public Tenant? Tenant { get; set; }
    public SubscriptionPlan? Plan { get; set; }
}
