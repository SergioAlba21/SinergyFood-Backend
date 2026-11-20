namespace InsumoPro.Application.Features.Products.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string? Sku { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!;          // raw_material, finished_good, both
    public int? UnitId { get; set; }
    public string? UnitName { get; set; }
    public decimal CostAverage { get; set; }
    public decimal PriceSale { get; set; }
    public decimal MinStock { get; set; }
    public bool HasExpiry { get; set; }
    public bool IsActive { get; set; }
}