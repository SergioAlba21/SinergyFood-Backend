namespace InsumoPro.Application.Common.Interfaces;

public interface ICurrentTenantService
{
    Guid? CompanyId { get; }
    Guid? UserId { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}