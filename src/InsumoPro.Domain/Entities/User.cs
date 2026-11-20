using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class User : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "admin"; // admin, operator, seller, viewer
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public Company Company { get; set; } = null!;
}