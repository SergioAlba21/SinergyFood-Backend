namespace InsumoPro.Application.Features.Purchases.DTOs;

public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string Status { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

public class PurchaseOrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreatePurchaseOrderRequest
{
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseOrderItemRequest> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemRequest
{
    public Guid ProductId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal UnitCost { get; set; }
}

public class ReceivePurchaseOrderRequest
{
    public List<ReceivePurchaseOrderItemRequest> Items { get; set; } = new();
    public string? Notes { get; set; }
}

public class ReceivePurchaseOrderItemRequest
{
    public Guid ProductId { get; set; }
    public decimal QuantityReceived { get; set; }
}