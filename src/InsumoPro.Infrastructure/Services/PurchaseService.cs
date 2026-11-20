using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Purchases.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Infrastructure.Services;

public class PurchaseService : IPurchaseService
{
    private readonly ApplicationDbContext _context;

    public PurchaseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PurchaseOrderDto>> GetAllAsync(Guid companyId)
    {
        var orders = await _context.PurchaseOrders
            .Where(o => o.CompanyId == companyId)
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<PurchaseOrderDto?> GetByIdAsync(Guid companyId, Guid id)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId);

        return order == null ? null : MapToDto(order);
    }

    public async Task<PurchaseOrderDto> CreateAsync(Guid companyId, Guid userId, CreatePurchaseOrderRequest request)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId && s.CompanyId == companyId)
            ?? throw new Exception("Proveedor no encontrado");

        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId && w.CompanyId == companyId)
            ?? throw new Exception("Almacén no encontrado");

        var order = new PurchaseOrder
        {
            CompanyId = companyId,
            SupplierId = request.SupplierId,
            WarehouseId = request.WarehouseId,
            Status = "ordered",
            OrderDate = DateTime.Now.Date,
            ExpectedDate = request.ExpectedDate.HasValue
                ? DateTime.SpecifyKind(request.ExpectedDate.Value.Date, DateTimeKind.Utc)
                : DateTime.Now.Date.AddDays(supplier.LeadTimeDays),
            Notes = request.Notes,
            CreatedBy = userId
        };

        decimal total = 0;

        foreach (var item in request.Items)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.CompanyId == companyId)
                ?? throw new Exception($"Producto {item.ProductId} no encontrado");

            order.Items.Add(new PurchaseOrderItem
            {
                ProductId = item.ProductId,
                QuantityOrdered = item.QuantityOrdered,
                QuantityReceived = 0,
                UnitCost = item.UnitCost
            });

            total += item.QuantityOrdered * item.UnitCost;
        }

        order.Total = total;

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(companyId, order.Id))!;
    }

    public async Task<PurchaseOrderDto?> ReceiveAsync(Guid companyId, Guid userId, Guid id, ReceivePurchaseOrderRequest request)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId);

        if (order == null) return null;
        if (order.Status is "received" or "cancelled")
            throw new Exception("La orden no se puede recibir en su estado actual");

        if (order.WarehouseId == null)
            throw new Exception("La orden no tiene almacén asignado");

        foreach (var receiveItem in request.Items)
        {
            var orderItem = order.Items.FirstOrDefault(i => i.ProductId == receiveItem.ProductId)
                ?? throw new Exception($"El producto {receiveItem.ProductId} no está en la orden");

            if (receiveItem.QuantityReceived <= 0) continue;

            orderItem.QuantityReceived += receiveItem.QuantityReceived;

            // Entrada a stock
            var movement = new StockMovement
            {
                CompanyId = companyId,
                ProductId = receiveItem.ProductId,
                WarehouseId = order.WarehouseId.Value,
                Type = "purchase",
                Quantity = Math.Abs(receiveItem.QuantityReceived),
                UnitCost = orderItem.UnitCost,
                ReferenceType = "purchase_order",
                ReferenceId = order.Id,
                Notes = $"Recepción OC {order.Id}",
                CreatedBy = userId
            };
            _context.StockMovements.Add(movement);

            var stock = await _context.ProductWarehouses
                .FirstOrDefaultAsync(pw =>
                    pw.ProductId == receiveItem.ProductId &&
                    pw.WarehouseId == order.WarehouseId.Value);

            if (stock == null)
            {
                stock = new ProductWarehouse
                {
                    CompanyId = companyId,
                    ProductId = receiveItem.ProductId,
                    WarehouseId = order.WarehouseId.Value,
                    Quantity = receiveItem.QuantityReceived
                };
                _context.ProductWarehouses.Add(stock);
            }
            else
            {
                stock.Quantity += receiveItem.QuantityReceived;
            }

            // Actualizar costo promedio del producto
            var product = orderItem.Product;
            var totalQty = stock.Quantity;
            if (totalQty > 0)
            {
                var previousQty = totalQty - receiveItem.QuantityReceived;
                var totalValue = (product.CostAverage * previousQty) + (orderItem.UnitCost * receiveItem.QuantityReceived);
                product.CostAverage = totalValue / totalQty;
            }
        }

        // Estado de la orden
        var allReceived = order.Items.All(i => i.QuantityReceived >= i.QuantityOrdered);
        var anyReceived = order.Items.Any(i => i.QuantityReceived > 0);

        order.Status = allReceived ? "received" : anyReceived ? "partial" : order.Status;

        if (!string.IsNullOrWhiteSpace(request.Notes))
            order.Notes = request.Notes;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(companyId, order.Id);
    }

    public async Task<bool> CancelAsync(Guid companyId, Guid id)
    {
        var order = await _context.PurchaseOrders
            .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId);

        if (order == null) return false;
        if (order.Status == "received")
            throw new Exception("No se puede cancelar una orden ya recibida");

        order.Status = "cancelled";
        await _context.SaveChangesAsync();
        return true;
    }

    private static PurchaseOrderDto MapToDto(PurchaseOrder o) => new()
    {
        Id = o.Id,
        SupplierId = o.SupplierId,
        SupplierName = o.Supplier?.Name,
        WarehouseId = o.WarehouseId,
        WarehouseName = o.Warehouse?.Name,
        Status = o.Status,
        OrderDate = o.OrderDate,
        ExpectedDate = o.ExpectedDate,
        Total = o.Total,
        Notes = o.Notes,
        CreatedAt = o.CreatedAt,
        Items = o.Items.Select(i => new PurchaseOrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? "",
            QuantityOrdered = i.QuantityOrdered,
            QuantityReceived = i.QuantityReceived,
            UnitCost = i.UnitCost,
            LineTotal = i.QuantityOrdered * i.UnitCost
        }).ToList()
    };
}