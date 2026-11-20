using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Sales.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    public CustomerService(ApplicationDbContext context) => _context = context;

    public async Task<List<CustomerDto>> GetAllAsync(Guid companyId)
    {
        return await _context.Customers
            .Where(c => c.CompanyId == companyId)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                Email = c.Email,
                Notes = c.Notes,
                IsActive = c.IsActive
            }).ToListAsync();
    }

    public async Task<CustomerDto> CreateAsync(Guid companyId, CreateCustomerRequest request)
    {
        var entity = new Customer
        {
            CompanyId = companyId,
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Notes = request.Notes,
            IsActive = true
        };

        _context.Customers.Add(entity);
        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Phone = entity.Phone,
            Email = entity.Email,
            Notes = entity.Notes,
            IsActive = entity.IsActive
        };
    }
}