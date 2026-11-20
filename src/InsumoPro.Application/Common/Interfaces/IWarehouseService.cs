using InsumoPro.Application.Features.Warehouses.DTOs;

namespace InsumoPro.Application.Common.Interfaces;

public interface IWarehouseService
{
    Task<List<WarehouseDto>> GetAllAsync(Guid companyId);
    Task<WarehouseDto?> GetByIdAsync(Guid companyId, Guid id);
    Task<WarehouseDto> CreateAsync(Guid companyId, CreateWarehouseRequest request);
    Task<WarehouseDto?> UpdateAsync(Guid companyId, Guid id, UpdateWarehouseRequest request);
    Task<bool> DeleteAsync(Guid companyId, Guid id);
}