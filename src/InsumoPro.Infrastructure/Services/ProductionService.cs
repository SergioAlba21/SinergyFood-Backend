using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Production.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Infrastructure.Services;

public class ProductionService : IProductionService
{
    private readonly ApplicationDbContext _context;

    public ProductionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductionOrderDto>> GetAllAsync(Guid companyId)
    {
        return await _context.ProductionOrders
            .Where(p => p.CompanyId == companyId)
            .Include(p => p.Product)
            .Include(p => p.Warehouse)
            .Include(p => p.Recipe)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<ProductionOrderDto?> GetByIdAsync(Guid companyId, Guid id)
    {
        var order = await _context.ProductionOrders
            .Include(p => p.Product)
            .Include(p => p.Warehouse)
            .Include(p => p.Recipe)
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

        return order == null ? null : MapToDto(order);
    }

    public async Task<ProductionOrderDto> CreateAsync(Guid companyId, Guid userId, CreateProductionOrderRequest request)
    {
        var recipe = await _context.Recipes
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == request.RecipeId && r.CompanyId == companyId)
            ?? throw new Exception("Receta no encontrada");

        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId && w.CompanyId == companyId)
            ?? throw new Exception("Almacén no encontrado");

        var order = new ProductionOrder
        {
            CompanyId = companyId,
            RecipeId = recipe.Id,
            ProductId = recipe.ProductId,
            WarehouseId = request.WarehouseId,
            QuantityPlanned = request.QuantityPlanned,
            QuantityProduced = 0,
            Status = "draft",
            ProductionDate = request.ProductionDate.HasValue
                                        ? DateTime.SpecifyKind(request.ProductionDate.Value.Date, DateTimeKind.Utc)
                                        : DateTime.Now.Date,
            Notes = request.Notes,
            CreatedBy = userId
        };

        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(companyId, order.Id))!;
    }

    public async Task<ProductionOrderDto?> CompleteAsync(Guid companyId, Guid userId, Guid id, CompleteProductionOrderRequest request)
    {
        var order = await _context.ProductionOrders
            .Include(o => o.Recipe)!.ThenInclude(r => r!.Items).ThenInclude(i => i.Product)
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId);

        if (order == null) return null;
        if (order.Status == "completed") throw new Exception("La orden ya está completada");
        if (order.Status == "cancelled") throw new Exception("La orden está cancelada");
        if (order.Recipe == null) throw new Exception("La orden no tiene receta asociada");

        var factor = request.QuantityProduced / order.Recipe.YieldQuantity;

        // 1. Descontar insumos
        foreach (var item in order.Recipe.Items)
        {
            var qtyToConsume = item.Quantity * factor;

            await ApplyStockMovement(
                companyId,
                userId,
                item.ProductId,
                order.WarehouseId,
                "production_out",
                -Math.Abs(qtyToConsume),
                item.Product.CostAverage,
                "production_order",
                order.Id,
                $"Consumo por producción {order.Id}"
            );
        }

        // 2. Entrar producto terminado
        await ApplyStockMovement(
            companyId,
            userId,
            order.ProductId,
            order.WarehouseId,
            "production_in",
            Math.Abs(request.QuantityProduced),
            null,
            "production_order",
            order.Id,
            $"Producción completada {order.Id}"
        );

        // 3. Actualizar orden
        order.QuantityProduced = request.QuantityProduced;
        order.Status = "completed";
        order.CompletedAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            order.Notes = request.Notes;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(companyId, order.Id);
    }

    public async Task<bool> CancelAsync(Guid companyId, Guid id)
    {
        var order = await _context.ProductionOrders
            .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId);

        if (order == null) return false;
        if (order.Status == "completed") throw new Exception("No se puede cancelar una orden completada");

        order.Status = "cancelled";
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task ApplyStockMovement(
        Guid companyId,
        Guid userId,
        Guid productId,
        Guid warehouseId,
        string type,
        decimal signedQuantity,
        decimal? unitCost,
        string referenceType,
        Guid referenceId,
        string notes)
    {
        var movement = new StockMovement
        {
            CompanyId = companyId,
            ProductId = productId,
            WarehouseId = warehouseId,
            Type = type,
            Quantity = signedQuantity,
            UnitCost = unitCost,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Notes = notes,
            CreatedBy = userId
        };

        _context.StockMovements.Add(movement);

        var stock = await _context.ProductWarehouses
            .FirstOrDefaultAsync(pw => pw.ProductId == productId && pw.WarehouseId == warehouseId);

        if (stock == null)
        {
            stock = new ProductWarehouse
            {
                CompanyId = companyId,
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = signedQuantity
            };
            _context.ProductWarehouses.Add(stock);
        }
        else
        {
            stock.Quantity += signedQuantity;
        }
    }

    private static ProductionOrderDto MapToDto(ProductionOrder p) => new()
    {
        Id = p.Id,
        RecipeId = p.RecipeId,
        RecipeName = p.Recipe?.Name,
        ProductId = p.ProductId,
        ProductName = p.Product?.Name ?? "",
        WarehouseId = p.WarehouseId,
        WarehouseName = p.Warehouse?.Name ?? "",
        QuantityPlanned = p.QuantityPlanned,
        QuantityProduced = p.QuantityProduced,
        Status = p.Status,
        ProductionDate = p.ProductionDate,
        Notes = p.Notes,
        CreatedAt = p.CreatedAt,
        CompletedAt = p.CompletedAt
    };
}