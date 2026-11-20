using InsumoPro.Domain.Common;
using System.Numerics;

namespace InsumoPro.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? LegalName { get; set; }
    public string? Rfc { get; set; }
    public string Country { get; set; } = "MX";
    public string Currency { get; set; } = "MXN";
    public string? LogoUrl { get; set; }

    public int? BusinessProfileId { get; set; }
    public int? PlanId { get; set; }
    public int? RecommendedPlanId { get; set; }
    public bool OnboardingCompleted { get; set; } = false;

    // Navigation
    public BusinessProfile? BusinessProfile { get; set; }
    public Plan? Plan { get; set; }
    public Plan? RecommendedPlan { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
}