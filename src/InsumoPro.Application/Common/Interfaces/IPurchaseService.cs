using InsumoPro.Application.Features.Purchases.DTOs;

namespace InsumoPro.Application.Common.Interfaces;

public interface IPurchaseService
{
    Task<List<PurchaseOrderDto>> GetAllAsync(Guid companyId);
    Task<PurchaseOrderDto?> GetByIdAsync(Guid companyId, Guid id);
    Task<PurchaseOrderDto> CreateAsync(Guid companyId, Guid userId, CreatePurchaseOrderRequest request);
    Task<PurchaseOrderDto?> ReceiveAsync(Guid companyId, Guid userId, Guid id, ReceivePurchaseOrderRequest request);
    Task<bool> CancelAsync(Guid companyId, Guid id);
}