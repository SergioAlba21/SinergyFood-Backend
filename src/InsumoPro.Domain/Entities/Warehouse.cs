using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class Warehouse : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDefault { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;
}