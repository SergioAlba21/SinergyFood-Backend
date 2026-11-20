namespace InsumoPro.Application.Features.Stock.DTOs;

public class CreateStockMovementRequest
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string Type { get; set; } = null!;          // adjustment, purchase, sale, etc.
    public decimal Quantity { get; set; }              // Siempre positivo, el Type define si suma o resta
    public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }
}