using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Suppliers.DTOs;
using InsumoPro.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _service;
    public SuppliersController(ISupplierService service) => _service = service;
    private Guid CompanyId => Guid.Parse(User.FindFirstValue("company_id")!);

    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(CompanyId));
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var r = await _service.GetByIdAsync(CompanyId, id);
        return r == null ? NotFound() : Ok(r);
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest req)
        => Ok(await _service.CreateAsync(CompanyId, req));
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest req)
    {
        var r = await _service.UpdateAsync(CompanyId, id, req);
        return r == null ? NotFound() : Ok(r);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => await _service.DeleteAsync(CompanyId, id) ? NoContent() : NotFound();
}