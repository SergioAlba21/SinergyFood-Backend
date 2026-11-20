using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class StockMovement : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string Type { get; set; } = null!;          // purchase, sale, production_in, production_out, adjustment, transfer
    public decimal Quantity { get; set; }              // Positivo = entrada, Negativo = salida
    public decimal? UnitCost { get; set; }
    public string? ReferenceType { get; set; }         // purchase_order, production_order, sale_order, etc.
    public Guid? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public Guid? CreatedBy { get; set; }

    public Company Company { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public User? Creator { get; set; }
}