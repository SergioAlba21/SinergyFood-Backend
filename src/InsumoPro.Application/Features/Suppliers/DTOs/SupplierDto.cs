namespace InsumoPro.Application.Features.Suppliers.DTOs;

public class SupplierDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int LeadTimeDays { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSupplierRequest
{
    public string Name { get; set; } = null!;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int LeadTimeDays { get; set; } = 3;
    public string? Notes { get; set; }
}

public class UpdateSupplierRequest
{
    public string Name { get; set; } = null!;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int LeadTimeDays { get; set; } = 3;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}