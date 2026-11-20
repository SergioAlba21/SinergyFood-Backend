using InsumoPro.Application.Features.Diners.DTOs;

namespace InsumoPro.Application.Features.Diners.DTOs;

public class CheckInResponse
{
    public bool Allowed { get; set; }
    public string Message { get; set; } = null!;
    public string? EmployeeNumber { get; set; }
    public string? DinerName { get; set; }
    public string? MealTypeName { get; set; }
    public DateTime? ValidAt { get; set; }
    public int ConsumptionsToday { get; set; }
    public int MaxPerDay { get; set; }
    public TicketDto? Ticket { get; set; }
}