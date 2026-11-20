using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Recipes.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Infrastructure.Services;

public class RecipeService : IRecipeService
{
    private readonly ApplicationDbContext _context;

    public RecipeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecipeDto>> GetAllAsync(Guid companyId)
    {
        var recipes = await _context.Recipes
            .Where(r => r.CompanyId == companyId)
            .Include(r => r.Product)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .Include(r => r.Items).ThenInclude(i => i.Unit)
            .ToListAsync();

        return recipes.Select(MapToDto).ToList();
    }

    public async Task<RecipeDto?> GetByIdAsync(Guid companyId, Guid id)
    {
        var recipe = await _context.Recipes
            .Include(r => r.Product)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .Include(r => r.Items).ThenInclude(i => i.Unit)
            .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

        return recipe == null ? null : MapToDto(recipe);
    }

    public async Task<RecipeDto> CreateAsync(Guid companyId, CreateRecipeRequest request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.CompanyId == companyId)
            ?? throw new Exception("Producto terminado no encontrado");

        var recipe = new Recipe
        {
            CompanyId = companyId,
            ProductId = request.ProductId,
            Name = request.Name,
            YieldQuantity = request.YieldQuantity,
            YieldUnitId = request.YieldUnitId,
            Notes = request.Notes,
            IsActive = true
        };

        foreach (var item in request.Items)
        {
            recipe.Items.Add(new RecipeItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitId = item.UnitId
            });
        }

        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(companyId, recipe.Id))!;
    }

    public async Task<RecipeDto?> UpdateAsync(Guid companyId, Guid id, UpdateRecipeRequest request)
    {
        var recipe = await _context.Recipes
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

        if (recipe == null) return null;

        recipe.Name = request.Name;
        recipe.YieldQuantity = request.YieldQuantity;
        recipe.YieldUnitId = request.YieldUnitId;
        recipe.Notes = request.Notes;
        recipe.IsActive = request.IsActive;

        // Reemplazar items
        _context.RecipeItems.RemoveRange(recipe.Items);
        recipe.Items.Clear();

        foreach (var item in request.Items)
        {
            recipe.Items.Add(new RecipeItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitId = item.UnitId
            });
        }

        await _context.SaveChangesAsync();
        return await GetByIdAsync(companyId, recipe.Id);
    }

    public async Task<bool> DeleteAsync(Guid companyId, Guid id)
    {
        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

        if (recipe == null) return false;

        recipe.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    private static RecipeDto MapToDto(Recipe recipe)
    {
        var items = recipe.Items.Select(i => new RecipeItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? "",
            Quantity = i.Quantity,
            UnitId = i.UnitId,
            UnitName = i.Unit?.Name,
            UnitCost = i.Product?.CostAverage ?? 0,
            LineCost = i.Quantity * (i.Product?.CostAverage ?? 0)
        }).ToList();

        return new RecipeDto
        {
            Id = recipe.Id,
            ProductId = recipe.ProductId,
            ProductName = recipe.Product?.Name ?? "",
            Name = recipe.Name,
            YieldQuantity = recipe.YieldQuantity,
            YieldUnitId = recipe.YieldUnitId,
            Notes = recipe.Notes,
            IsActive = recipe.IsActive,
            EstimatedCost = items.Sum(i => i.LineCost),
            Items = items
        };
    }
}