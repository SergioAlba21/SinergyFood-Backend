using InsumoPro.Application.Features.Recipes.DTOs;

namespace InsumoPro.Application.Common.Interfaces;

public interface IRecipeService
{
    Task<List<RecipeDto>> GetAllAsync(Guid companyId);
    Task<RecipeDto?> GetByIdAsync(Guid companyId, Guid id);
    Task<RecipeDto> CreateAsync(Guid companyId, CreateRecipeRequest request);
    Task<RecipeDto?> UpdateAsync(Guid companyId, Guid id, UpdateRecipeRequest request);
    Task<bool> DeleteAsync(Guid companyId, Guid id);
}