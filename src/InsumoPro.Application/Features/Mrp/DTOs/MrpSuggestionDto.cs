namespace InsumoPro.Application.Features.Mrp.DTOs;

public class MrpSuggestionDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Sku { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public decimal SuggestedQuantity { get; set; }
    public decimal DemandFromOrders { get; set; }
    public decimal DemandFromForecast { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal SafetyStock { get; set; }
    public decimal IncomingPurchases { get; set; }
    public DateTime? SuggestedOrderDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = null!;
}

public class ConvertSuggestionRequest
{
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal? Quantity { get; set; } // si null, usa suggested_quantity
    public decimal UnitCost { get; set; }
}