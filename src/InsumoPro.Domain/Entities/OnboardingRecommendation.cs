using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class OnboardingRecommendation : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public int? RecommendedPlanId { get; set; }
    public string? Reason { get; set; }
    public int ScoreBasic { get; set; }
    public int ScoreProduction { get; set; }
    public int ScoreComplete { get; set; }

    public Company Company { get; set; } = null!;
    public Plan? RecommendedPlan { get; set; }
}