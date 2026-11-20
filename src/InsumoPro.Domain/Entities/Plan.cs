namespace InsumoPro.Domain.Entities;

public class Plan
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;          // basic, production, complete
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal PriceMonthly { get; set; }
    public decimal? PriceYearly { get; set; }
    public int MaxUsers { get; set; } = 3;
    public int MaxWarehouses { get; set; } = 1;
    public string Features { get; set; } = "[]";        // JSON
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}