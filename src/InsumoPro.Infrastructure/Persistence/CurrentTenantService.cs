using System.Security.Claims;
using InsumoPro.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace InsumoPro.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CompanyId
    {
        get
        {
            var companyId = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue("company_id");

            return Guid.TryParse(companyId, out var id) ? id : null;
        }
    }

    public Guid? UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id) ? id : null;
        }
    }

    public string? Role => _httpContextAccessor.HttpContext?.User?
        .FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}