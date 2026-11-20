using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Mrp.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Infrastructure.Services;

public class MrpService : IMrpService
{
    private readonly ApplicationDbContext _context;

    public MrpService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MrpSuggestionDto>> GenerateSuggestionsAsync(Guid companyId)
    {
        // Limpiar sugerencias pendientes anteriores
        var old = await _context.MrpSuggestions
            .Where(s => s.CompanyId == companyId && s.Status == "pending")
            .ToListAsync();
        _context.MrpSuggestions.RemoveRange(old);

        // Productos activos tipo insumo o both
        var products = await _context.Products
            .Where(p => p.CompanyId == companyId && p.IsActive)
            .Where(p => p.Type == "raw_material" || p.Type == "both")
            .ToListAsync();

        // Stock actual agrupado por producto
        var stocks = await _context.ProductWarehouses
            .Where(pw => pw.CompanyId == companyId)
            .GroupBy(pw => pw.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Qty);

        // Compras pendientes (ordered / partial)
        var incoming = await _context.PurchaseOrderItems
            .Where(i => i.PurchaseOrder.CompanyId == companyId)
            .Where(i => i.PurchaseOrder.Status == "ordered" || i.PurchaseOrder.Status == "partial")
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Qty = g.Sum(x => x.QuantityOrdered - x.QuantityReceived)
            })
            .ToDictionaryAsync(x => x.ProductId, x => x.Qty);

        // Demanda real por producción abierta (draft/confirmed)
        var openProduction = await _context.ProductionOrders
            .Where(o => o.CompanyId == companyId)
            .Where(o => o.Status == "draft" || o.Status == "confirmed")
            .Include(o => o.Recipe)!.ThenInclude(r => r!.Items)
            .ToListAsync();

        var demandByProduct = new Dictionary<Guid, decimal>();

        foreach (var order in openProduction)
        {
            if (order.Recipe == null) continue;

            var factor = order.QuantityPlanned / (order.Recipe.YieldQuantity <= 0 ? 1 : order.Recipe.YieldQuantity);

            foreach (var item in order.Recipe.Items)
            {
                var need = item.Quantity * factor;
                if (demandByProduct.ContainsKey(item.ProductId))
                    demandByProduct[item.ProductId] += need;
                else
                    demandByProduct[item.ProductId] = need;
            }
        }

        var warehouseDefault = await _context.Warehouses
            .Where(w => w.CompanyId == companyId && w.IsDefault)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync();

        var suggestions = new List<MrpSuggestion>();

        foreach (var product in products)
        {
            var currentStock = stocks.TryGetValue(product.Id, out var s) ? s : 0;
            var incomingQty = incoming.TryGetValue(product.Id, out var inc) ? Math.Max(inc, 0) : 0;
            var demandOrders = demandByProduct.TryGetValue(product.Id, out var d) ? d : 0;
            var safetyStock = product.MinStock;

            // Disponible neto
            var available = currentStock + incomingQty;

            // Necesidad total
            var needed = demandOrders + safetyStock;

            var suggested = needed - available;

            if (suggested <= 0) continue;

            var reason = BuildReason(demandOrders, safetyStock, currentStock, incomingQty);

            suggestions.Add(new MrpSuggestion
            {
                CompanyId = companyId,
                ProductId = product.Id,
                WarehouseId = warehouseDefault,
                SuggestedQuantity = Math.Round(suggested, 3),
                DemandFromOrders = Math.Round(demandOrders, 3),
                DemandFromForecast = 0, // lo dejamos listo para fase 2
                CurrentStock = Math.Round(currentStock, 3),
                SafetyStock = Math.Round(safetyStock, 3),
                IncomingPurchases = Math.Round(incomingQty, 3),
                SuggestedOrderDate = DateTime.Now.Date,
                Reason = reason,
                Status = "pending"
            });
        }

        _context.MrpSuggestions.AddRange(suggestions);
        await _context.SaveChangesAsync();

        return await GetSuggestionsAsync(companyId, "pending");
    }

    public async Task<List<MrpSuggestionDto>> GetSuggestionsAsync(Guid companyId, string? status = "pending")
    {
        var query = _context.MrpSuggestions
            .Where(s => s.CompanyId == companyId)
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.Status == status);

        var list = await query
            .OrderByDescending(s => s.SuggestedQuantity)
            .ToListAsync();

        return list.Select(s => new MrpSuggestionDto
        {
            Id = s.Id,
            ProductId = s.ProductId,
            ProductName = s.Product.Name,
            Sku = s.Product.Sku,
            WarehouseId = s.WarehouseId,
            WarehouseName = s.Warehouse?.Name,
            SuggestedQuantity = s.SuggestedQuantity,
            DemandFromOrders = s.DemandFromOrders,
            DemandFromForecast = s.DemandFromForecast,
            CurrentStock = s.CurrentStock,
            SafetyStock = s.SafetyStock,
            IncomingPurchases = s.IncomingPurchases,
            SuggestedOrderDate = s.SuggestedOrderDate,
            Reason = s.Reason,
            Status = s.Status
        }).ToList();
    }

    public async Task<object> ConvertToPurchaseOrderAsync(
        Guid companyId,
        Guid userId,
        Guid suggestionId,
        ConvertSuggestionRequest request)
    {
        var suggestion = await _context.MrpSuggestions
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.Id == suggestionId && s.CompanyId == companyId)
            ?? throw new Exception("Sugerencia no encontrada");

        if (suggestion.Status != "pending")
            throw new Exception("La sugerencia ya fue procesada");

        var qty = request.Quantity ?? suggestion.SuggestedQuantity;
        if (qty <= 0) throw new Exception("Cantidad inválida");

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
            ExpectedDate = DateTime.Now.Date.AddDays(supplier.LeadTimeDays),
            Notes = $"Generada desde Mini-MRP: {suggestion.Product.Name}",
            CreatedBy = userId,
            Total = qty * request.UnitCost
        };

        order.Items.Add(new PurchaseOrderItem
        {
            ProductId = suggestion.ProductId,
            QuantityOrdered = qty,
            QuantityReceived = 0,
            UnitCost = request.UnitCost
        });

        _context.PurchaseOrders.Add(order);

        suggestion.Status = "converted";
        suggestion.PurchaseOrderId = order.Id;

        await _context.SaveChangesAsync();

        return new
        {
            message = "Sugerencia convertida a orden de compra",
            purchaseOrderId = order.Id,
            productName = suggestion.Product.Name,
            quantity = qty
        };
    }

    public async Task<bool> DismissAsync(Guid companyId, Guid suggestionId)
    {
        var suggestion = await _context.MrpSuggestions
            .FirstOrDefaultAsync(s => s.Id == suggestionId && s.CompanyId == companyId);

        if (suggestion == null) return false;

        suggestion.Status = "dismissed";
        await _context.SaveChangesAsync();
        return true;
    }

    private static string BuildReason(decimal demandOrders, decimal safety, decimal stock, decimal incoming)
    {
        var parts = new List<string>();

        if (demandOrders > 0)
            parts.Add($"Demanda producción: {demandOrders:0.###}");

        if (safety > 0)
            parts.Add($"Stock mínimo: {safety:0.###}");

        parts.Add($"Stock actual: {stock:0.###}");

        if (incoming > 0)
            parts.Add($"Compras entrantes: {incoming:0.###}");

        return string.Join(" | ", parts);
    }
}