namespace InsumoPro.Application.Features.Stock.DTOs;

public class ProductStockDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Sku { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal MinStock { get; set; }
    public bool IsBelowMin { get; set; }
}