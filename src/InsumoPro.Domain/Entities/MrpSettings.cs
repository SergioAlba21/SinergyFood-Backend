using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class MrpSettings : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public int ForecastWeeks { get; set; } = 4;
    public bool ConsiderOpenSales { get; set; } = true;
    public bool ConsiderOpenProduction { get; set; } = true;

    public Company Company { get; set; } = null!;
}