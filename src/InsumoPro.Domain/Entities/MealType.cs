using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class MealType : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = null!;          // Desayuno, Comida, Cena
    public string? Code { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int MaxPerDay { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;
}