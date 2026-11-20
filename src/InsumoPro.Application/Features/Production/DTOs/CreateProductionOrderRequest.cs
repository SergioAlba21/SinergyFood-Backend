namespace InsumoPro.Application.Features.Production.DTOs;

public class CreateProductionOrderRequest
{
    public Guid RecipeId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal QuantityPlanned { get; set; }
    public DateTime? ProductionDate { get; set; }
    public string? Notes { get; set; }
}