namespace InsumoPro.Application.Features.Recipes.DTOs;

public class CreateRecipeRequest
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = null!;
    public decimal YieldQuantity { get; set; } = 1;
    public int? YieldUnitId { get; set; }
    public string? Notes { get; set; }
    public List<CreateRecipeItemRequest> Items { get; set; } = new();
}

public class CreateRecipeItemRequest
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public int? UnitId { get; set; }
}