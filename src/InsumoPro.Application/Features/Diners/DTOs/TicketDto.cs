namespace InsumoPro.Application.Features.Diners.DTOs;

public class TicketDto
{
    public string Title { get; set; } = "COMEDOR";
    public string EmployeeNumber { get; set; } = null!;
    public string DinerName { get; set; } = null!;
    public string MealTypeName { get; set; } = null!;
    public DateTime IssuedAt { get; set; }
    public string ValidFor { get; set; } = "1 consumo";
    public string Folio { get; set; } = null!;
}

public class PendingTicketDto
{
    public Guid ConsumptionId { get; set; }
    public string Folio { get; set; } = null!;
    public string EmployeeNumber { get; set; } = null!;
    public string DinerName { get; set; } = null!;
    public string MealTypeName { get; set; } = null!;
    public DateTime IssuedAt { get; set; }
    public string ValidFor { get; set; } = "1 consumo";
    public string Title { get; set; } = "COMEDOR";
}