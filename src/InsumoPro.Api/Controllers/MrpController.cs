using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Mrp.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsumoPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MrpController : ControllerBase
{
    private readonly IMrpService _mrpService;

    public MrpController(IMrpService mrpService)
    {
        _mrpService = mrpService;
    }

    private Guid CompanyId => Guid.Parse(User.FindFirstValue("company_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Genera sugerencias nuevas de compra
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate()
    {
        var result = await _mrpService.GenerateSuggestionsAsync(CompanyId);
        return Ok(result);
    }

    /// <summary>
    /// Lista sugerencias
    /// </summary>
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions([FromQuery] string? status = "pending")
    {
        var result = await _mrpService.GetSuggestionsAsync(CompanyId, status);
        return Ok(result);
    }

    /// <summary>
    /// Convierte sugerencia en orden de compra
    /// </summary>
    [HttpPost("suggestions/{id}/convert")]
    public async Task<IActionResult> Convert(Guid id, [FromBody] ConvertSuggestionRequest request)
    {
        try
        {
            var result = await _mrpService.ConvertToPurchaseOrderAsync(CompanyId, UserId, id, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Descartar sugerencia
    /// </summary>
    [HttpPost("suggestions/{id}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid id)
    {
        var ok = await _mrpService.DismissAsync(CompanyId, id);
        return ok ? NoContent() : NotFound();
    }
}