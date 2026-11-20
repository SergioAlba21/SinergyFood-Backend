using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Suppliers.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly ApplicationDbContext _context;

    public SupplierService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupplierDto>> GetAllAsync(Guid companyId)
    {
        return await _context.Suppliers
            .Where(s => s.CompanyId == companyId)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactName = s.ContactName,
                Phone = s.Phone,
                Email = s.Email,
                LeadTimeDays = s.LeadTimeDays,
                Notes = s.Notes,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<SupplierDto?> GetByIdAsync(Guid companyId, Guid id)
    {
        var s = await _context.Suppliers
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

        if (s == null) return null;

        return new SupplierDto
        {
            Id = s.Id,
            Name = s.Name,
            ContactName = s.ContactName,
            Phone = s.Phone,
            Email = s.Email,
            LeadTimeDays = s.LeadTimeDays,
            Notes = s.Notes,
            IsActive = s.IsActive
        };
    }

    public async Task<SupplierDto> CreateAsync(Guid companyId, CreateSupplierRequest request)
    {
        var supplier = new Supplier
        {
            CompanyId = companyId,
            Name = request.Name,
            ContactName = request.ContactName,
            Phone = request.Phone,
            Email = request.Email,
            LeadTimeDays = request.LeadTimeDays,
            Notes = request.Notes,
            IsActive = true
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(companyId, supplier.Id))!;
    }

    public async Task<SupplierDto?> UpdateAsync(Guid companyId, Guid id, UpdateSupplierRequest request)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId);

        if (supplier == null) return null;

        supplier.Name = request.Name;
        supplier.ContactName = request.ContactName;
        supplier.Phone = request.Phone;
        supplier.Email = request.Email;
        supplier.LeadTimeDays = request.LeadTimeDays;
        supplier.Notes = request.Notes;
        supplier.IsActive = request.IsActive;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(companyId, supplier.Id);
    }

    public async Task<bool> DeleteAsync(Guid companyId, Guid id)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId);

        if (supplier == null) return false;

        supplier.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}