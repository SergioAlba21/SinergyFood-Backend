namespace InsumoPro.Application.Features.Diners.DTOs;

public class CreateProvisionalCouponRequest
{
    public string? GuestName { get; set; }
    public string? GuestNote { get; set; }
    public Guid? MealTypeId { get; set; }
}