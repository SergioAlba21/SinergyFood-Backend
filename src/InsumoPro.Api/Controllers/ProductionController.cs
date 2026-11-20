using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Production.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsumoPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductionController : ControllerBase
{
    private readonly IProductionService _productionService;

    public ProductionController(IProductionService productionService)
    {
        _productionService = productionService;
    }

    private Guid GetCompanyId() => Guid.Parse(User.FindFirstValue("company_id")!);
    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<ProductionOrderDto>>> GetAll()
        => Ok(await _productionService.GetAllAsync(GetCompanyId()));

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductionOrderDto>> GetById(Guid id)
    {
        var result = await _productionService.GetByIdAsync(GetCompanyId(), id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductionOrderDto>> Create([FromBody] CreateProductionOrderRequest request)
    {
        try
        {
            var result = await _productionService.CreateAsync(GetCompanyId(), GetUserId(), request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<ProductionOrderDto>> Complete(Guid id, [FromBody] CompleteProductionOrderRequest request)
    {
        try
        {
            var result = await _productionService.CompleteAsync(GetCompanyId(), GetUserId(), id, request);
            return result == null ? NotFound() : Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> Cancel(Guid id)
    {
        try
        {
            var ok = await _productionService.CancelAsync(GetCompanyId(), id);
            return ok ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}