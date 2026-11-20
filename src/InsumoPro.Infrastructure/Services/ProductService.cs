using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Products.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetAllAsync(Guid companyId)
    {
        return await _context.Products
            .Where(p => p.CompanyId == companyId)
            .Include(p => p.Unit)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                Description = p.Description,
                Type = p.Type,
                UnitId = p.UnitId,
                UnitName = p.Unit != null ? p.Unit.Name : null,
                CostAverage = p.CostAverage,
                PriceSale = p.PriceSale,
                MinStock = p.MinStock,
                HasExpiry = p.HasExpiry,
                IsActive = p.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid companyId, Guid id)
    {
        var p = await _context.Products
            .Include(x => x.Unit)
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

        if (p == null) return null;

        return new ProductDto
        {
            Id = p.Id,
            Sku = p.Sku,
            Name = p.Name,
            Description = p.Description,
            Type = p.Type,
            UnitId = p.UnitId,
            UnitName = p.Unit?.Name,
            CostAverage = p.CostAverage,
            PriceSale = p.PriceSale,
            MinStock = p.MinStock,
            HasExpiry = p.HasExpiry,
            IsActive = p.IsActive
        };
    }

    public async Task<ProductDto> CreateAsync(Guid companyId, CreateProductRequest request)
    {
        var product = new Product
        {
            CompanyId = companyId,
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            UnitId = request.UnitId,
            CostAverage = request.CostAverage,
            PriceSale = request.PriceSale,
            MinStock = request.MinStock,
            HasExpiry = request.HasExpiry,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(companyId, product.Id))!;
    }

    public async Task<ProductDto?> UpdateAsync(Guid companyId, Guid id, UpdateProductRequest request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

        if (product == null) return null;

        product.Sku = request.Sku;
        product.Name = request.Name;
        product.Description = request.Description;
        product.Type = request.Type;
        product.UnitId = request.UnitId;
        product.CostAverage = request.CostAverage;
        product.PriceSale = request.PriceSale;
        product.MinStock = request.MinStock;
        product.HasExpiry = request.HasExpiry;
        product.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(companyId, product.Id);
    }

    public async Task<bool> DeleteAsync(Guid companyId, Guid id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

        if (product == null) return false;

        product.IsActive = false; // Soft delete
        await _context.SaveChangesAsync();
        return true;
    }
}