using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class ProductionOrder : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid? RecipeId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal QuantityPlanned { get; set; }
    public decimal QuantityProduced { get; set; }
    public string Status { get; set; } = "draft";       // draft, confirmed, completed, cancelled
    public DateTime? ProductionDate { get; set; }
    public string? Notes { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Company Company { get; set; } = null!;
    public Recipe? Recipe { get; set; }
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public User? Creator { get; set; }
}