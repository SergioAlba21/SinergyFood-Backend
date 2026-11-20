using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Sales.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Infrastructure.Services;

public class SaleService : ISaleService
{
    private readonly ApplicationDbContext _context;
    public SaleService(ApplicationDbContext context) => _context = context;

    public async Task<List<SaleOrderDto>> GetAllAsync(Guid companyId)
    {
        var orders = await _context.SaleOrders
            .Where(o => o.CompanyId == companyId)
            .Include(o => o.Customer)
            .Include(o => o.Warehouse)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<SaleOrderDto?> GetByIdAsync(Guid companyId, Guid id)
    {
        var order = await _context.SaleOrders
            .Include(o => o.Customer)
            .Include(o => o.Warehouse)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId);

        return order == null ? null : MapToDto(order);
    }

    public async Task<SaleOrderDto> CreateAndConfirmAsync(Guid companyId, Guid userId, CreateSaleOrderRequest request)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId && w.CompanyId == companyId)
            ?? throw new Exception("Almacén no encontrado");

        var order = new SaleOrder
        {
            CompanyId = companyId,
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            Status = "confirmed",
            OrderDate = DateTime.Now.Date,
            Notes = request.Notes,
            CreatedBy = userId
        };

        decimal total = 0;

        foreach (var item in request.Items)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.CompanyId == companyId)
                ?? throw new Exception($"Producto {item.ProductId} no encontrado");

            var unitPrice = item.UnitPrice ?? product.PriceSale;
            var qty = item.Quantity;

            // Validar stock
            var stock = await _context.ProductWarehouses
                .FirstOrDefaultAsync(pw =>
                    pw.ProductId == product.Id &&
                    pw.WarehouseId == request.WarehouseId);

            var available = stock?.Quantity ?? 0;
            if (available < qty)
                throw new Exception($"Stock insuficiente de {product.Name}. Disponible: {available}");

            order.Items.Add(new SaleOrderItem
            {
                ProductId = product.Id,
                Quantity = qty,
                UnitPrice = unitPrice
            });

            total += qty * unitPrice;

            // Descontar stock
            stock!.Quantity -= qty;

            // Movimiento
            _context.StockMovements.Add(new StockMovement
            {
                CompanyId = companyId,
                ProductId = product.Id,
                WarehouseId = request.WarehouseId,
                Type = "sale",
                Quantity = -Math.Abs(qty),
                UnitCost = product.CostAverage,
                ReferenceType = "sale_order",
                ReferenceId = order.Id,
                Notes = $"Venta {product.Name}",
                CreatedBy = userId
            });
        }

        order.Total = total;
        _context.SaleOrders.Add(order);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(companyId, order.Id))!;
    }

    public async Task<bool> CancelAsync(Guid companyId, Guid id)
    {
        var order = await _context.SaleOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId);

        if (order == null) return false;
        if (order.Status == "cancelled") return true;
        if (order.WarehouseId == null) throw new Exception("La venta no tiene almacén");

        // Revertir stock
        foreach (var item in order.Items)
        {
            var stock = await _context.ProductWarehouses
                .FirstOrDefaultAsync(pw =>
                    pw.ProductId == item.ProductId &&
                    pw.WarehouseId == order.WarehouseId);

            if (stock != null)
                stock.Quantity += item.Quantity;

            _context.StockMovements.Add(new StockMovement
            {
                CompanyId = companyId,
                ProductId = item.ProductId,
                WarehouseId = order.WarehouseId.Value,
                Type = "adjustment_in",
                Quantity = Math.Abs(item.Quantity),
                ReferenceType = "sale_order_cancel",
                ReferenceId = order.Id,
                Notes = "Reverso por cancelación de venta"
            });
        }

        order.Status = "cancelled";
        await _context.SaveChangesAsync();
        return true;
    }

    private static SaleOrderDto MapToDto(SaleOrder o) => new()
    {
        Id = o.Id,
        CustomerId = o.CustomerId,
        CustomerName = o.Customer?.Name,
        WarehouseId = o.WarehouseId,
        WarehouseName = o.Warehouse?.Name,
        Status = o.Status,
        OrderDate = o.OrderDate,
        Total = o.Total,
        Notes = o.Notes,
        CreatedAt = o.CreatedAt,
        Items = o.Items.Select(i => new SaleOrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? "",
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            LineTotal = i.Quantity * i.UnitPrice
        }).ToList()
    };
}