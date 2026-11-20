using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class DinerConsumption : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid? DinerId { get; set; }
    public Guid MealTypeId { get; set; }
    public Guid? ProductId { get; set; }               // Platillo consumido (opcional)
    public decimal Quantity { get; set; } = 1;
    public DateTime ConsumedAt { get; set; } = DateTime.Now;
    public Guid? ValidatedBy { get; set; }             // Usuario que validó
    public string? Notes { get; set; }

    public Company Company { get; set; } = null!;
    public Diner? Diner { get; set; }
    public MealType MealType { get; set; } = null!;
    public Product? Product { get; set; }
    public User? Validator { get; set; }

    public string? Folio { get; set; }
    public bool Printed { get; set; } = false;
    public DateTime? PrintedAt { get; set; }

    // Cupón provisional
    public bool IsProvisional { get; set; } = false;
    public string? GuestName { get; set; }
    public string? GuestNote { get; set; }

}