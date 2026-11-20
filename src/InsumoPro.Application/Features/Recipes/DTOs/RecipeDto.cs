namespace InsumoPro.Application.Features.Recipes.DTOs;

public class RecipeDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal YieldQuantity { get; set; }
    public int? YieldUnitId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public decimal EstimatedCost { get; set; }
    public List<RecipeItemDto> Items { get; set; } = new();
}

public class RecipeItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public int? UnitId { get; set; }
    public string? UnitName { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineCost { get; set; }
}