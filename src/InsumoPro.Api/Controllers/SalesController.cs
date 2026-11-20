using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Sales.DTOs;
using InsumoPro.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISaleService _service;
    public SalesController(ISaleService service) => _service = service;
    private Guid CompanyId => Guid.Parse(User.FindFirstValue("company_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(CompanyId));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var r = await _service.GetByIdAsync(CompanyId, id);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleOrderRequest request)
    {
        try { return Ok(await _service.CreateAndConfirmAsync(CompanyId, UserId, request)); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try { return await _service.CancelAsync(CompanyId, id) ? NoContent() : NotFound(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}