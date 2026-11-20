using InsumoPro.Application.Features.Products.DTOs;

namespace InsumoPro.Application.Common.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(Guid companyId);
    Task<ProductDto?> GetByIdAsync(Guid companyId, Guid id);
    Task<ProductDto> CreateAsync(Guid companyId, CreateProductRequest request);
    Task<ProductDto?> UpdateAsync(Guid companyId, Guid id, UpdateProductRequest request);
    Task<bool> DeleteAsync(Guid companyId, Guid id);
}