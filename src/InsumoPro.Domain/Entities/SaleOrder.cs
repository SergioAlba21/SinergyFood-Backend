using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class SaleOrder : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string Status { get; set; } = "draft"; // draft, confirmed, cancelled
    public DateTime OrderDate { get; set; } = DateTime.Now.Date;
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public Guid? CreatedBy { get; set; }

    public Company Company { get; set; } = null!;
    public Customer? Customer { get; set; }
    public Warehouse? Warehouse { get; set; }
    public User? Creator { get; set; }
    public ICollection<SaleOrderItem> Items { get; set; } = new List<SaleOrderItem>();
}