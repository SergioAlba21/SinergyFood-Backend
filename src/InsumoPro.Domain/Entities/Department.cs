using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class Department : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;
    public ICollection<Diner> Diners { get; set; } = new List<Diner>();
}