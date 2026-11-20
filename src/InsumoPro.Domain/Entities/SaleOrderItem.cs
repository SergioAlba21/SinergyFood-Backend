using InsumoPro.Domain.Common;

namespace InsumoPro.Domain.Entities;

public class SaleOrderItem : BaseEntity
{
    public Guid SaleOrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int? UnitId { get; set; }

    public SaleOrder SaleOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Unit? Unit { get; set; }
}