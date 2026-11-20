using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Warehouses.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsumoPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    private Guid GetCompanyId() => Guid.Parse(User.FindFirstValue("company_id")!);

    [HttpGet]
    public async Task<ActionResult<List<WarehouseDto>>> GetAll()
    {
        var result = await _warehouseService.GetAllAsync(GetCompanyId());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WarehouseDto>> GetById(Guid id)
    {
        var result = await _warehouseService.GetByIdAsync(GetCompanyId(), id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> Create([FromBody] CreateWarehouseRequest request)
    {
        var result = await _warehouseService.CreateAsync(GetCompanyId(), request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WarehouseDto>> Update(Guid id, [FromBody] UpdateWarehouseRequest request)
    {
        var result = await _warehouseService.UpdateAsync(GetCompanyId(), id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _warehouseService.DeleteAsync(GetCompanyId(), id);
        if (!success) return NotFound();
        return NoContent();
    }
}