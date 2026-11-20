namespace InsumoPro.Application.Features.Warehouses.DTOs;

public class CreateWarehouseRequest
{
    public string Name { get; set; } = null!;
    public bool IsDefault { get; set; }
    public string? Address { get; set; }
}