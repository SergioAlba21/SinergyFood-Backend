using InsumoPro.Domain.Common;

namespace InsumoPro.Domain.Entities;

public class RecipeItem : BaseEntity
{
    public Guid RecipeId { get; set; }
    public Guid ProductId { get; set; }                 // Insumo
    public decimal Quantity { get; set; }
    public int? UnitId { get; set; }
    public int SortOrder { get; set; }

    public Recipe Recipe { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Unit? Unit { get; set; }
}