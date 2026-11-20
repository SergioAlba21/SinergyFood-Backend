using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Diners.DTOs;
using InsumoPro.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsumoPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DinersController : ControllerBase
{
    private readonly IDinerService _service;

    public DinersController(IDinerService service) => _service = service;

    private Guid CompanyId => Guid.Parse(User.FindFirstValue("company_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Departments
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
        => Ok(await _service.GetDepartmentsAsync(CompanyId));

    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
        => Ok(await _service.CreateDepartmentAsync(CompanyId, request));

    // Meal types
    [HttpGet("meal-types")]
    public async Task<IActionResult> GetMealTypes()
        => Ok(await _service.GetMealTypesAsync(CompanyId));

    [HttpPost("meal-types")]
    public async Task<IActionResult> CreateMealType([FromBody] CreateMealTypeRequest request)
        => Ok(await _service.CreateMealTypeAsync(CompanyId, request));

    // Diners
    [HttpGet]
    public async Task<IActionResult> GetDiners()
        => Ok(await _service.GetDinersAsync(CompanyId));


    [HttpPost]
    public async Task<IActionResult> CreateDiner([FromBody] CreateDinerRequest request)
    {
        try { return Ok(await _service.CreateDinerAsync(CompanyId, request)); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDiner(Guid id, [FromBody] UpdateDinerRequest request)
    {
        var r = await _service.UpdateDinerAsync(CompanyId, id, request);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDiner(Guid id)
        => await _service.DeleteDinerAsync(CompanyId, id) ? NoContent() : NotFound();

    // Access validation
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateAccessRequest request)
        => Ok(await _service.ValidateAndRegisterAsync(CompanyId, UserId, request));

    // Consumptions
    [HttpGet("consumptions")]
    public async Task<IActionResult> GetConsumptions(
    [FromQuery] DateTime? from = null,
    [FromQuery] DateTime? to = null,
    CancellationToken ct = default)
    => Ok(await _service.GetConsumptionsAsync(CompanyId, from, to, ct));

    [HttpGet("consumptions/export")]
    public async Task<IActionResult> ExportConsumptions(
    [FromQuery] DateTime from,
    [FromQuery] DateTime to,
    [FromQuery] decimal? pricePerDay = null,
    CancellationToken ct = default)
    {
        if (to.Date < from.Date)
            return BadRequest(new { message = "El rango de fechas no es válido" });

        var bytes = await _service.ExportConsumptionsExcelAsync(
            CompanyId, from, to, pricePerDay, ct);

        var fileName = $"consumos_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx";
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }




    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDiner(Guid id)
    {
        var r = await _service.GetDinerByIdAsync(CompanyId, id);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpPost("checkin")]
    public async Task<ActionResult<CheckInResponse>> CheckIn(
    [FromBody] DeviceCheckInRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _service.CheckInFromDeviceAsync(
            CompanyId,
            UserId,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("tickets/pending")]
    public async Task<IActionResult> GetPendingTickets(CancellationToken ct)
    => Ok(await _service.GetPendingTicketsAsync(CompanyId, ct));

    [HttpPost("tickets/{consumptionId:guid}/printed")]
    public async Task<IActionResult> MarkPrinted(Guid consumptionId, CancellationToken ct)
    {
        var ok = await _service.MarkTicketPrintedAsync(CompanyId, consumptionId, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("tickets/{consumptionId:guid}")]
    public async Task<IActionResult> GetTicket(Guid consumptionId, CancellationToken ct)
    {
        var t = await _service.GetTicketByConsumptionIdAsync(CompanyId, consumptionId, ct);
        return t == null ? NotFound() : Ok(t);
    }

    [HttpPost("coupons/provisional")]
    public async Task<ActionResult<CheckInResponse>> CreateProvisionalCoupon(
    [FromBody] CreateProvisionalCouponRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _service.CreateProvisionalCouponAsync(
            CompanyId,
            UserId,
            request,
            cancellationToken);

        return Ok(result);
    }
}