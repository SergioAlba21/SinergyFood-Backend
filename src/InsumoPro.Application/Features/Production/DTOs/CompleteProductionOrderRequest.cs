namespace InsumoPro.Application.Features.Production.DTOs;

public class CompleteProductionOrderRequest
{
    public decimal QuantityProduced { get; set; }
    public string? Notes { get; set; }
}