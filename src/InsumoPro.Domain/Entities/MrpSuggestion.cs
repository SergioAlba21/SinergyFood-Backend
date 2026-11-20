using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class MrpSuggestion : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? WarehouseId { get; set; }
    public decimal SuggestedQuantity { get; set; }
    public decimal DemandFromOrders { get; set; }
    public decimal DemandFromForecast { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal SafetyStock { get; set; }
    public decimal IncomingPurchases { get; set; }
    public DateTime? SuggestedOrderDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "pending"; // pending, converted, dismissed
    public Guid? PurchaseOrderId { get; set; }

    public Company Company { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Warehouse? Warehouse { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
}