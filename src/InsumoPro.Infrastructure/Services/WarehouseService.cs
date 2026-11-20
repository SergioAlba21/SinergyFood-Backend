using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Warehouses.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Infrastructure.Services;

public class WarehouseService : IWarehouseService
{
    private readonly ApplicationDbContext _context;

    public WarehouseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WarehouseDto>> GetAllAsync(Guid companyId)
    {
        return await _context.Warehouses
            .Where(w => w.CompanyId == companyId)
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Name = w.Name,
                IsDefault = w.IsDefault,
                Address = w.Address,
                IsActive = w.IsActive
            })
            .ToListAsync();
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid companyId, Guid id)
    {
        var w = await _context.Warehouses
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

        if (w == null) return null;

        return new WarehouseDto
        {
            Id = w.Id,
            Name = w.Name,
            IsDefault = w.IsDefault,
            Address = w.Address,
            IsActive = w.IsActive
        };
    }

    public async Task<WarehouseDto> CreateAsync(Guid companyId, CreateWarehouseRequest request)
    {
        // Si se marca como default, quitar el default de los demás
        if (request.IsDefault)
        {
            var currentDefaults = await _context.Warehouses
                .Where(w => w.CompanyId == companyId && w.IsDefault)
                .ToListAsync();

            foreach (var item in currentDefaults)
                item.IsDefault = false;
        }

        var warehouse = new Warehouse
        {
            CompanyId = companyId,
            Name = request.Name,
            IsDefault = request.IsDefault,
            Address = request.Address,
            IsActive = true
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(companyId, warehouse.Id))!;
    }

    public async Task<WarehouseDto?> UpdateAsync(Guid companyId, Guid id, UpdateWarehouseRequest request)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && w.CompanyId == companyId);

        if (warehouse == null) return null;

        if (request.IsDefault && !warehouse.IsDefault)
        {
            var currentDefaults = await _context.Warehouses
                .Where(w => w.CompanyId == companyId && w.IsDefault)
                .ToListAsync();

            foreach (var item in currentDefaults)
                item.IsDefault = false;
        }

        warehouse.Name = request.Name;
        warehouse.IsDefault = request.IsDefault;
        warehouse.Address = request.Address;
        warehouse.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(companyId, warehouse.Id);
    }

    public async Task<bool> DeleteAsync(Guid companyId, Guid id)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && w.CompanyId == companyId);

        if (warehouse == null) return false;

        warehouse.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}