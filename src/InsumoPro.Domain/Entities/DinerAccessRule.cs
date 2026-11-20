using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class DinerAccessRule : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid DinerId { get; set; }
    public Guid MealTypeId { get; set; }
    public int MaxPerDay { get; set; } = 1;
    public bool IsAllowed { get; set; } = true;

    public Company Company { get; set; } = null!;
    public Diner Diner { get; set; } = null!;
    public MealType MealType { get; set; } = null!;
}