namespace InsumoPro.Domain.Entities;

public class BusinessProfile
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool EnablesRecipes { get; set; } = true;
    public bool EnablesProduction { get; set; } = true;
    public bool EnablesExpiry { get; set; } = false;
    public int? DefaultPlanId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Plan? DefaultPlan { get; set; }
}