using InsumoPro.Application.Features.Suppliers.DTOs;

namespace InsumoPro.Application.Common.Interfaces;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetAllAsync(Guid companyId);
    Task<SupplierDto?> GetByIdAsync(Guid companyId, Guid id);
    Task<SupplierDto> CreateAsync(Guid companyId, CreateSupplierRequest request);
    Task<SupplierDto?> UpdateAsync(Guid companyId, Guid id, UpdateSupplierRequest request);
    Task<bool> DeleteAsync(Guid companyId, Guid id);
}