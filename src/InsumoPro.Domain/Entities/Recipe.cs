using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class Recipe : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }                 // Producto terminado
    public string Name { get; set; } = null!;
    public decimal YieldQuantity { get; set; } = 1;     // Cuánto produce la receta
    public int? YieldUnitId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Unit? YieldUnit { get; set; }
    public ICollection<RecipeItem> Items { get; set; } = new List<RecipeItem>();
}