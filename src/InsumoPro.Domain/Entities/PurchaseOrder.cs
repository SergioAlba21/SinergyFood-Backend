using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class PurchaseOrder : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string Status { get; set; } = "draft"; // draft, ordered, partial, received, cancelled
    public DateTime OrderDate { get; set; } = DateTime.Now.Date;
    public DateTime? ExpectedDate { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public Guid? CreatedBy { get; set; }

    public Company Company { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public Warehouse? Warehouse { get; set; }
    public User? Creator { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}