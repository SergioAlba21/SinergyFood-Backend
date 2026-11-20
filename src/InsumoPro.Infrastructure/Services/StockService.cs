using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Stock.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Infrastructure.Services;

public class StockService : IStockService
{
    private readonly ApplicationDbContext _context;

    public StockService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductStockDto>> GetStockAsync(Guid companyId, Guid? warehouseId = null)
    {
        var query = _context.ProductWarehouses
            .Where(pw => pw.CompanyId == companyId)
            .Include(pw => pw.Product)
            .Include(pw => pw.Warehouse)
            .AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(pw => pw.WarehouseId == warehouseId.Value);

        return await query.Select(pw => new ProductStockDto
        {
            ProductId = pw.ProductId,
            ProductName = pw.Product.Name,
            Sku = pw.Product.Sku,
            WarehouseId = pw.WarehouseId,
            WarehouseName = pw.Warehouse.Name,
            Quantity = pw.Quantity,
            MinStock = pw.Product.MinStock,
            IsBelowMin = pw.Quantity < pw.Product.MinStock
        }).ToListAsync();
    }

    public async Task<List<StockMovementDto>> GetMovementsAsync(Guid companyId, Guid? productId = null, Guid? warehouseId = null)
    {
        var query = _context.StockMovements
            .Where(m => m.CompanyId == companyId)
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .OrderByDescending(m => m.CreatedAt)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);

        if (warehouseId.HasValue)
            query = query.Where(m => m.WarehouseId == warehouseId.Value);

        return await query.Select(m => new StockMovementDto
        {
            Id = m.Id,
            ProductId = m.ProductId,
            ProductName = m.Product.Name,
            WarehouseId = m.WarehouseId,
            WarehouseName = m.Warehouse.Name,
            Type = m.Type,
            Quantity = m.Quantity,
            UnitCost = m.UnitCost,
            Notes = m.Notes,
            CreatedAt = m.CreatedAt
        }).ToListAsync();
    }

    public async Task<StockMovementDto> CreateMovementAsync(Guid companyId, Guid userId, CreateStockMovementRequest request)
    {
        // Validar producto y almacén
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.CompanyId == companyId)
            ?? throw new Exception("Producto no encontrado");

        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId && w.CompanyId == companyId)
            ?? throw new Exception("Almacén no encontrado");

        // Determinar si suma o resta
        var isInbound = request.Type is "purchase" or "production_in" or "adjustment_in" or "transfer_in";
        var signedQuantity = isInbound ? Math.Abs(request.Quantity) : -Math.Abs(request.Quantity);

        // Crear movimiento
        var movement = new StockMovement
        {
            CompanyId = companyId,
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            Type = request.Type,
            Quantity = signedQuantity,
            UnitCost = request.UnitCost,
            Notes = request.Notes,
            CreatedBy = userId
        };

        _context.StockMovements.Add(movement);

        // Actualizar o crear existencia
        var stock = await _context.ProductWarehouses
            .FirstOrDefaultAsync(pw => pw.ProductId == request.ProductId && pw.WarehouseId == request.WarehouseId);

        if (stock == null)
        {
            stock = new ProductWarehouse
            {
                CompanyId = companyId,
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                Quantity = signedQuantity
            };
            _context.ProductWarehouses.Add(stock);
        }
        else
        {
            stock.Quantity += signedQuantity;
        }

        // Actualizar costo promedio si es entrada con costo
        if (isInbound && request.UnitCost.HasValue && request.UnitCost > 0)
        {
            // Costo promedio simple ponderado
            var totalValue = (product.CostAverage * (stock.Quantity - signedQuantity)) + (request.UnitCost.Value * Math.Abs(signedQuantity));
            var totalQty = stock.Quantity;
            if (totalQty > 0)
                product.CostAverage = totalValue / totalQty;
        }

        await _context.SaveChangesAsync();

        return new StockMovementDto
        {
            Id = movement.Id,
            ProductId = movement.ProductId,
            ProductName = product.Name,
            WarehouseId = movement.WarehouseId,
            WarehouseName = warehouse.Name,
            Type = movement.Type,
            Quantity = movement.Quantity,
            UnitCost = movement.UnitCost,
            Notes = movement.Notes,
            CreatedAt = movement.CreatedAt
        };
    }
}