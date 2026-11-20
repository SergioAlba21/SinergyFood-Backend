using InsumoPro.Application.Features.Diners.DTOs;

namespace InsumoPro.Application.Common.Interfaces;

public interface IDinerService
{
    // Departments
    Task<List<DepartmentDto>> GetDepartmentsAsync(Guid companyId);
    Task<DepartmentDto> CreateDepartmentAsync(Guid companyId, CreateDepartmentRequest request);

    // Meal types
    Task<List<MealTypeDto>> GetMealTypesAsync(Guid companyId);
    Task<MealTypeDto> CreateMealTypeAsync(Guid companyId, CreateMealTypeRequest request);

    // Diners
    Task<List<DinerDto>> GetDinersAsync(Guid companyId);
    Task<DinerDto?> GetDinerByIdAsync(Guid companyId, Guid id);
    Task<DinerDto> CreateDinerAsync(Guid companyId, CreateDinerRequest request);
    Task<DinerDto?> UpdateDinerAsync(Guid companyId, Guid id, UpdateDinerRequest request);
    Task<bool> DeleteDinerAsync(Guid companyId, Guid id);

    // Access / consumption
    Task<ValidateAccessResponse> ValidateAndRegisterAsync(Guid companyId, Guid userId, ValidateAccessRequest request);
    Task<List<DinerConsumptionDto>> GetConsumptionsAsync(Guid companyId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);

    Task<byte[]> ExportConsumptionsExcelAsync(
    Guid companyId,
    DateTime from,
    DateTime to,
    decimal? pricePerDay = null,
    CancellationToken ct = default);

    Task<CheckInResponse> CheckInFromDeviceAsync(
    Guid companyId,
    Guid? userId,
    DeviceCheckInRequest request,
    CancellationToken cancellationToken = default);

    Task<List<PendingTicketDto>> GetPendingTicketsAsync(Guid companyId, CancellationToken ct = default);
    Task<bool> MarkTicketPrintedAsync(Guid companyId, Guid consumptionId, CancellationToken ct = default);
    Task<PendingTicketDto?> GetTicketByConsumptionIdAsync(
    Guid companyId,
    Guid consumptionId,
    CancellationToken ct = default);

    Task<CheckInResponse> CreateProvisionalCouponAsync(
    Guid companyId,
    Guid? userId,
    CreateProvisionalCouponRequest request,
    CancellationToken cancellationToken = default);
}