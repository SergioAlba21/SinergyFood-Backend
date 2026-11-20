using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class Product : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string? Sku { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!;           // raw_material, finished_good, both
    public int? UnitId { get; set; }
    public decimal CostAverage { get; set; }
    public decimal PriceSale { get; set; }
    public decimal MinStock { get; set; }
    public bool HasExpiry { get; set; }
    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;
    public Unit? Unit { get; set; }
}