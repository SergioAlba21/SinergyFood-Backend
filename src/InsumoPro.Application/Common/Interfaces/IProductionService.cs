using InsumoPro.Application.Features.Production.DTOs;

namespace InsumoPro.Application.Common.Interfaces;

public interface IProductionService
{
    Task<List<ProductionOrderDto>> GetAllAsync(Guid companyId);
    Task<ProductionOrderDto?> GetByIdAsync(Guid companyId, Guid id);
    Task<ProductionOrderDto> CreateAsync(Guid companyId, Guid userId, CreateProductionOrderRequest request);
    Task<ProductionOrderDto?> CompleteAsync(Guid companyId, Guid userId, Guid id, CompleteProductionOrderRequest request);
    Task<bool> CancelAsync(Guid companyId, Guid id);
}