namespace InsumoPro.Application.Features.Recipes.DTOs;

public class UpdateRecipeRequest
{
    public string Name { get; set; } = null!;
    public decimal YieldQuantity { get; set; } = 1;
    public int? YieldUnitId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public List<CreateRecipeItemRequest> Items { get; set; } = new();
}