using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Sales.DTOs;
using InsumoPro.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;
    public CustomersController(ICustomerService service) => _service = service;
    private Guid CompanyId => Guid.Parse(User.FindFirstValue("company_id")!);

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(CompanyId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        => Ok(await _service.CreateAsync(CompanyId, request));
}