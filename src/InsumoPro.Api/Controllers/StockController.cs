using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Stock.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsumoPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    private Guid GetCompanyId() => Guid.Parse(User.FindFirstValue("company_id")!);
    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Consultar existencias actuales
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ProductStockDto>>> GetStock([FromQuery] Guid? warehouseId = null)
    {
        var result = await _stockService.GetStockAsync(GetCompanyId(), warehouseId);
        return Ok(result);
    }

    /// <summary>
    /// Historial de movimientos
    /// </summary>
    [HttpGet("movements")]
    public async Task<ActionResult<List<StockMovementDto>>> GetMovements(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? warehouseId = null)
    {
        var result = await _stockService.GetMovementsAsync(GetCompanyId(), productId, warehouseId);
        return Ok(result);
    }

    /// <summary>
    /// Crear movimiento de stock (entrada, salida o ajuste)
    /// </summary>
    [HttpPost("movements")]
    public async Task<ActionResult<StockMovementDto>> CreateMovement([FromBody] CreateStockMovementRequest request)
    {
        try
        {
            var result = await _stockService.CreateMovementAsync(GetCompanyId(), GetUserId(), request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}