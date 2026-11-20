namespace InsumoPro.Application.Features.Production.DTOs;

public class ProductionOrderDto
{
    public Guid Id { get; set; }
    public Guid? RecipeId { get; set; }
    public string? RecipeName { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public decimal QuantityPlanned { get; set; }
    public decimal QuantityProduced { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? ProductionDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}