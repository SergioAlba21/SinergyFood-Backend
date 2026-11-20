using InsumoPro.Application.Features.Sales.DTOs;

namespace InsumoPro.Application.Common.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync(Guid companyId);
    Task<CustomerDto> CreateAsync(Guid companyId, CreateCustomerRequest request);
}

public interface ISaleService
{
    Task<List<SaleOrderDto>> GetAllAsync(Guid companyId);
    Task<SaleOrderDto?> GetByIdAsync(Guid companyId, Guid id);
    Task<SaleOrderDto> CreateAndConfirmAsync(Guid companyId, Guid userId, CreateSaleOrderRequest request);
    Task<bool> CancelAsync(Guid companyId, Guid id);
}