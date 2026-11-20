using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Recipes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsumoPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _recipeService;

    public RecipesController(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    private Guid GetCompanyId() => Guid.Parse(User.FindFirstValue("company_id")!);

    [HttpGet]
    public async Task<ActionResult<List<RecipeDto>>> GetAll()
        => Ok(await _recipeService.GetAllAsync(GetCompanyId()));

    [HttpGet("{id}")]
    public async Task<ActionResult<RecipeDto>> GetById(Guid id)
    {
        var result = await _recipeService.GetByIdAsync(GetCompanyId(), id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RecipeDto>> Create([FromBody] CreateRecipeRequest request)
    {
        try
        {
            var result = await _recipeService.CreateAsync(GetCompanyId(), request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RecipeDto>> Update(Guid id, [FromBody] UpdateRecipeRequest request)
    {
        var result = await _recipeService.UpdateAsync(GetCompanyId(), id, request);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var ok = await _recipeService.DeleteAsync(GetCompanyId(), id);
        return ok ? NoContent() : NotFound();
    }
}