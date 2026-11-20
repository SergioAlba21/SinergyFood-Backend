using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Purchases.DTOs;
using InsumoPro.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _service;
    public PurchasesController(IPurchaseService service) => _service = service;
    private Guid CompanyId => Guid.Parse(User.FindFirstValue("company_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(CompanyId));
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var r = await _service.GetByIdAsync(CompanyId, id);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest req)
    {
        try { return Ok(await _service.CreateAsync(CompanyId, UserId, req)); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id}/receive")]
    public async Task<IActionResult> Receive(Guid id, [FromBody] ReceivePurchaseOrderRequest req)
    {
        try
        {
            var r = await _service.ReceiveAsync(CompanyId, UserId, id, req);
            return r == null ? NotFound() : Ok(r);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try { return await _service.CancelAsync(CompanyId, id) ? NoContent() : NotFound(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}