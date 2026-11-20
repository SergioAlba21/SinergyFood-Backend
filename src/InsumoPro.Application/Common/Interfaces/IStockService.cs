using InsumoPro.Application.Features.Stock.DTOs;

namespace InsumoPro.Application.Common.Interfaces;

public interface IStockService
{
    Task<List<ProductStockDto>> GetStockAsync(Guid companyId, Guid? warehouseId = null);
    Task<List<StockMovementDto>> GetMovementsAsync(Guid companyId, Guid? productId = null, Guid? warehouseId = null);
    Task<StockMovementDto> CreateMovementAsync(Guid companyId, Guid userId, CreateStockMovementRequest request);
}