namespace InsumoPro.Application.Features.Warehouses.DTOs;

public class UpdateWarehouseRequest
{
    public string Name { get; set; } = null!;
    public bool IsDefault { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}