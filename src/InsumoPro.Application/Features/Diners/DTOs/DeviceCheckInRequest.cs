namespace InsumoPro.Application.Features.Diners.DTOs;

public class DeviceCheckInRequest
{
    public string EmployeeNumber { get; set; } = null!;
    public DateTime? PunchedAt { get; set; }
    public Guid? MealTypeId { get; set; }
    public string? DeviceSerial { get; set; }
}