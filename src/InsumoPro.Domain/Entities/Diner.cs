using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class Diner : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PhotoUrl { get; set; }
    public string? AccessCode { get; set; }            // QR / Tarjeta / Código
    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;
    public Department? Department { get; set; }
    public ICollection<DinerAccessRule> AccessRules { get; set; } = new List<DinerAccessRule>();
    public ICollection<DinerConsumption> Consumptions { get; set; } = new List<DinerConsumption>();
}