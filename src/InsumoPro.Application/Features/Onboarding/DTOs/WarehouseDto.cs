namespace InsumoPro.Application.Features.Warehouses.DTOs;

public class WarehouseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDefault { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}